using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ESOAPIExplorer.Services;

public class ESODocumentationService : IESODocumentationService
{
    private string FileName { get; set; }
    private string CurrentLine { get; set; }
    private ICollection<EsoUIEnumValue> CurrentEnum { get; set; }
    private EsoUIFunction CurrentFunction { get; set; }
    private EsoUIObject CurrentObject { get; set; }
    // private EsoUIXMLElement CurrentElement { get; set; }
    private ReaderState State { get; set; }
    private readonly ApplicationDataContainer _Settings = ApplicationData.Current.LocalSettings;
    private readonly FileOpenPicker _FilePicker;
    private readonly ILuaObjectScanner _LuaObjectScanner;
    private readonly IRegexService _RegexService;

    public EsoUIDocumentation Documentation { get; set; }
    // public EsoUIDocumentation Data { get; set; }

    public ESODocumentationService(ILuaObjectScanner luaObjectScanner, IRegexService regexService)
    {
        _FilePicker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        // Get the current window's HWND by passing in the Window object
        Window _MainWindow = (Window)Application.Current.GetType().GetProperty("MainWindow").GetValue(Application.Current);
        nint hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_MainWindow);

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(_FilePicker, hwnd);

        _FilePicker.FileTypeFilter.Add(".txt");
        _LuaObjectScanner = luaObjectScanner;
        _RegexService = regexService;
    }

    public async Task InitialiseAsync()
    {
        string path = $"{ApplicationData.Current.LocalFolder.Path}\\apiCache.br";

        try
        {
            // do we have cached data
            if (File.Exists(path))
            {
                using (FileStream cacheFile = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (MemoryStream data = new MemoryStream())
                    {
                        using (BrotliStream decompressor = new BrotliStream(cacheFile, CompressionMode.Decompress))
                        {
                            decompressor.CopyTo(data);
                        }

                        byte[] byteData = data.ToArray();
                        Documentation = JsonSerializer.Deserialize<EsoUIDocumentation>(byteData);
                    };
                };
            }
            else
            {
                Documentation = await GetDocumentationAsync();

                if (Documentation != null)
                {
                    // cache the documentation
                    byte[] data = JsonSerializer.SerializeToUtf8Bytes(Documentation);

                    using (FileStream cacheFile = new FileStream(path, FileMode.Create))
                    {
                        using (BrotliStream compressor = new BrotliStream(cacheFile, CompressionLevel.Fastest))
                        {
                            compressor.Write(data, 0, data.Length);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private ConcurrentDictionary<string, string> GetEsoStrings(string path)
    {
        ConcurrentDictionary<string, string> lookup = [];
        BlockingCollection<string> lines = [];

        Task readLines = Task.Run(() =>
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            lines.CompleteAdding();
        });


        Task processLines = Task.Run(() =>
        {
            Parallel.ForEach(lines.GetConsumingEnumerable(), line =>
            {
                Match matches = _RegexService.EsoStringMatcher().Match(line);

                if (matches.Success)
                {
                    lookup.TryAdd(matches.Groups[2].Value, matches.Groups[1].Value);
                }
            });
        });

        Task.WaitAll(readLines, processLines);

        return lookup;
    }

    private ConcurrentDictionary<string, string> GetLocaleStrings(string path)
    {
        BlockingCollection<string> lines = [];
        ConcurrentDictionary<string, string> lookup = [];

        Task readLines = Task.Run(() =>
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            lines.CompleteAdding();
        });


        Task processLines = Task.Run(() =>
        {
            Parallel.ForEach(lines.GetConsumingEnumerable(), line =>
            {
                Match matches = _RegexService.LocaleStringMatcher().Match(line);

                if (matches.Success)
                {
                    lookup.TryAdd(matches.Groups[1].Value, matches.Groups[2].Value);
                }
            });
        });

        Task.WaitAll(readLines, processLines);

        return lookup;
    }

    private async Task<EsoUIDocumentation> GetDocumentationAsync()
    {
        string path = await GetPathAsync();
        string directoryPath = Path.GetDirectoryName(path);
        string ingamePath = $"{directoryPath}\\esoui\\ingamelocalization\\LocalizeGeneratedStrings.lua";
        string localePath = $"{directoryPath}\\esoui\\lang\\en_client.lua";

        EsoUIDocumentation documentation = await ParseAsync(path);
        _LuaObjectScanner.FolderPath = directoryPath;
        _LuaObjectScanner.ScanFolderForLuaFunctions();
        LuaScanResults luaobjects = _LuaObjectScanner.Results;

        Parallel.ForEach(luaobjects.Functions, func => documentation.Functions.TryAdd(func.Name, func));
        Parallel.ForEach(luaobjects.Globals, global => documentation.Globals.TryAdd(global.Name, [new EsoUIEnumValue(global.Name, null)]));
        Parallel.ForEach(luaobjects.Objects, obj => documentation.Objects.TryAdd(obj.Name, obj));

        // lookups
        ConcurrentDictionary<string, string> esoStrings = GetEsoStrings(ingamePath);
        ConcurrentDictionary<string, string> localeStrings = GetLocaleStrings(localePath);

        documentation.SI_Lookup = esoStrings
            .Concat(localeStrings)
            .GroupBy(kvp  => kvp.Key)
            .ToDictionary(global => global.Key, global => global.First().Value);

        esoStrings.Clear();
        localeStrings.Clear();

        // find constants
        ConcurrentDictionary<string, EsoUIEnumValue> otherGlobals = [];

        List<string> globalKeys = documentation.Globals
            .SelectMany(item => item.Value.Select(detail => detail.Name))
            .Concat(documentation.Globals.SelectMany(item => item.Value.Select(detail => item.Key)))
            .ToList();

        Parallel.ForEach(ConstantValues.Values, kvp =>
        {
            if (!globalKeys.Contains(kvp.Key))
            {
                EsoUIEnumValue global = new EsoUIEnumValue(kvp.Key, kvp.Value);
                EsoUIGlobalValue value = new EsoUIGlobalValue
                {
                    Code = $"{global.Name} = {global.Value}",
                    DisplayValue = global.Value
                };

                switch (global.Type)
                {
                    case "number":
                        if (double.TryParse(global.Value, out double doubleValue)) { value.DoubleValue = doubleValue; }
                        break;
                    case "integer":
                        if (int.TryParse(global.Value, out int intValue)) { value.IntValue = intValue; }
                        break;
                    default:

                        value.StringValue = global.Value;
                        break;
                }

                documentation.Constants.TryAdd(global.Name, new EsoUIConstantValue(global.Name, value));
            }
        });

        return documentation;
    }

    private async Task<string> GetPathAsync()
    {
        string path = _Settings.Values["last path"] as string;

        if (string.IsNullOrEmpty(path))
        {
            StorageFile file = await _FilePicker.PickSingleFileAsync();

            if (file != null)
            {
                _Settings.Values["last path"] = file.Path;
                path = file.Path;
            }
        }
        else if (!File.Exists(path))
        {
            _Settings.Values["last path"] = null;
            path = await GetPathAsync();
        }

        return path;
    }

    private bool LineStartsWith(string prefix) => CurrentLine.StartsWith(prefix);

    private string GetFirstMatch(Regex pattern)
    {
        MatchCollection matches = pattern.Matches(CurrentLine);

        if (matches.Count >= 1)
        {
            return matches[0].Groups[1].Value;
        }
        else
        {
            throw new Exception($"No matches for pattern \"{pattern}\" in line \"{CurrentLine}\".");
        }
    }

    private List<string> GetMatches(Regex pattern)
    {
        MatchCollection matches = pattern.Matches(CurrentLine);
        List<string> result = [];

        for (int i = 1; i < matches[0].Groups.Count; i++)
        {
            result.Add(matches[0].Groups[i].Value);
        }

        return result;
    }

    private ReaderState FindNextState()
    {
        switch (true)
        {
            case true when LineStartsWith("{TOC:maxLevel"):
                return ReaderState.READ_API_VERSION;
            case true when LineStartsWith("{TOC:maxLevel"):
                return ReaderState.READ_API_VERSION;
            case true when LineStartsWith("h2. VM Functions"):
            case true when LineStartsWith("h2. ZOS helper functions"):
                return ReaderState.READ_VM_FUNCTIONS;
            case true when LineStartsWith("h2. Global Variables"):
                return ReaderState.READ_GLOBALS;
            case true when LineStartsWith("h2. Game API"):
                return ReaderState.READ_GAME_API;
            case true when LineStartsWith("h2. Object API"):
                return ReaderState.READ_OBJECT_API;
            case true when LineStartsWith("h2. Events"):
                return ReaderState.READ_EVENTS;
            //case true when LineStartsWith("h2. UI XML Layout"):
            //    return ReaderState.READ_XML_ATTRIBUTES;
            //case true when (State == ReaderState.READ_XML_ATTRIBUTES):
            //    return ReaderState.READ_XML_LAYOUT;
            default:
                return ReaderState.UNDETERMINED;
        }
    }

    private bool ReadApiVersion()
    {
        if (LineStartsWith("h1. "))
        {
            Documentation.ApiVersion = int.Parse(GetFirstMatch(_RegexService.ApiVersionMatcher()));

            return true;
        }

        return false;
    }

    private ICollection<EsoUIEnumValue> GetOrCreateGlobal(string name)
    {
        if (!Documentation.Globals.TryGetValue(name, out ICollection<EsoUIEnumValue> value))
        {
            value = [];
            Documentation.Globals[name] = value;
        }

        return value;
    }

    private bool ReadGlobals()
    {
        switch (true)
        {
            case true when LineStartsWith("h2. "):
                return true;
            case true when LineStartsWith("h5. "):
                string enumName = GetFirstMatch(_RegexService.EnumNameMatcher());
                CurrentEnum = GetOrCreateGlobal(enumName);
                break;
            case true when LineStartsWith("* "):
                string enumValue = GetFirstMatch(_RegexService.EnumMatcher());
                CurrentEnum.Add(new EsoUIEnumValue(enumValue, ConstantValues.GetConstantValue(enumValue)));
                break;
            default:
                break;
        }

        return false;
    }

    private List<EsoUIArgument> ParseArgs(string args)
    {
        List<EsoUIArgument> data = [];
        string[] argsArray = args.Split(',');

        int argId = 1;

        foreach (string arg in argsArray)
        {
            Match match = _RegexService.ArgumentMatcher().Match(arg);
            data.Add(new EsoUIArgument(match.Groups[2].Value, new EsoUIType(match.Groups[1].Value), argId));
            argId++;
        }

        return data;
    }

    private bool ReadGameApi()
    {
        if (LineStartsWith("h2. "))
        {
            return true;
        }

        ReadFunction(Documentation.Functions);

        return false;
    }

    private void ReadFunction(ConcurrentDictionary<string, EsoUIFunction> functions)
    {
        switch (true)
        {
            case true when LineStartsWith("* "):
                List<string> matches = GetMatches(_RegexService.FunctionNameMatcher());
                Match nameMatch = _RegexService.FunctionAccessMatcher().Match(matches[0]);
                string name = nameMatch.Groups[1].Value;
                string access = nameMatch.Groups[3].Value;
                EsoUIFunctionAccess accessLevel = access == string.Empty ? EsoUIFunctionAccess.PUBLIC : access.ToEsoUIFunctionAccess();

                CurrentFunction = new EsoUIFunction(name, accessLevel);
                CurrentFunction.AddCode(CurrentLine);

                if (!string.IsNullOrEmpty(matches[1]))
                {
                    CurrentFunction.Args = ParseArgs(matches[1]);
                }

                functions[name] = CurrentFunction;
                break;
            case true when LineStartsWith("** _Uses variable returns"):
                CurrentFunction.HasVariableReturns = true;
                CurrentFunction.AddCode(CurrentLine);
                break;
            case true when LineStartsWith("** _Returns:_"):
                string args = GetFirstMatch(_RegexService.FunctionReturnMatcher());
                CurrentFunction.Returns = ParseArgs(args);
                CurrentFunction.AddCode(CurrentLine);
                break;
        }
    }

    private EsoUIObject GetOrCreateObject(string name)
    {
        if (!Documentation.Objects.TryGetValue(name, out EsoUIObject value))
        {
            value = new EsoUIObject(name, true);
            value.AddCode(CurrentLine);
            Documentation.Objects[name] = value;
        }

        return value;
    }

    private bool ReadObjectApi()
    {
        switch (true)
        {
            case true when LineStartsWith("h2. "):
                return true;
            case true when LineStartsWith("h3. "):
                string objectName = GetFirstMatch(_RegexService.ObjectNameMatcher());
                CurrentObject = GetOrCreateObject(objectName);
                break;
            case true when LineStartsWith("* "):
                CurrentObject.AddCode(CurrentLine);
                ReadFunction(CurrentObject.Functions);
                CurrentFunction.Parent = CurrentObject.Name;
                break;
            case true when LineStartsWith("** _Uses variable returns"):
                CurrentObject.AddCode(CurrentLine);
                CurrentFunction.HasVariableReturns = true;
                CurrentFunction.AddCode(CurrentLine);
                break;
            case true when LineStartsWith("** _Returns:_"):
                string args = GetFirstMatch(_RegexService.FunctionReturnMatcher());
                CurrentObject.AddCode(CurrentLine);
                CurrentFunction.Returns = ParseArgs(args);
                CurrentFunction.AddCode(CurrentLine);
                break;
            default:
                break;
        }

        return false;
    }

    private bool ReadEvents()
    {
        switch (true)
        {
            case true when LineStartsWith("h2. "):
                // section ended
                return true;
            case true when LineStartsWith("* "):
                List<string> matches = GetMatches(_RegexService.EventMatcher());
                string name = matches[0];
                string argsString = matches[2];

                List<EsoUIArgument> args;

                if (string.IsNullOrEmpty(argsString))
                {
                    args = ParseArgs("*integer* _eventId_");
                }
                else
                {
                    args = ParseArgs("*integer* _eventId_," + argsString);
                }

                Documentation.Events[name] = new EsoUIEvent(name, args);
                Documentation.Events[name].AddCode(CurrentLine);

                return false;
            default:
                return false;
        }
    }

    //private bool ReadXmlAttributes()
    //{
    //    if (LineStartsWith("h4. Attributes"))
    //    {
    //        // ignore the header
    //        return false;
    //    }
    //    else if (!LineStartsWith("* "))
    //    {
    //        // section ended
    //        return true;
    //    }

    //    List<string> matches = GetMatches(_RegexService.XMLAttributeMatcher());
    //    string name = matches[0];
    //    string type = matches[1];
    //    Documentation.XmlAttributes[name] = new EsoUIArgument(name, new EsoUIType(type), 1);

    //    return false;
    //}

    private EsoUIXMLElement GetOrCreateElement(string name)
    {
        if (!Documentation.XmlLayout.TryGetValue(name, out EsoUIXMLElement value))
        {
            value = new EsoUIXMLElement(name);
            Documentation.XmlLayout[name] = value;
        }

        return value;
    }

    //private bool ReadXmlLayout()
    //{
    //    switch (true)
    //    {
    //        case true when LineStartsWith("h5. sentinel_element"):
    //            // section ended
    //            return true;
    //        case true when LineStartsWith("h5. "):
    //            string elementName = GetFirstMatch(_RegexService.XMLElementNameMatcher());
    //            CurrentElement = GetOrCreateElement(elementName);
    //            break;
    //        case true when LineStartsWith("* _attr"):
    //            string attribute = GetFirstMatch(_RegexService.XMLAttributeTypeMatcher());
    //            Match match = _RegexService.XMLAttributeNameMatcher().Match(attribute);
    //            string type = match.Groups[1].Value;
    //            string name = match.Groups[2].Value;
    //            CurrentElement.Attributes.Add(new EsoUIArgument(name, new EsoUIType(type), 1));
    //            break;
    //        case true when LineStartsWith("* ScriptArguments"):
    //            CurrentElement.Documentation = GetFirstMatch(_RegexService.XMLScriptArgumentMatcher());
    //            break;
    //        case true when LineStartsWith("* ["):
    //            {
    //                List<string> matches = GetMatches(_RegexService.XMLLineTypeMatcher());
    //                string lineType = matches[0];
    //                string lname = matches[1];
    //                string ltype = matches[2];
    //                switch (lineType)
    //                {
    //                    case "Child":
    //                        if (ltype == "Attributes")
    //                        {
    //                            CurrentElement.Attributes.Add(Documentation.XmlAttributes[lname]);
    //                        }
    //                        else
    //                        {
    //                            CurrentElement.Children.Add(new EsoUIType(lname, ltype));
    //                        }
    //                        break;
    //                    case "Inherits":
    //                        CurrentElement.Parent = new EsoUIType(ltype);
    //                        break;
    //                    default:
    //                        Console.WriteLine("Unhandled prefix: " + lineType + " - " + CurrentLine);
    //                        break;
    //                }
    //                break;
    //            }
    //    }

    //    return false;
    //}

    private void InjectCustomData()
    {
        EsoUIFunction registerForEvent = new EsoUIFunction("RegisterForEvent");
        registerForEvent.AddArgument("namespace", "string");
        registerForEvent.AddArgument("event", "integer");
        registerForEvent.AddArgument("callback", "function");
        registerForEvent.AddReturn("success", "bool");

        EsoUIFunction registerForAllEvents = new EsoUIFunction("RegisterForAllEvents");
        registerForAllEvents.AddArgument("namespace", "string");
        registerForAllEvents.AddArgument("callback", "function");

        EsoUIFunction unregisterForEvent = new EsoUIFunction("UnregisterForEvent");
        unregisterForEvent.AddArgument("namespace", "string");
        unregisterForEvent.AddArgument("event", "integer");
        unregisterForEvent.AddReturn("success", "bool");

        EsoUIFunction addFilterForEvent = new EsoUIFunction("AddFilterForEvent");
        addFilterForEvent.AddArgument("namespace", "string");
        addFilterForEvent.AddArgument("event", "integer");
        addFilterForEvent.AddArgument("filterType", "RegisterForEventFilterType");
        addFilterForEvent.AddArgument("filterValue");
        addFilterForEvent.AddArgument("...");
        addFilterForEvent.AddReturn("success", "bool");

        EsoUIFunction registerForUpdate = new EsoUIFunction("RegisterForUpdate");
        registerForUpdate.AddArgument("namespace", "string");
        registerForUpdate.AddArgument("interval", "integer");
        registerForUpdate.AddArgument("callback", "function");
        registerForUpdate.AddReturn("success", "bool");

        EsoUIFunction unregisterForUpdate = new EsoUIFunction("UnregisterForUpdate");
        unregisterForUpdate.AddArgument("namespace", "string");
        unregisterForUpdate.AddReturn("success", "bool");

        EsoUIObject eventManager = new EsoUIObject("EventManager");
        eventManager.AddFunction(registerForEvent);
        eventManager.AddFunction(registerForAllEvents);
        eventManager.AddFunction(unregisterForEvent);
        eventManager.AddFunction(addFilterForEvent);
        eventManager.AddFunction(registerForUpdate);
        eventManager.AddFunction(unregisterForUpdate);

        EsoUIFunction getWindowManager = new EsoUIFunction("GetWindowManager");
        getWindowManager.AddReturn("windowManager", "WindowManager");

        EsoUIFunction getAnimationManager = new EsoUIFunction("GetAnimationManager");
        getAnimationManager.AddReturn("animationManager", "AnimationManager");

        EsoUIFunction getEventManager = new EsoUIFunction("GetEventManager");
        getEventManager.AddReturn("eventManager", "EventManager");

        EsoUIFunction getAddOnManager = new EsoUIFunction("GetAddOnManager");
        getAddOnManager.AddReturn("addOnManager", "AddOnManager");

        Documentation.Objects["EventManager"] = eventManager;
        Documentation.Functions["GetWindowManager"] = getWindowManager;
        Documentation.Functions["GetAnimationManager"] = getAnimationManager;
        Documentation.Functions["GetEventManager"] = getEventManager;
        Documentation.Functions["GetAddOnManager"] = getAddOnManager;
    }

    private Task<EsoUIDocumentation> ParseFileAsync()
    {
        return Task.Run(() =>
        {
            Documentation = new EsoUIDocumentation();
            // 5555555555555555555555555555555555555555InjectCustomData();
            State = ReaderState.UNDETERMINED;

            if (!string.IsNullOrEmpty(FileName))
            {
                using (StreamReader reader = new StreamReader(FileName))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        OnReadLine(line);
                    }
                }

                return Documentation;
            }
            else
            {
                return null;
            }
        });
    }

    private Task<EsoUIDocumentation> ParseAsync(string fileName)
    {
        FileName = fileName;

        return ParseFileAsync();
    }

    private void OnReadLine(string line)
    {
        CurrentLine = line;
        bool finished = false;

        switch (State)
        {
            case ReaderState.READ_API_VERSION:
                finished = ReadApiVersion();
                break;
            case ReaderState.READ_GLOBALS:
                finished = ReadGlobals();
                break;
            case ReaderState.READ_VM_FUNCTIONS:
            case ReaderState.READ_GAME_API:
                finished = ReadGameApi();
                break;
            case ReaderState.READ_OBJECT_API:
                finished = ReadObjectApi();
                break;
            case ReaderState.READ_EVENTS:
                finished = ReadEvents();
                break;
            //case ReaderState.READ_XML_ATTRIBUTES:
            //    finished = ReadXmlAttributes();
            //    break;
            //case ReaderState.READ_XML_LAYOUT:
            //    finished = ReadXmlLayout();
            //    break;
            case ReaderState.UNDETERMINED:
            default:
                State = FindNextState();
                break;
        }

        if (finished)
        {
            State = FindNextState();
            CurrentEnum = null;
            CurrentFunction = null;
            CurrentObject = null;
            // CurrentElement = null;
        }
    }
}
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
    private ReaderState State { get; set; }
    public bool UseCache { get; set; } = true;

    private readonly ApplicationDataContainer _Settings = ApplicationData.Current.LocalSettings;
    private readonly FileOpenPicker _FilePicker;
    private readonly ILuaObjectScanner _LuaObjectScanner;
    private readonly IRegexService _RegexService;

    public EsoUIDocumentation Documentation { get; set; }

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
        string path = $"{ApplicationData.Current.LocalCacheFolder.Path}\\apiCache.br";

#if DEBUG
        UseCache = false;
#endif
        try
        {
            // do we have cached data
            if (UseCache && File.Exists(path))
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
                    if (UseCache)
                    {
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
        _LuaObjectScanner.ScanFolderForLuaFunctions(documentation.Objects.First(o => o.Value.Name == "ZO_CallbackObject").Value);
        LuaScanResults luaobjects = _LuaObjectScanner.Results;

        var dpet = luaobjects.Objects.Where(o => o.InstanceName.StartsWith("PROMOTIONAL")).FirstOrDefault();
        var dpeto = luaobjects.Objects.Where(o => o.Name.Contains("Promotional")).FirstOrDefault();

        Parallel.ForEach(luaobjects.Functions, func => documentation.Functions.TryAdd(func.Name, func));
        Parallel.ForEach(luaobjects.InstanceNames, name => documentation.InstanceNames.TryAdd(name.Name, name));
        Parallel.ForEach(luaobjects.Objects, obj => documentation.Objects.TryAdd(obj.Name, obj));
        Parallel.ForEach(luaobjects.Fragments, fragment => documentation.Fragments.Add(fragment.Key));

        // lookups
        ConcurrentDictionary<string, string> esoStrings = GetEsoStrings(ingamePath);
        ConcurrentDictionary<string, string> localeStrings = GetLocaleStrings(localePath);

        documentation.SI_Lookup = esoStrings
            .Concat(localeStrings)
            .GroupBy(kvp => kvp.Key)
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

    private void AddManagers()
    {
        Documentation.Objects["WINDOW_MANAGER"] = Documentation.Objects["WindowManager"];
        Documentation.Objects["ANIMATION_MANAGER"] = Documentation.Objects["AnimationManager"];
        Documentation.Objects["EVENT_MANAGER"] = Documentation.Objects["EventManager"];
    }

    private void AddCustomData()
    {
        #region event manager
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
        #endregion

        EsoUIFunction getWindowManager = new EsoUIFunction("GetWindowManager");
        getWindowManager.AddReturn("windowManager", "WindowManager");
        getWindowManager.AddCode("* GetWindowManager()");
        getWindowManager.AddCode("** _Returns:_ *object* _windowManager_");

        EsoUIFunction getAnimationManager = new EsoUIFunction("GetAnimationManager");
        getAnimationManager.AddReturn("animationManager", "AnimationManager");
        getAnimationManager.AddCode("* GetAnimationManager()");
        getAnimationManager.AddCode("** _Returns:_ *object* _apRetAnimationManager_");

        EsoUIFunction getEventManager = new EsoUIFunction("GetEventManager");
        getEventManager.AddReturn("eventManager", "EventManager");
        getEventManager.AddCode("* GetEventManager()");
        getEventManager.AddCode("** _Returns:_ *object* _eventManager_");

        EsoUIFunction getAddOnManager = new EsoUIFunction("GetAddOnManager");
        getAddOnManager.AddReturn("addOnManager", "AddOnManager");
        getAddOnManager.AddCode("* GetAddOnManager()");
        getAddOnManager.AddCode("** _Returns:_ *object* _addOnManager_");

        #region callback manager
        EsoUIFunction registerCallback = new EsoUIFunction("RegisterCallback");
        registerCallback.AddArgument("eventName", "string");
        registerCallback.AddArgument("callback", "function");
        registerCallback.AddArgument("arg");

        EsoUIFunction unregisterAllCallbacks = new EsoUIFunction("UnregisterAllCallbacks");
        unregisterAllCallbacks.AddArgument("eventName", "string");

        EsoUIFunction setHandleOnce = new EsoUIFunction("SetHandleOnce");
        setHandleOnce.AddArgument("handleOnce", "boolean");

        EsoUIFunction fireCallbacks = new EsoUIFunction("FireCallbacks");
        fireCallbacks.AddArgument("eventName", "string");
        fireCallbacks.AddArgument("...");

        EsoUIFunction clean = new EsoUIFunction("Clean");
        clean.AddArgument("eventName", "string");

        EsoUIFunction subclass = new EsoUIFunction("Subclass");
        subclass.AddReturn("subclass", "object");

        EsoUIFunction clearCallbackRegistry = new EsoUIFunction("ClearCallbackRegistry");

        EsoUIFunction getFireCallbackDepth = new EsoUIFunction("GetFireCallbackDepth");
        fireCallbacks.AddReturn("fireCallbackDepth", "integer");

        EsoUIFunction getDirtyEvents = new EsoUIFunction("GetDirtyEvents");
        fireCallbacks.AddReturn("dirtyEvents", "table");

        EsoUIObject callbackManager = new EsoUIObject("CALLBACK_MANAGER");
        callbackManager.AddFunction(registerCallback);
        callbackManager.AddFunction(unregisterAllCallbacks);
        callbackManager.AddFunction(setHandleOnce);
        callbackManager.AddFunction(fireCallbacks);
        callbackManager.AddFunction(clean);
        callbackManager.AddFunction(clearCallbackRegistry);
        callbackManager.AddFunction(getFireCallbackDepth);
        callbackManager.AddFunction(getDirtyEvents);

        EsoUIObject callbackObject = new EsoUIObject("ZO_CallbackObject");
        callbackObject.AddFunction(registerCallback);
        callbackObject.AddFunction(unregisterAllCallbacks);
        callbackObject.AddFunction(setHandleOnce);
        callbackObject.AddFunction(fireCallbacks);
        callbackObject.AddFunction(clean);
        callbackObject.AddFunction(clearCallbackRegistry);
        callbackObject.AddFunction(getFireCallbackDepth);
        callbackObject.AddFunction(getDirtyEvents);
        callbackObject.AddFunction(subclass);
        #endregion

        #region guiroot
        EsoUIFunction getbottom = new EsoUIFunction("GetBottom");
        getbottom.AddReturn("bottom", "number");

        EsoUIFunction getcenter = new EsoUIFunction("GetCenter");
        getcenter.AddReturn("centerX", "number");
        getcenter.AddReturn("centerY", "number");

        EsoUIFunction getchild = new EsoUIFunction("GetChild");
        getchild.AddArgument("childIndex", "integer");
        getchild.AddReturn("childControl", "object");

        EsoUIFunction getdimensions = new EsoUIFunction("GetDimensions");
        getdimensions.AddReturn("width", "number");
        getdimensions.AddReturn("height", "number");

        EsoUIFunction getheight = new EsoUIFunction("GetHeight");
        getheight.AddReturn("height", "number");

        EsoUIFunction getleft = new EsoUIFunction("GetLeft");
        getleft.AddReturn("left", "number");

        EsoUIFunction getnumchildren = new EsoUIFunction("GetNumChildren");
        getnumchildren.AddReturn("numChildren", "integer");

        EsoUIFunction getnamedchild = new EsoUIFunction("GetNamedChild");
        getnamedchild.AddArgument("childName", "string");
        getnamedchild.AddReturn("childControl", "object");

        EsoUIFunction getright = new EsoUIFunction("GetRight");
        getright.AddReturn("right", "number");

        EsoUIFunction getscale = new EsoUIFunction("GetScale");
        getscale.AddReturn("scale", "number");

        EsoUIFunction gettop = new EsoUIFunction("GetTop");
        gettop.AddReturn("top", "number");

        EsoUIFunction getwidth = new EsoUIFunction("GetWidth");
        getwidth.AddReturn("width", "number");

        EsoUIObject guiroot = new EsoUIObject("GuiRoot");
        guiroot.AddCode("GuiRoot");
        guiroot.ElementType = APIElementType.C_OBJECT_TYPE;
        guiroot.AddFunction(getbottom);
        guiroot.AddFunction(getcenter);
        guiroot.AddFunction(getchild);
        guiroot.AddFunction(getdimensions);
        guiroot.AddFunction(getheight);
        guiroot.AddFunction(getleft);
        guiroot.AddFunction(getnumchildren);
        guiroot.AddFunction(getnamedchild);
        guiroot.AddFunction(getright);
        guiroot.AddFunction(getscale);
        guiroot.AddFunction(gettop);
        guiroot.AddFunction(getwidth);
        #endregion

        Documentation.Objects["EventManager"] = eventManager;
        Documentation.Functions["GetWindowManager"] = getWindowManager;
        Documentation.Functions["GetAnimationManager"] = getAnimationManager;
        Documentation.Functions["GetEventManager"] = getEventManager;
        Documentation.Functions["GetAddOnManager"] = getAddOnManager;
        Documentation.Objects["CALLBACK_MANAGER"] = callbackManager;
        Documentation.Objects["ZO_CallbackObject"] = callbackObject;
        Documentation.Objects["GuiRoot"] = guiroot;
    }

    private Task<EsoUIDocumentation> ParseFileAsync()
    {
        return Task.Run(() =>
        {
            Documentation = new EsoUIDocumentation();
            AddCustomData();
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

            AddManagers();
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
        }
    }
}
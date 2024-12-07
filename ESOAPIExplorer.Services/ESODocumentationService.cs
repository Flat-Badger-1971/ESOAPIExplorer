using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ESOAPIExplorer.Services;


public class ESODocumentationService : IESODocumentationService
{
    public EsoUIDocumentation Data { get; set; }
    public string FileName { get; set; }
    public string CurrentLine { get; set; }
    public List<string> CurrentEnum { get; set; }
    public EsoUIFunction CurrentFunction { get; set; }
    public EsoUIObject CurrentObject { get; set; }
    public EsoUIXMLElement CurrentElement { get; set; }
    public ReaderState State { get; set; }
    private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;
    private readonly FileOpenPicker _filePicker;
    public EsoUIDocumentation Documentation { get; set; }

    public ESODocumentationService()
    {
        _filePicker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        // Get the current window's HWND by passing in the Window object
        Window _MainWindow = (Window)Application.Current.GetType().GetProperty("Window").GetValue(Application.Current);
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_MainWindow);

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(_filePicker, hwnd);

        _filePicker.FileTypeFilter.Add(".txt");
    }

    public async Task InitialiseAsync()
    {
        Documentation = await GetDocumentationAsync();
    }

    private async Task<EsoUIDocumentation> GetDocumentationAsync()
    {
        string path = await GetPathAsync();
        return await ParseAsync(path);
    }

    private async Task<string> GetPathAsync()
    {
        string path = _settings.Values["last path"] as string;

        if (string.IsNullOrEmpty(path))
        {
            StorageFile file = await _filePicker.PickSingleFileAsync();
            if (file != null)
            {
                _settings.Values["last path"] = file.Path;
                path = file.Path;
            }
        }
        else if (!System.IO.File.Exists(path))
        {
            _settings.Values["last path"] = null;
            path = await GetPathAsync();
        }

        return path;
    }

    public bool LineStartsWith(string prefix)
    {
        return CurrentLine.StartsWith(prefix);
    }

    public bool LineEndsWith(string suffix)
    {
        return CurrentLine.EndsWith(suffix);
    }

    public string GetFirstMatch(Regex pattern)
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

    public List<string> GetMatches(Regex pattern)
    {
        MatchCollection matches = pattern.Matches(CurrentLine);
        List<string> result = [];

        for (int i = 1; i < matches[0].Groups.Count; i++)
        {
            result.Add(matches[0].Groups[i].Value);
        }

        return result;
    }

    public ReaderState FindNextState()
    {
        switch (true)
        {
            case true when LineStartsWith("{TOC:maxLevel"):
                return ReaderState.READ_API_VERSION;
            case true when LineStartsWith("{TOC:maxLevel"):
                return ReaderState.READ_API_VERSION;
            case true when LineStartsWith("h2. VM Functions"):
                return ReaderState.READ_VM_FUNCTIONS;
            case true when LineStartsWith("h2. Global Variables"):
                return ReaderState.READ_GLOBALS;
            case true when LineStartsWith("h2. Game API"):
                return ReaderState.READ_GAME_API;
            case true when LineStartsWith("h2. Object API"):
                return ReaderState.READ_OBJECT_API;
            case true when LineStartsWith("h2. Events"):
                return ReaderState.READ_EVENTS;
            case true when LineStartsWith("h2. UI XML Layout"):
                return ReaderState.READ_XML_ATTRIBUTES;
            case true when (State == ReaderState.READ_XML_ATTRIBUTES):
                return ReaderState.READ_XML_LAYOUT;
            default:
                return ReaderState.UNDETERMINED;
        }
    }

    public bool ReadApiVersion()
    {
        if (LineStartsWith("h1. "))
        {
            Data.ApiVersion = int.Parse(GetFirstMatch(new Regex(@"(\d+)$")));

            return true;
        }

        return false;
    }

    public List<string> GetOrCreateGlobal(string name)
    {
        if (!Data.Globals.TryGetValue(name, out List<string> value))
        {
            value = [];
            Data.Globals[name] = value;
        }

        return value;
    }

    public bool ReadGlobals()
    {
        switch (true)
        {
            case true when LineStartsWith("h2. "):
                return true;
            case true when LineStartsWith("h5. "):
                string enumName = GetFirstMatch(new Regex(@"h5\. (.+)$"));
                CurrentEnum = GetOrCreateGlobal(enumName);
                break;
            case true when LineStartsWith("* "):
                string enumValue = GetFirstMatch(new Regex(@"\* (.+)$"));
                CurrentEnum.Add(enumValue);
                break;
            default:
                break;
        }

        return false;
    }

    public static List<EsoUIArgument> ParseArgs(string args)
    {
        List<EsoUIArgument> data = [];
        string[] argsArray = args.Split(',');

        int argId = 1;
        foreach (string arg in argsArray)
        {
            Match match = Regex.Match(arg, @"\*(.+)\* _(.+)_");
            data.Add(new EsoUIArgument(match.Groups[2].Value, new EsoUIType(match.Groups[1].Value), argId));
            argId++;
        }

        return data;
    }

    public bool ReadGameApi()
    {
        if (LineStartsWith("h2. "))
        {
            return true;
        }

        ReadFunction(Data.Functions);

        return false;
    }

    public void ReadFunction(Dictionary<string, EsoUIFunction> functions)
    {
        switch (true)
        {
            case true when LineStartsWith("* "):
                List<string> matches = GetMatches(new Regex(@"^\* (.+)\((.*)\)$"));
                Match nameMatch = Regex.Match(matches[0], @"([^\s]+)( \*(.+)\*)?");
                string name = nameMatch.Groups[1].Value;
                string access = nameMatch.Groups[3].Value;
                EsoUIFunctionAccess accessLevel = access == string.Empty ? EsoUIFunctionAccess.PUBLIC : access.ToEsoUIFunctionAccess();

                CurrentFunction = new EsoUIFunction(name, accessLevel);

                if (!string.IsNullOrEmpty(matches[1]))
                {
                    CurrentFunction.Args = ParseArgs(matches[1]);
                }

                functions[name] = CurrentFunction;
                break;
            case true when LineStartsWith("** _Uses variable returns"):
                CurrentFunction.HasVariableReturns = true;
                break;
            case true when LineStartsWith("** _Returns:_"):
                string args = GetFirstMatch(new Regex(@"\*\*\ _Returns:_ (.+)$"));
                CurrentFunction.Returns = ParseArgs(args);
                break;
        }
    }

    public EsoUIObject GetOrCreateObject(string name)
    {
        if (!Data.Objects.TryGetValue(name, out EsoUIObject value))
        {
            value = new EsoUIObject(name);
            Data.Objects[name] = value;
        }

        return value;
    }

    public bool ReadObjectApi()
    {
        if (LineStartsWith("h2. "))
        {
            return true;
        }

        return false;
    }

    public bool ReadEvents()
    {
        switch (true)
        {
            case true when LineStartsWith("h2. "):
                // section ended
                return true;
            case true when LineStartsWith("* "):
                List<string> matches = GetMatches(new Regex(@"^\*\ ([^\s]+)(\s\((.+)\))?"));
                string name = matches[0];
                string argsString = matches[2];

                List<EsoUIArgument> args = null;

                if (string.IsNullOrEmpty(argsString))
                {
                    args = ParseArgs("*integer* _eventId_");
                }
                else
                {
                    args = ParseArgs("*integer* _eventId_," + argsString);
                }

                Data.Events[name] = new EsoUIEvent(name, args);
                return false;
            default:
                return false;
        }
    }

    public bool ReadXmlAttributes()
    {
        if (LineStartsWith("h4. Attributes"))
        {
            // ignore the header
            return false;
        }
        else if (!LineStartsWith("* "))
        {
            // section ended
            return true;
        }

        List<string> matches = GetMatches(new Regex(@"^\*\ (.+) \*(.+)\*$"));
        string name = matches[0];
        string type = matches[1];
        Data.XmlAttributes[name] = new EsoUIArgument(name, new EsoUIType(type), 1);

        return false;
    }

    public EsoUIXMLElement GetOrCreateElement(string name)
    {
        if (!Data.XmlLayout.TryGetValue(name, out EsoUIXMLElement value))
        {
            value = new EsoUIXMLElement(name);
            Data.XmlLayout[name] = value;
        }

        return value;
    }

    public bool ReadXmlLayout()
    {
        switch (true)
        {
            case true when LineStartsWith("h5. sentinel_element"):
                // section ended
                return true;
            case true when LineStartsWith("h5. "):
                string elementName = GetFirstMatch(new Regex(@"h5. (.+)$"));
                CurrentElement = GetOrCreateElement(elementName);
                break;
            case true when LineStartsWith("* _attr"):
                string attribute = GetFirstMatch(new Regex(@"\* _attribute:_ (.+)$"));
                Match match = Regex.Match(attribute, @"\*(.+)\* _(.+)_");
                string type = match.Groups[1].Value;
                string name = match.Groups[2].Value;
                CurrentElement.Attributes.Add(new EsoUIArgument(name, new EsoUIType(type), 1));
                break;
            case true when LineStartsWith("* ScriptArguments"):
                CurrentElement.Documentation = GetFirstMatch(new Regex(@"\* ScriptArguments: (.+)$"));
                break;
            case true when LineStartsWith("* ["):
                {
                    List<string> matches = GetMatches(new Regex(@"^\*\ \[(.+): (.+)\|#(.+)\]$"));
                    string lineType = matches[0];
                    string lname = matches[1];
                    string ltype = matches[2];
                    switch (lineType)
                    {
                        case "Child":
                            if (ltype == "Attributes")
                            {
                                CurrentElement.Attributes.Add(Data.XmlAttributes[lname]);
                            }
                            else
                            {
                                CurrentElement.Children.Add(new EsoUIType(lname, ltype));
                            }
                            break;
                        case "Inherits":
                            CurrentElement.Parent = new EsoUIType(ltype);
                            break;
                        default:
                            Console.WriteLine("Unhandled prefix: " + lineType + " - " + CurrentLine);
                            break;
                    }
                    break;
                }
        }

        return false;
    }

    public void InjectCustomData()
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

        Data.Objects["EventManager"] = eventManager;
        Data.Functions["GetWindowManager"] = getWindowManager;
        Data.Functions["GetAnimationManager"] = getAnimationManager;
        Data.Functions["GetEventManager"] = getEventManager;
        Data.Functions["GetAddOnManager"] = getAddOnManager;
    }

    public Task<EsoUIDocumentation> ParseFileAsync()
    {
        return Task.Run(() =>
        {
            Data = new EsoUIDocumentation();
            InjectCustomData();
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

                return Data;
            }
            else
            {
                return null;
            }
        });
    }

    public Task<EsoUIDocumentation> ParseAsync(string fileName)
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
            case ReaderState.READ_XML_ATTRIBUTES:
                finished = ReadXmlAttributes();
                break;
            case ReaderState.READ_XML_LAYOUT:
                finished = ReadXmlLayout();
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
            CurrentElement = null;
        }
    }
}
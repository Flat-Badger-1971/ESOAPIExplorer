using ESOAPIExplorer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace ESOAPIExplorer.Services;

public class LuaLanguageServerDefinitionsGeneratorService(IESODocumentationService _esoDocumentationService, IRegexService _regexService) : ILuaLanguageServerDefinitionsGeneratorService
{
    private readonly StringBuilder returns = new StringBuilder();
    private readonly StringBuilder param = new StringBuilder();
    private readonly StringBuilder func = new StringBuilder();
    private readonly EsoUIDocumentation docs = _esoDocumentationService.Documentation;
    private bool _added = false;

    public async Task Generate(StorageFolder esofolder)
    {
        StorageFile esoapiFile;

        // create definition files
        esoapiFile = await esofolder.CreateFileAsync("aliases.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddAliases());

        esoapiFile = await esofolder.CreateFileAsync("api.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddAPI());

        esoapiFile = await esofolder.CreateFileAsync("classes.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddClasses(false));

        esoapiFile = await esofolder.CreateFileAsync("zoclasses.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddClasses(true));

        esoapiFile = await esofolder.CreateFileAsync("events.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddEvents());

        esoapiFile = await esofolder.CreateFileAsync("functions.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddFunctions());

        esoapiFile = await esofolder.CreateFileAsync("globals.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddGlobals());

        esoapiFile = await esofolder.CreateFileAsync("sounds.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddSounds());

        esoapiFile = await esofolder.CreateFileAsync("zoclasses.min", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddClasses(true, true));

        esoapiFile = await esofolder.CreateFileAsync("fragments.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddFragments());
    }

    private string AddFragments()
    {
        StringBuilder fragmentDefs = new StringBuilder("--- @meta\n--- @diagnostic disable\n\n");

        foreach (KeyValuePair<string, bool> fragment in docs.Fragments)
        {
            fragmentDefs.AppendLine($"--- @class {fragment.Key}:ZO_SceneFragment");
            fragmentDefs.AppendLine($"{fragment.Key} = {{}}");
        }

        return fragmentDefs.ToString();
    }

    private string AddSounds()
    {
        string sounds = File.ReadAllText(docs.SoundsPath);

        StringBuilder sounddefs = new StringBuilder("--- @meta\n--- @diagnostic disable\n\n");

        sounddefs.AppendLine("--- @enum SOUNDS");
        sounddefs.AppendLine(sounds);

        return sounddefs.ToString();
    }

    private void AddDeclaredFunctions()
    {
        EsoUIFunction getClassIcon = new EsoUIFunction("GetClassIcon");
        getClassIcon.AddArgument("classId", "integer");
        getClassIcon.AddReturn("textureName", "textureName");

        docs.Functions.TryAdd("GetClassIcon", getClassIcon);

        EsoUIFunction getRoleIcon = new EsoUIFunction("GetroleIcon");
        getRoleIcon.AddArgument("roleId", "integer");
        getRoleIcon.AddReturn("textureName", "textureName");

        docs.Functions.TryAdd("GetRoleIcon", getRoleIcon);
    }

    private string AddFunctions()
    {
        AddDeclaredFunctions();

        // functions declared in lua files
        StringBuilder funcDefs = new StringBuilder("--- @meta\n--- @diagnostic disable\n\n");
        string retString = "--- @return any";

        foreach (KeyValuePair<string, EsoUIFunction> func in docs.Functions)
        {
            Clear();
            List<string> rawparamlist = [];

            for (int i = 0; i < func.Value.Args.Count; i++)
            {
                EsoUIArgument arg = func.Value.Args[i];

                if (i == func.Value.Args.Count - 1)
                {
                    param.Append($"--- @param {arg.Name} {Utility.InferType(arg.Name, _regexService)}");
                }
                else
                {
                    param.AppendLine($"--- @param {arg.Name} {Utility.InferType(arg.Name, _regexService)}");
                }

                rawparamlist.Add(arg.Name);
            }

            string code = string.Empty;

            if (func.Value.Code.Count > 0)
            {
                code = func.Value.Code[0].Replace("\r", "");
            }

            if (!code.StartsWith("* "))
            {
                if (param.Length > 0)
                {
                    funcDefs.AppendLine(param.ToString());
                }

                funcDefs.AppendLine(retString);
                funcDefs.AppendLine($"function {func.Key}({string.Join(", ", rawparamlist)}) end");
            }
        }
        funcDefs.AppendLine("\nSLASH_COMMANDS = {}");

        string returnString = funcDefs.ToString();

        string newString = "--- @param labelText any\n--- @param onSelect any\n--- @param itemType? any\n--- @param labelFont? any\n--- @param normalColor? any\n--- @param highlightColor? any\n--- @param itemYPad? any\n--- @param horizontalAlignment? any\n--- @param isHighlighted? boolean\n--- @param onEnter? any\n--- @param onExit? any\n--- @param enabled? boolean";
        string oldString = "--- @param labelText any\r\n--- @param onSelect any\r\n--- @param itemType any\r\n--- @param labelFont any\r\n--- @param normalColor any\r\n--- @param highlightColor any\r\n--- @param itemYPad any\r\n--- @param horizontalAlignment any\r\n--- @param isHighlighted boolean\r\n--- @param onEnter any\r\n--- @param onExit any\r\n--- @param enabled boolean";

        returnString = ReplaceString(returnString, oldString, newString);

        newString = "--- @param name string\n--- @param data? userdata\n--- @param textParams? any\n--- @param isGamepad? boolean\n--- @return any\nfunction ZO_Dialogs_ShowDialog(name, data, textParams, isGamepad) end";
        oldString = "--- @param name string\r\n--- @param data userdata\r\n--- @param textParams any\r\n--- @param isGamepad boolean\r\n--- @return any\r\nfunction ZO_Dialogs_ShowDialog(name, data, textParams, isGamepad) end";

        returnString = ReplaceString(returnString, oldString, newString);

        newString = "--- @param hookFunction? function\n--- @return any\nfunction ZO_PreHook(";
        oldString = "--- @param hookFunction function\r\n--- @return any\r\nfunction ZO_PreHook(";

        returnString = ReplaceString(returnString, oldString, newString);

        oldString = "--- @param control userdata\r\n--- @param handlerName string\r\n--- @param hookFunction function\r\n--- @return any\r\nfunction ZO_";
        newString = "--- @param control any\n--- @param handlerName any\n--- @param hookFunction? function\nfunction ZO_";

        returnString = ReplaceString(returnString, oldString, newString);

        newString = "--- @param hookFunction? function\n--- @return any\nfunction ZO_PostHook(";
        oldString = "--- @param hookFunction function\r\n--- @return any\r\nfunction ZO_PostHook(";

        returnString = ReplaceString(returnString, oldString, newString);

        return returnString;
    }

    private string AddGlobals()
    {
        StringBuilder objectdefs = new StringBuilder("--- @meta\n--- @diagnostic disable\n\n");

        // api globals
        List<string> aliasValues = [];

        foreach (KeyValuePair<string, ICollection<EsoUIEnumValue>> global in docs.Globals)
        {
            aliasValues.Clear();

            foreach (var val in global.Value)
            {
                objectdefs.AppendLine($"{val.Name} = {val.Value}");
                aliasValues.Add(val.Name);
            }

            if (global.Key != "Globals")
            {
                IOrderedEnumerable<string> sortedValues = aliasValues.OrderBy(v => v);
                objectdefs.AppendLine($"--- @alias {global.Key} {string.Join("|", sortedValues)}");
            }

            objectdefs.AppendLine();
        }

        // in game constants
        foreach (KeyValuePair<string, EsoUIConstantValue> constant in docs.Constants)
        {
            if (double.TryParse(constant.Value.Value, out double value))
            {
                objectdefs.AppendLine($"{constant.Value.Name} = {value}");
            }
            else
            {
                objectdefs.AppendLine($"{constant.Value.Name} = \"{constant.Value.Value}\"");
            }
        }

        return objectdefs.ToString();
    }

    private static void AppendAlias(StringBuilder defs, KeyValuePair<string, EsoUIObject> obj)
    {
        if (!string.IsNullOrEmpty(obj.Value.InstanceName) && obj.Value.InstanceName != obj.Key)
        {
            defs.AppendLine($"--- @class {obj.Value.InstanceName}:{obj.Key}");
            defs.AppendLine($"{obj.Value.InstanceName} = {{}}");
        }
    }

    private void AddMissing()
    {
        EsoUIFunction initialise = new EsoUIFunction("Initialize");
        initialise.AddArgument("...", "any");

        EsoUIFunction iosubclass = new EsoUIFunction("Subclass");
        iosubclass.AddReturn("initializingObject", "ZO_InitializingObject");

        if (docs.Objects.TryRemove("ZO_InitializingObject", out EsoUIObject obj))
        {
            obj.FromAPI = true;
            obj.AddFunction(initialise);
            obj.AddFunction(iosubclass);

            docs.Objects.TryAdd("ZO_InitializingObject", obj);
        }

        EsoUIFunction iocsubclass = new EsoUIFunction("Subclass");
        iocsubclass.AddReturn("", "ZO_InitializingCallbackObject");

        EsoUIObject initialisingCallbackObject = new EsoUIObject("ZO_InitializingCallbackObject")
        {
            FromAPI = true
        };

        initialisingCallbackObject.AddFunction(iocsubclass);
        initialisingCallbackObject.AddExtends("ZO_InitializingObject, ZO_CallbackObject");

        docs.Objects.TryAdd("ZO_InitializingCallbackObject", initialisingCallbackObject);

        if (docs.Objects.TryRemove("ZO_UnitFrames_Manager", out EsoUIObject unitFramesManager))
        {
            unitFramesManager.AddInstanceName("UNIT_FRAMES");

            docs.Objects.TryAdd("ZO_UnitFrames_Manager", unitFramesManager);
        }
    }

    private string AddClasses(bool zoclasses, bool trimmed = false)
    {
        if (!_added)
        {
            AddMissing();
            _added = true;
        }

        string optional = string.Empty;

        StringBuilder objectdefs = new StringBuilder("--- @meta\n--- @diagnostic disable\n\n");

        foreach (KeyValuePair<string, EsoUIObject> obj in docs.Objects)
        {
            if (zoclasses && !obj.Key.StartsWith("ZO_"))
            {
                continue;
            }

            if (!zoclasses && obj.Key.StartsWith("ZO_"))
            {
                continue;
            }

            Clear();
            string extends = $":{obj.Value.Extends ?? "ZO_Object"}";

            if (obj.Value.Name == "ZO_Object")
            {
                extends = string.Empty;
            }

            objectdefs.AppendLine($"--- @class {obj.Key}{extends}");
            objectdefs.AppendLine($"{obj.Key} = {{}}");

            if (!trimmed)
            {
                if (zoclasses)
                {
                    optional = "?";
                }
                else
                {
                    optional = string.Empty;
                }

                foreach (KeyValuePair<string, EsoUIFunction> esofunc in obj.Value.Functions)
                {
                    Clear();

                    List<string> rawparams = [];

                    for (int i = 0; i < esofunc.Value.Args.Count; i++)
                    {
                        EsoUIArgument arg = esofunc.Value.Args[i];

                        if (i == esofunc.Value.Args.Count - 1)
                        {
                            param.Append($"--- @param {arg.Name}{optional} {arg.Type.Name.Replace(":nilable", "|nil")}");
                        }
                        else
                        {
                            param.AppendLine($"--- @param {arg.Name}{optional} {arg.Type.Name.Replace(":nilable", "|nil")}");
                        }

                        rawparams.Add(arg.Name);
                    }

                    string code = $"function {obj.Key}:{esofunc.Key}({string.Join(", ", rawparams)}) end";

                    func.AppendLine(code);

                    if (zoclasses && (esofunc.Key == "New" || esofunc.Key == "Clone" || esofunc.Key == "Subclass"))
                    {
                        returns.Append($"--- @return {obj.Key}");
                    }
                    else if (obj.Value.FromAPI)
                    {
                        if (esofunc.Value.Returns.Count > 1)
                        {
                            returns.Append("--- @return any");
                        }
                        else if (esofunc.Value.Returns.Count == 1)
                        {
                            returns.Append($"--- @return {string.Join(", ", esofunc.Value.Returns.First().Values.Select(v => v.Type.Name.Replace(":nilable", "|nil")))}");
                        }
                        else
                        {
                            returns.Append("--- @return void");
                        }
                    }
                    else
                    {
                        returns.Append("--- @return any");
                    }

                    if (returns.Length == 0)
                    {
                        returns.Append("--- @return void");
                    }

                    if (param.Length > 0)
                    {
                        objectdefs.AppendLine(param.ToString());
                    }

                    objectdefs.AppendLine(returns.ToString());
                    objectdefs.AppendLine(func.ToString());
                }
            }

            AppendAlias(objectdefs, obj);
            objectdefs.AppendLine();
        }

        if (zoclasses)
        {
            objectdefs.AppendLine("--- @class PROMOTIONAL_EVENT_MANAGER:ZO_PromotionalEvent_Manager");
            objectdefs.AppendLine("PROMOTIONAL_EVENT_MANAGER = {}");
            objectdefs.AppendLine("--- @param reason string\n--- @param hidden boolean\n--- @param fadeInTime? number\n--- @param fadeOutTime? number\nfunction ZO_HUDFadeSceneFragment:SetHiddenForReason(reason, hidden, fadeInTime, fadeOutTime) end");
        }

        string returnString = objectdefs.ToString();
        string oldString;
        string newString;

        if (zoclasses && !trimmed)
        {
            newString = "--- @param r? number|string|table\n--- @param g? number\n--- @param b? number\n--- @param a? number\n--- @return ZO_ColorDef\nfunction ZO_ColorDef:New(r, g, b, a) end";
            oldString = "--- @param template? any\r\n--- @return ZO_ColorDef\r\nfunction ZO_ColorDef:New(template) end";

            returnString = ReplaceString(returnString, oldString, newString);

            newString = "--- @param ... any\n--- @return ZO_PlatformStyle\nfunction ZO_PlatformStyle:New(...) end";
            oldString = "--- @param template any\r\n--- @return ZO_PlatformStyle\r\nfunction ZO_PlatformStyle:New(template) end";

            returnString = ReplaceString(returnString, oldString, newString);

            oldString = "--- @return any\r\nfunction ZO_HUDFadeSceneFragment:Show() end";
            newString = "--- @param fadeInTime? number\n--- @return any\nfunction ZO_HUDFadeSceneFragment:Show(fadeInTime) end";

            returnString = ReplaceString(returnString, oldString, newString);

            oldString = "--- @return any\r\nfunction ZO_HUDFadeSceneFragment:Hide() end";
            newString = "--- @param fadeOutTime? number\n--- @return any\nfunction ZO_HUDFadeSceneFragment:Hide(fadeOutTime) end";

            returnString = ReplaceString(returnString, oldString, newString);
        }
        else if (!trimmed)
        {
            oldString = "--- @param rewardId number\r\n--- @param quantity any\r\n--- @param parentChoice any\r\n--- @param validationFunction function\r\n--- @param isSelectedChoiceFunction boolean\r\n--- @return any\r\nfunction IngameRewardsManager:GetInfoForReward(";
            newString = "--- @param rewardId number\n--- @param quantity any\n--- @param parentChoice? any\n--- @param validationFunction? function\n--- @param isSelectedChoiceFunction? boolean\n--- @return any\nfunction IngameRewardsManager:GetInfoForReward(";

            returnString = ReplaceString(returnString, oldString, newString);

            oldString = "--- @param name string\r\n--- @return any\r\nfunction WINDOW_MANAGER:CreateTop";
            newString = "--- @param name? string\n--- @return any\nfunction WINDOW_MANAGER:CreateTop";

            returnString = ReplaceString(returnString, oldString, newString);
        }

        oldString = "--- @param optionalSuffix string";
        newString = "--- @param optionalSuffix? string";

        returnString = ReplaceString(returnString, oldString, newString);

        return returnString;
    }

    private static string ReplaceString(string input, string oldString, string newString)
    {
        int p = input.IndexOf(oldString);

        if (p != -1)
        {
            return input.Remove(p, oldString.Length).Insert(p, newString);
        }

        return input;
    }

    private string AddEvents()
    {
        // missing for some reason
        EsoUIEvent playerLogout = new EsoUIEvent("EVENT_PLAYER_LOGOUT", []);
        EsoUIEvent playerQuit = new EsoUIEvent("EVENT_PLAYER_QUIT", []);

        docs.Events.TryAdd("EVENT_PLAYER_LOGOUT", playerLogout);
        docs.Events.TryAdd("EVENT_PLAYER_QUIT", playerQuit);

        StringBuilder eventdefs = new StringBuilder("--- @meta\n--- @diagnostic disable\n\n");

        foreach (KeyValuePair<string, EsoUIEvent> esoevent in docs.Events)
        {
            Clear();

            for (int i = 0; i < esoevent.Value.Args.Count; i++)
            {
                EsoUIArgument arg = esoevent.Value.Args[i];

                if (i == esoevent.Value.Args.Count - 1)
                {
                    param.Append($"--- @param {arg.Name} {arg.Type.Name.Replace(":nilable", "|nil")}");
                }
                else
                {
                    param.AppendLine($"--- @param {arg.Name} {arg.Type.Name.Replace(":nilable", "|nil")}");
                }
            }

            returns.Append("--- @return void");
            func.AppendLine($"function {esoevent.Key}({string.Join(", ", esoevent.Value.Args.Select(a => a.Name))}) end");

            eventdefs.AppendLine(param.ToString());
            eventdefs.AppendLine(returns.ToString());
            eventdefs.AppendLine(func.ToString());
        }

        return eventdefs.ToString();
    }

    /*

            // Enums
            ObservableCollection<APIElement> enums = new ObservableCollection<APIElement>(_esoDocumentationService.Documentation.Globals
                .Where(g => g.Key != "Globals")
                .SelectMany(item =>
                    item.Value
                    .Select(detail =>
                        new APIElement
                        {
                            Id = detail.Name,
                            Name = detail.Name,
                            ElementType = APIElementType.ENUM_CONSTANT,
                            Parent = item.Key,
                            Code = [$"* {detail.Name}"]
                        }))
                .Concat(_esoDocumentationService.Documentation.Globals
                .Where(g => g.Key != "Global")
                .SelectMany(item =>
                    item.Value
                    .Select(detail =>
                        new APIElement
                        {
                            Id = item.Key,
                            Name = item.Key,
                            ElementType = APIElementType.ENUM_TYPE,
                            Code = [$"* {item.Key}"]
                        }))
                .GroupBy(e => e.Id)
                .Select(e => e.First())));

            // Constants
            ObservableCollection<APIElement> constants = new ObservableCollection<APIElement>(_esoDocumentationService.Documentation.Constants
                .Select(item =>
                    new APIElement
                    {
                        Id = item.Key,
                        Name = item.Value.Name,
                        ElementType = item.Value.Name.StartsWith("SI_") ? APIElementType.SI_GLOBAL : APIElementType.CONSTANT,
                        Code = [$"{item.Value.Name} = {item.Value.Value}"]
                    })
                .Concat(_esoDocumentationService.Documentation.Globals
                    .Where(g => g.Key == "Globals")
                    .SelectMany(item =>
                        item.Value
                        .Select(detail =>
                            new APIElement
                            {
                                Id = detail.Name,
                                Name = detail.Name,
                                ElementType = APIElementType.CONSTANT,
                                Code = [$"* {detail.Name}"]
                            }))));
     */
    private string AddAPI()
    {
        StringBuilder apidefs = new StringBuilder("--- @meta\n--- @diagnostic disable\n\n");

        // api functions
        foreach (KeyValuePair<string, EsoUIFunction> esofunc in docs.Functions)
        {
            Clear();

            if (esofunc.Value.ElementType == APIElementType.C_FUNCTION)
            {
                for (int i = 0; i < esofunc.Value.Args.Count; i++)
                {
                    EsoUIArgument arg = esofunc.Value.Args[i];

                    arg.Name = arg.Name.Replace("function", "func");

                    if (esofunc.Value.Name == "GetString")
                    {
                        if (arg.Name == "contextId")
                        {
                            arg.Type.Name = arg.Type.Name + ":nilable";
                        }
                    }

                    if (i == esofunc.Value.Args.Count - 1)
                    {
                        param.Append($"--- @param {arg.Name} {arg.Type.Name.Replace(":nilable", "|nil")}");
                    }
                    else
                    {
                        param.AppendLine($"--- @param {arg.Name} {arg.Type.Name.Replace(":nilable", "|nil")}");
                    }
                }

                if (esofunc.Value.Code.Count > 0)
                {
                    string code = esofunc.Value.Code[0].Replace("\r", "");

                    if (code.StartsWith("* "))
                    {
                        Match apifuncvalues = _regexService.ApiParamSplitMatcher().Match(code);
                        string paramlist = string.Empty;
                        string rawparamlist = string.Empty;

                        if (apifuncvalues.Success)
                        {
                            string[] paramarray = apifuncvalues.Groups[2].Value.Split(",");

                            if (paramarray.Length > 0)
                            {
                                StringBuilder rawparambuilder = new StringBuilder();

                                foreach (string param in paramarray)
                                {
                                    if (!string.IsNullOrWhiteSpace(param))
                                    {
                                        string[] parts = param.Trim().Split(" ");
                                        rawparambuilder.Append($"{parts[1].Replace("_", "")}, ");
                                    }
                                }

                                rawparamlist = rawparambuilder.ToString().TrimEnd(' ').TrimEnd(',').Replace("function", "func");
                            }
                        }

                        string funcLine = apifuncvalues.Groups[1].Value;

                        Match scopeInfo = _regexService.ScopeMatcher().Match(funcLine);

                        if (scopeInfo.Success)
                        {
                            switch (scopeInfo.Groups[1].Value)
                            {
                                case "protected":
                                    param.Append("\n--- @protected");
                                    break;
                                case "private":
                                    param.Append("\n--- @private");
                                    break;
                            }

                            code = $"function {funcLine.Replace($" *{scopeInfo.Groups[1].Value}* ", "")}({rawparamlist}) end";
                        }
                        else
                        {
                            code = $"function {funcLine}({rawparamlist}) end";
                        }
                    }

                    func.AppendLine(code);
                }

                for (int i = 0; i < esofunc.Value.Returns.Count; i++)
                {
                    EsoUIReturn ret = esofunc.Value.Returns[i];

                    if (i == esofunc.Value.Returns.Count - 1)
                    {
                        returns.Append($"--- @return {string.Join(", ", ret.Values.Select(v => $"{v.Type.Name.Replace(":nilable", "|nil")}"))}");
                    }
                    else
                    {
                        returns.AppendLine($"--- @return {string.Join(", ", ret.Values.Select(v => $"{v.Type.Name.Replace(":nilable", "|nil")}"))}");
                    }
                }

                if (returns.Length == 0)
                {
                    returns.Append("--- @return void");
                }

                if (param.Length > 0)
                {
                    apidefs.AppendLine(param.ToString());
                }

                apidefs.AppendLine(returns.ToString());
                apidefs.AppendLine(func.ToString());
            }
        }

        return apidefs.ToString();
    }

    private static string AddAliases()
    {
        StringBuilder aliasdefs = new StringBuilder("--- @meta\n--- @diagnostic disable\n\n");

        aliasdefs.AppendLine("--- @alias bool boolean");
        aliasdefs.AppendLine("--- @alias Event integer");
        aliasdefs.AppendLine("--- @alias id64 integer");
        aliasdefs.AppendLine("--- @alias integer53 integer");
        aliasdefs.AppendLine("--- @alias ItemSetCollectionSlot_id64 integer");
        aliasdefs.AppendLine("--- @alias layout_measurement integer");
        aliasdefs.AppendLine("--- @alias luaindex integer");
        aliasdefs.AppendLine("--- @alias textureName string");
        aliasdefs.AppendLine("--- @alias void nil");

        return aliasdefs.ToString();
    }

    private void Clear()
    {
        returns.Clear();
        param.Clear();
        func.Clear();
    }

    private static async Task<StorageFolder> SetupDefinitionsStorage(StorageFolder folder)
    {
        // Check if .luarc.json exists
        StorageFile luarcFile = await folder.TryGetItemAsync(".luarc.json") as StorageFile;

        if (luarcFile == null)
        {
            // Create .luarc.json if it does not exist
            luarcFile = await folder.CreateFileAsync(".luarc.json", CreationCollisionOption.FailIfExists);
            await FileIO.WriteTextAsync(luarcFile, "{}");
        }

        // Read the content of .luarc.json
        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true, ReadCommentHandling = JsonCommentHandling.Skip };
        string luarcContent = await FileIO.ReadTextAsync(luarcFile);
        Dictionary<string, object> luarcJson = JsonSerializer.Deserialize<Dictionary<string, object>>(luarcContent, options) ?? [];

        // Check if "workspace.library" exists and update it
        if (luarcJson.ContainsKey("workspace.library"))
        {
            luarcJson["workspace.library"] = new[] { "./esoapi/esoapi.lua" };
        }
        else
        {
            string[] value = ["./esoapi/esoapi.lua"];
            luarcJson.Add("workspace.library", value);
        }

        // Write the updated content back to .luarc.json
        string updatedLuarcContent = JsonSerializer.Serialize(luarcJson, options);
        await FileIO.WriteTextAsync(luarcFile, updatedLuarcContent);

        // ensure the folder exists
        return await folder.TryGetItemAsync("esoapi") as StorageFolder ?? await folder.CreateFolderAsync("esoapi");
    }
}
using ESOAPIExplorer.Models;
using System;
using System.Collections.Generic;
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

    public async Task Generate(StorageFolder folder)
    {
        StorageFolder esofolder = await SetupDefinitionsStorage(folder);

        // add modules
        StringBuilder definitions = new StringBuilder("--- @meta\n\n");

        definitions.AppendLine("--- @module './aliases.lua'");
        definitions.AppendLine("--- @module './events.lua'");
        definitions.AppendLine("--- @module './objects.lua'");
        //definitions.AppendLine("--- @module './aliases.lua'");
        //definitions.AppendLine("--- @module './aliases.lua'");

        // create esoapi.lua
        StorageFile esoapiFile = await esofolder.CreateFileAsync("esoapi.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, definitions.ToString());

        esoapiFile = await esofolder.CreateFileAsync("aliases.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddAliases());

        esoapiFile = await esofolder.CreateFileAsync("events.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddEvents());

        esoapiFile = await esofolder.CreateFileAsync("objects.lua", CreationCollisionOption.ReplaceExisting);
        await FileIO.WriteTextAsync(esoapiFile, AddObjects());
    }

    private string ReplaceProblematicCharacters(string input)
    {
        return input.Replace("#", "(num)")
            .Replace(" ", "_")
            .Replace("<=", "lte")
            .Replace(">=", "gte");
    }

    private string AddObjects()
    {
        StringBuilder objectdefs = new StringBuilder();

        foreach (KeyValuePair<string, EsoUIObject> obj in docs.Objects)
        {
            Clear();

            objectdefs.AppendLine($"--- @class {obj.Key}");
            objectdefs.AppendLine($"{obj.Key} = {{}}\n");

            foreach (KeyValuePair<string, EsoUIFunction> esofunc in obj.Value.Functions)
            {
                Clear();

                for (int i = 0; i < esofunc.Value.Args.Count; i++)
                {
                    EsoUIArgument arg = esofunc.Value.Args[i];

                    if (i == esofunc.Value.Args.Count - 1)
                    {
                        param.Append($"--- @param {arg.Name} {arg.Type.Name.Replace(":nilable", "|nil")}");
                    }
                    else
                    {
                        param.AppendLine($"--- @param {arg.Name} {arg.Type.Name.Replace(":nilable", "|nil")}");
                    }
                }

                if (esofunc.Value.Code.Count == 0)
                {
                    // TODO: fix missing items
                    func.AppendLine("-- ***missing!***");
                }
                else
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

                            if (paramarray.Length > 1)
                            {
                                StringBuilder rawparambuilder = new StringBuilder();

                                foreach (string param in paramarray)
                                {
                                    string[] parts = param.Trim().Split(" ");
                                    rawparambuilder.Append($"{parts[1].Replace("_", "")}, ");
                                }

                                rawparamlist = rawparambuilder.ToString().TrimEnd(' ').TrimEnd(',');
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
                    else
                    {
                        // remove any comments
                        int trueEOL = code.LastIndexOf(')');

                        if (trueEOL != -1 && trueEOL < code.Length - 1)
                        {
                            code = code.Substring(0, trueEOL + 1);
                        }

                        code = $"{code} end";
                    }

                    func.AppendLine(code);
                }

                for (int i = 0; i < esofunc.Value.Returns.Count; i++)
                {
                    EsoUIReturn ret = esofunc.Value.Returns[i];

                    if (i == esofunc.Value.Returns.Count - 1)
                    {
                        returns.Append($"--- @return {string.Join(", ", ret.Values.Select(v => $"{ReplaceProblematicCharacters(v.Name)} {v.Type.Name.Replace(":nilable", "|nil")}"))}");
                    }
                    else
                    {
                        returns.AppendLine($"--- @return {string.Join(", ", ret.Values.Select(v => $"{ReplaceProblematicCharacters(v.Name)} {v.Type.Name.Replace(":nilable", "|nil")}"))}");
                    }
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

        return objectdefs.ToString();
    }

    private string AddEvents()
    {
        StringBuilder eventdefs = new StringBuilder();

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

    private static string AddAliases()
    {
        StringBuilder aliasdefs = new StringBuilder();

        aliasdefs.AppendLine("--- @alias void nil");
        aliasdefs.AppendLine("--- @alias bool boolean");
        aliasdefs.AppendLine("--- @alias id64 integer");
        aliasdefs.AppendLine("--- @alias luaindex integer");

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
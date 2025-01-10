using ESOAPIExplorer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public class LuaObjectScannerService(IRegexService regexService) : ILuaObjectScanner
{
    public string FolderPath { get; set; }
    public LuaScanResults Results { get; set; } = new LuaScanResults();

    private ConcurrentDictionary<string, string> _Subclasses = [];

    public void ScanFolderForLuaFunctions()
    {
        ConcurrentDictionary<string, bool> ignore = [];
        ignore.TryAdd("internalingame_rewards_manager.lua", true);

        // Parallel.ForEach(Directory.EnumerateFiles(FolderPath, "*.lua", SearchOption.AllDirectories), file =>
        foreach (string file in Directory.EnumerateFiles(FolderPath, "*.lua", SearchOption.AllDirectories))
        {
            string filename = Path.GetFileName(file);

            if (!ignore.ContainsKey(filename))
            {
                string content = File.ReadAllText(file);
                ScanFile(content, file);
            }
        };

        // check for subclass methods
        foreach (KeyValuePair<string, string> subclass in _Subclasses)
        {
            EsoUIObject obj = Results.Objects.FirstOrDefault(o => o.Name == subclass.Key);

            if (obj != null)
            {
                EsoUIObject parent = Results.Objects.FirstOrDefault(o => o.Name == subclass.Value); ;

                if (parent != null)
                {
                    foreach (KeyValuePair<string, EsoUIFunction> func in parent.Functions)
                    {
                        obj.AddFunction(func.Value);
                    }
                }
            }
        }
    }

    private void ScanFile(string fileContent, string filepath)
    {
        string[] lines = fileContent.Split('\n');
        bool insideComment = false;
        Dictionary<string, EsoUIObject> objects = [];

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Replace("\r", "").Trim();
            bool comment = line.StartsWith("--");

            if (line.StartsWith("--[[")) insideComment = true;
            if (line.StartsWith("]]")) insideComment = false;
            // some files have incorrect comment endings
            if (line.StartsWith("--]]")) insideComment = false;

            if (!comment && !insideComment)
            {
                // Match functions
                Match functionMatch = regexService.FunctionMatcher().Match(line);

                if (functionMatch.Success)
                {
                    AddFunction(lines, functionMatch, i);
                }

                // Match objects
                Match objectMatch = regexService.ObjectTypeMatcher().Match(line);

                if (objectMatch.Success)
                {
                    int endOfFunction = FindEndOfFunction(lines, i);
                    AddObject(objects, objectMatch, lines, i, endOfFunction);
                }

                // Match instance
                Match instanceMatch = regexService.InstanceMatcher().Match(line);

                if (instanceMatch.Success)
                {
                    if (objects.TryGetValue(instanceMatch.Groups[2].Value, out EsoUIObject obj))
                    {
                        string name = instanceMatch.Groups[1].Value;
                        obj.AddInstanceName(name);

                        int esouiIndex = filepath.LastIndexOf("esoui", StringComparison.OrdinalIgnoreCase);
                        string path = esouiIndex != -1 ? filepath.Substring(esouiIndex) : string.Empty;

                        Results.InstanceNames.Add(new EsoUIInstance(name, obj.Name, instanceMatch.Groups[0].Value, path));
                    }
                }

                // Match subclasses
                Match subclassMatch = regexService.SubclassMatcher().Match(line);

                if (subclassMatch.Success)
                {
                    _Subclasses.TryAdd(subclassMatch.Groups[1].Value, subclassMatch.Groups[2].Value);
                }
            }
        }

        foreach (EsoUIObject obj in objects.Values)
        {
            Results.Objects.Add(obj);
        }
    }

    private int AddFunction(string[] lines, Match match, int startIndex)
    {
        string functionName = match.Groups[1].Value.Trim();
        string parameters = match.Groups[3].Value;
        EsoUIFunction func = new EsoUIFunction(functionName);

        // Find end of function
        int endPosition = FindEndOfFunction(lines, startIndex);

        // Extract return statement if any
        List<string> returnTypes = ExtractReturnTypes(lines, startIndex, endPosition);
        string returnType = returnTypes.Count > 0 ? string.Join(" /// ", returnTypes) : null;

        foreach (string p in parameters.Split(','))
        {
            func.AddArgument(p.Trim(), "Unknown");
        }

        func.AddReturn(returnType, "Unknown");

        foreach (string codeline in lines.Skip(startIndex).Take(endPosition - startIndex + 1))
        {
            func.AddCode(codeline);
        }

        func.ElementType = APIElementType.FUNCTION;

        Results.Functions.Add(func);

        return endPosition - startIndex;
    }

    private void AddObject(Dictionary<string, EsoUIObject> objects, Match match, string[] lines, int startOfFunction, int endOfFunction)
    {
        string objectName = match.Groups[1].Value.Trim();
        string objectMethod = match.Groups[2].Value;
        string[] objectParameters = match.Groups[3].Value.Split(',');

        // Extract return statement if any
        List<string> returnTypes = ExtractReturnTypes(lines, startOfFunction, endOfFunction);
        string returnType = returnTypes.Count > 0 ? string.Join(" /// ", returnTypes) : null;

        if (!objects.TryGetValue(objectName, out EsoUIObject obj))
        {
            obj = new EsoUIObject(objectName)
            {
                ElementType = APIElementType.OBJECT_TYPE
            };

            objects.Add(objectName, obj);
        }

        EsoUIFunction func = new EsoUIFunction(objectMethod)
        {
            Parent = objectName,
            Returns = returnType == null ? null : [new EsoUIArgument(returnType, new EsoUIType("Unknown"), 1)]
        };

        foreach (string codeline in lines.Skip(startOfFunction).Take(endOfFunction - startOfFunction + 1))
        {
            func.AddCode(codeline);

            if (objectMethod == "New" || objectMethod == "Initialize")
            {
                Match callback = regexService.CallbackObjectMatcher().Match(codeline);
                Match selfAssignment = regexService.SelfAssignmentMatcher().Match(codeline);
                Match instance = callback.Success ? callback : selfAssignment;

                if (instance.Success)
                {
                    obj.InstanceName = instance.Groups[1].Value;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(objectParameters[0]))
        {
            foreach (string p in objectParameters)
            {
                func.AddArgument(p.Trim());
            }
        }

        obj.AddFunction(func);
    }

    private int FindEndOfFunction(string[] lines, int startPosition)
    {
        int depth = 0;

        for (int i = startPosition; i < lines.Length; i++)
        {
            string line = lines[i];

            if (!line.Trim().StartsWith("--"))
            {
                if (regexService.FunctionKeywordMatcher().IsMatch(line) || regexService.IfKeywordMatcher().IsMatch(line) || regexService.DoKeywordMatcher().IsMatch(line))
                {
                    depth++;
                }
                else if (regexService.EndKeywordMatcher().IsMatch(line))
                {
                    depth--;

                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }
        }

        return lines.Length - 1;
    }

    private static List<string> ExtractReturnTypes(string[] lines, int startPosition, int endPosition)
    {
        List<string> returnTypes = [];

        for (int i = startPosition; i <= endPosition; i++)
        {
            string line = lines[i].Trim();

            if (line.StartsWith("return "))
            {
                returnTypes.Add(line.Substring(7));
            }
        }

        return returnTypes;
    }
}

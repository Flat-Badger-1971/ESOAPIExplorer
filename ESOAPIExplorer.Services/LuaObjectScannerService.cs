using ESOAPIExplorer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public class LuaObjectScannerService(IRegexService _regexService) : ILuaObjectScanner
{
    public string FolderPath { get; set; }
    public LuaScanResults Results { get; set; } = new LuaScanResults();
    public string SoundsPath { get; set; }

    private readonly ConcurrentDictionary<string, string> _subclasses = new ConcurrentDictionary<string, string>();
    private readonly ConcurrentDictionary<string, string> _instanceNames = new ConcurrentDictionary<string, string>();

    public void ScanFolderForLuaFunctions(EsoUIObject callbackObject)
    {
        ConcurrentDictionary<string, bool> ignore = new ConcurrentDictionary<string, bool>
        {
            ["internalingame_rewards_manager.lua"] = true
        };

        Parallel.ForEach(Directory.EnumerateFiles(FolderPath, "*.lua", SearchOption.AllDirectories), file =>
        {
            string filename = Path.GetFileName(file);

            if (filename == "soundids.lua")
            {
                SoundsPath = file;
            }
            else if (!ignore.ContainsKey(filename))
            {
                string content = File.ReadAllText(file);
                ScanFile(content, file, filename);
            }
        });

        // Check for subclass methods
        foreach (KeyValuePair<string, string> subclass in _subclasses)
        {
            EsoUIObject obj = Results.Objects.FirstOrDefault(o => o.Name == subclass.Key);

            if (obj != null)
            {
                EsoUIObject parent = subclass.Value == "ZO_CallbackObject" ? callbackObject : Results.Objects.FirstOrDefault(o => o.Name == subclass.Value);

                if (parent != null)
                {
                    foreach (KeyValuePair<string, EsoUIFunction> func in parent.Functions)
                    {
                        obj.AddFunction(func.Value);
                    }
                }

                obj.AddExtends(subclass.Value);
            }
        }

        // Check for instances defined in functions
        foreach (KeyValuePair<string, string> instanceName in _instanceNames)
        {
            EsoUIObject obj = Results.Objects.FirstOrDefault(o => o.Name == instanceName.Value);

            if (obj != null && string.IsNullOrEmpty(obj.InstanceName))
            {
                obj.AddInstanceName(instanceName.Key);
            }
        }

        // Check for colordefs
        EsoUIObject colordef = Results.Objects.FirstOrDefault(o => o.Name == "ZO_ColorDef");

        if (colordef != null)
        {
            foreach (EsoUIObject colorDef in Results.Objects.Where(o => o.ElementType == APIElementType.OBJECT_TYPE && o.Name.EndsWith("_COLOR")))
            {
                foreach (KeyValuePair<string, EsoUIFunction> func in colordef.Functions.Where(f => f.Value.Name != "New"))
                {
                    colorDef.AddFunction(func.Value);
                }
            }
        }
    }

    private void ScanFile(string fileContent, string filepath, string filename)
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
            if (line.StartsWith("--]]")) insideComment = false;
            if (line.StartsWith("--[[") && (line.EndsWith("]]--") || line.EndsWith("]]")))
            {
                insideComment = false;
                comment = true;
            }

            if (!comment && !insideComment)
            {
                // Match functions
                Match functionMatch = _regexService.FunctionMatcher().Match(line);
                if (functionMatch.Success)
                {
                    AddFunction(lines, functionMatch, i);
                }

                // Match objects
                Match objectMatch = _regexService.ObjectTypeMatcher().Match(line);
                if (objectMatch.Success)
                {
                    int endOfFunction = FindEndOfFunction(lines, i);
                    AddObject(objects, objectMatch, lines, i, endOfFunction);
                }

                // Match instance
                Match instanceMatch = _regexService.InstanceMatcher().Match(line);
                if (instanceMatch.Success)
                { 
                    if (instanceMatch.Groups[2].Value.Contains("SHARED_INVENTORY"))
                    {

                    }

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
                Match subclassMatch = _regexService.SubclassMatcher().Match(line);
                if (subclassMatch.Success)
                {
                    _subclasses.TryAdd(subclassMatch.Groups[1].Value, subclassMatch.Groups[2].Value);
                }

                // Match fragments
                Match fragmentMatch = _regexService.FragmentMatcher().Match(line);
                if (fragmentMatch.Success)
                {
                    Results.Fragments.TryAdd(fragmentMatch.Groups[1].Value, true);
                }

                // Match aliases
                if (filename == "addoncompatibilityaliases.lua" || filename == "globalapi.lua")
                {
                    Match aliasMatch = _regexService.AliasMatcher().Match(line);
                    if (aliasMatch.Success)
                    {
                        string name = aliasMatch.Groups[1].Value.Trim();
                        if (ConstantValues.GetConstantValue(name) == null)
                        {
                            EsoUIObject esoobject = new EsoUIObject(name, false)
                            {
                                ElementType = APIElementType.ALIAS
                            };
                            esoobject.AddCode(aliasMatch.Groups[0].Value.Trim());
                            Results.Objects.Add(esoobject);
                        }
                    }
                }

                // Match colordefs
                Match colorDefMatch = _regexService.ColorDefMatcher().Match(line);

                if (colorDefMatch.Success)
                {
                    string name = colorDefMatch.Groups[1].Value.Trim();
                    EsoUIObject esoobject = new EsoUIObject(name, false)
                    {
                        ElementType = APIElementType.OBJECT_TYPE,
                        InstanceName = name,
                        Code = [colorDefMatch.Groups[0].Value.Trim()],
                        Functions = [],
                        Name = name
                    };

                    Results.Objects.Add(esoobject);
                }
            }
        }

        foreach (EsoUIObject obj in objects.Values)
        {
            Results.Objects.Add(obj);
        }
    }

    private void AddFunction(string[] lines, Match match, int startIndex)
    {
        string functionName = match.Groups[1].Value.Trim();

        if (functionName != "all")
        {
            string parameters = match.Groups[2].Value;
            EsoUIFunction func = new EsoUIFunction(functionName);

            // Find end of function
            int endPosition = FindEndOfFunction(lines, startIndex);

            // Extract return statements if any
            if (func.Name == "Subclass" && !string.IsNullOrEmpty(func.Parent))
            {
                func.AddReturn("", func.Parent);
            }
            else
            {
                List<EsoUIReturn> returns = ExtractReturns(lines, startIndex, endPosition);
                func.AddReturns(returns);
            }

            if (!string.IsNullOrWhiteSpace(parameters))
            {
                foreach (string p in parameters.Split(','))
                {
                    string pt = p.Replace(" : ", "_").Trim();
                    func.AddArgument(pt, Utility.InferType(pt, _regexService));
                }
            }

            foreach (string codeline in lines.Skip(startIndex).Take(endPosition - startIndex + 1))
            {
                func.AddCode(codeline);

                Match instanceMatch = _regexService.InstanceMatcher().Match(codeline);
                if (instanceMatch.Success)
                {
                    string name = instanceMatch.Groups[1].Value;
                    string objectName = instanceMatch.Groups[2].Value;
                    _instanceNames.TryAdd(name, objectName);
                }
            }

            func.ElementType = APIElementType.FUNCTION;
            Results.Functions.Add(func);
        }
    }

    private void AddObject(Dictionary<string, EsoUIObject> objects, Match match, string[] lines, int startOfFunction, int endOfFunction)
    {
        string objectName = match.Groups[1].Value.Trim();
        string objectMethod = match.Groups[2].Value;
        string[] objectParameters = match.Groups[3].Value.Split(',');

        // Extract return statement if any
        List<EsoUIReturn> returns = ExtractReturns(lines, startOfFunction, endOfFunction);
        EsoUIFunction func = new EsoUIFunction(objectMethod)
        {
            ElementType = APIElementType.OBJECT_METHOD,
            Parent = objectName,
            Returns = returns
        };

        if (!objects.TryGetValue(objectName, out EsoUIObject obj))
        {
            obj = new EsoUIObject(objectName)
            {
                ElementType = APIElementType.OBJECT_TYPE,
                Code = [match.Groups[0].Value.Replace("function ", "")]
            };

            objects.Add(objectName, obj);
        }

        string firstLine = lines[startOfFunction].Trim();

        foreach (string codeline in lines.Skip(startOfFunction).Take(endOfFunction - startOfFunction + 1))
        {
            func.AddCode(codeline);

            if (objectMethod == "New" || objectMethod == "Initialize")
            {
                Match callback = _regexService.CallbackObjectMatcher().Match(codeline);
                Match selfAssignment = _regexService.SelfAssignmentMatcher().Match(codeline);
                Match instance = callback.Success ? callback : selfAssignment;

                if (instance.Success)
                {
                    obj.AddInstanceName(instance.Groups[1].Value);
                }
            }

            if (objectMethod == "Subclass" || objectMethod == "New" || objectMethod == "Clone")
            {
                if (codeline.Trim() == firstLine)
                {
                    func.SetReturn(objectName);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(objectParameters[0]))
        {
            foreach (string p in objectParameters)
            {
                func.AddArgument(p.Trim(), Utility.InferType(p.Trim(), _regexService));
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
                if (_regexService.FunctionKeywordMatcher().IsMatch(line) || _regexService.IfKeywordMatcher().IsMatch(line) || _regexService.DoKeywordMatcher().IsMatch(line))
                {
                    depth++;
                }
                else if (_regexService.EndKeywordMatcher().IsMatch(line))
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

    private int FindEndOfLocalFunction(string[] lines, int startPosition)
    {
        int depth = 1;

        for (int i = startPosition; i < lines.Length; i++)
        {
            string line = lines[i];

            if (!line.Trim().StartsWith("--"))
            {
                if (_regexService.IfKeywordMatcher().IsMatch(line) || _regexService.DoKeywordMatcher().IsMatch(line))
                {
                    depth++;
                }
                else if (_regexService.EndKeywordMatcher().IsMatch(line))
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

    private static List<string> SplitReturnValues(string values)
    {
        List<string> returnValues = [];
        StringBuilder stringBuilder = new();
        int valueCount = 0;
        int objCount = 0;
        bool isObject = false;

        foreach (string value in values.Split(','))
        {
            valueCount += value.Count(c => c == '(');
            valueCount -= value.Count(c => c == ')');
            objCount += value.Count(c => c == '{');
            objCount -= value.Count(c => c == '}');

            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append(", ");
            }

            stringBuilder.Append(value.Trim());

            if (objCount > 0)
            {
                isObject = true;
            }

            if (isObject && objCount == 0)
            {
                returnValues.Clear();
                returnValues.Add("data");

                return returnValues;
            }
            else if (valueCount == 0)
            {
                returnValues.Add(stringBuilder.ToString());
                stringBuilder.Clear();
            }
        }

        return returnValues;
    }

    private List<EsoUIReturn> ExtractReturns(string[] lines, int startPosition, int endPosition)
    {
        List<EsoUIReturn> returns = [];
        int returnIndex = 0;
        bool insideReturnBlock = false;
        StringBuilder returnBlock = new();

        for (int i = startPosition; i <= endPosition; i++)
        {
            if (lines[i].Contains("local function"))
            {
                i = FindEndOfLocalFunction(lines, i);
                continue;
            }

            string line = lines[i].Trim();

            if (line.StartsWith("return "))
            {
                insideReturnBlock = true;
                returnBlock.Append(line.Substring(7).Trim());
            }
            else if (insideReturnBlock)
            {
                returnBlock.Append(line.Trim());

                if (line.EndsWith('}') || line.EndsWith("},") || line.EndsWith(',') || string.IsNullOrWhiteSpace(line))
                {
                    if (!line.EndsWith(',') && !string.IsNullOrWhiteSpace(line))
                    {
                        insideReturnBlock = false;
                    }

                    if (!insideReturnBlock || i == endPosition)
                    {
                        EsoUIReturn ret = new()
                        {
                            Index = returnIndex++,
                            Values = SplitReturnValues(returnBlock.ToString())
                                .Select((split, index) => new EsoUIArgument(split, new EsoUIType(Utility.InferType(split, _regexService)), index + 1))
                                .ToList()
                        };
                        returns.Add(ret);
                        returnBlock.Clear();
                    }
                }
            }
            else if (line.StartsWith("return"))
            {
                insideReturnBlock = true;
            }
        }

        return returns;
    }
}

using ESOAPIExplorer.Models;
using Microsoft.UI.Xaml.Shapes;
using System;
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

    public void ScanFolderForLuaFunctions()
    {
        Parallel.ForEach(Directory.EnumerateFiles(FolderPath, "*.lua", SearchOption.AllDirectories), file =>
        {
            string content = File.ReadAllText(file);
            ScanFile(content, file);
        });
    }

    private void ScanFile(string fileContent, string filepath)
    {
        string[] lines = fileContent.Split('\n');
        Dictionary<string, EsoUIObject> objects = [];

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Replace("\r", "");

            // Match functions
            Match functionMatch = regexService.FunctionMatcher().Match(line);

            if (functionMatch.Success)
            {
                AddFunction(lines, functionMatch, i);
                continue;
            }

            // Match objects
            Match objectMatch = regexService.ObjectTypeMatcher().Match(line);

            if (objectMatch.Success)
            {
                int endOfFunction = FindEndOfFunction(lines, i);
                AddObject(objects, objectMatch, lines, i, endOfFunction);
                i = endOfFunction + 1;
                continue;
            }

            // Match alias
            Match aliasMatch = regexService.AliasMatcher().Match(line);

            if (aliasMatch.Success)
            {
                if (objects.TryGetValue(aliasMatch.Groups[2].Value, out EsoUIObject obj))
                {
                    string name = aliasMatch.Groups[1].Value;

                    obj.AddInstanceName(name);

                    int esouiIndex = filepath.LastIndexOf("esoui", StringComparison.OrdinalIgnoreCase);
                    string path = string.Empty;

                    if (esouiIndex != -1)
                    {
                        path = filepath.Substring(esouiIndex);
                    }

                    Results.InstanceNames.Add(new EsoUIInstance(name, obj.Name, aliasMatch.Groups[0].Value, path));
                }
            }
        }

        if (objects.Count > 0)
        {
            foreach (KeyValuePair<string, EsoUIObject> obj in objects)
            {
                Results.Objects.Add(obj.Value);
            }
        }
    }

    private void AddFunction(string[] lines, Match match, int startIndex)
    {
        string functionName = match.Groups[1].Value;
        string parameters = match.Groups[3].Value;

        EsoUIFunction func = new(functionName);

        // Find end of function
        int endPosition = FindEndOfFunction(lines, startIndex);

        // Extract return statement if any
        string returnType = ExtractReturnType(lines, startIndex, endPosition);

        foreach (string p in parameters.Split(','))
        {
            func.AddArgument(p.Trim(), "Unknown");
        }

        func.AddReturn(returnType, "Unknown");

        foreach (string codeline in lines.Skip(startIndex).Take(endPosition - startIndex + 1))
        {
            func.AddCode(codeline);
        }

        Results.Functions.Add(func);
    }

    private static void AddObject(Dictionary<string, EsoUIObject> objects, Match match, string[] lines, int startOfFunction, int endOfFunction)
    {
        string objectName = match.Groups[1].Value;
        string objectMethod = match.Groups[2].Value;
        string[] objectParameters = match.Groups[3].Value.Split(',');

        // Extract return statement if any
        string returnType = ExtractReturnType(lines, startOfFunction, endOfFunction);

        if (!objects.TryGetValue(objectName, out EsoUIObject obj))
        {
            obj = new EsoUIObject(objectName);
            objects.Add(objectName, obj);
        }

        EsoUIFunction func = new(objectMethod)
        {
            Parent = objectName,
            Returns = [new EsoUIArgument(returnType, new EsoUIType("Unknown"), 1)]
        };

        foreach (string codeline in lines.Skip(startOfFunction).Take(endOfFunction - startOfFunction + 1))
        {
            func.AddCode(codeline);
        }

        foreach (string p in objectParameters)
        {
            func.AddArgument(p.Trim());
        }

        obj.AddFunction(func);
    }

    private static void AddAlias(Dictionary<string, EsoUIObject> objects, string objectName, string alias)
    {
        if (objects.TryGetValue(objectName, out EsoUIObject obj))
        {
            obj.AddInstanceName(alias);
        }
    }

    private int FindEndOfFunction(string[] lines, int startPosition)
    {
        int depth = 0;

        for (int i = startPosition; i < lines.Length; i++)
        {
            string line = lines[i];

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

        return lines.Length - 1;
    }

    private static string ExtractReturnType(string[] lines, int startPosition, int endPosition)
    {
        for (int i = startPosition; i <= endPosition; i++)
        {
            string line = lines[i].Trim();

            if (line.StartsWith("return "))
            {
                return line.Substring(7);
            }
        }

        return null;
    }
}
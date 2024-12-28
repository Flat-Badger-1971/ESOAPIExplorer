using ESOAPIExplorer.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public class LuaObjectScanneService(IRegexService _RegexService) : ILuaObjectScanner
{
    public string FolderPath { get; set; }
    public LuaScanResults Results { get; set; } = new LuaScanResults();

    public void ScanFolderForLuaFunctions()
    {
        Parallel.ForEach(Directory.EnumerateFiles(FolderPath, "*.lua", SearchOption.AllDirectories), file =>
        {
            string content = File.ReadAllText(file);
            ScanFile(content);
        });
    }

    private void ScanFile(string fileContent)
    {
        string[] lines = fileContent.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // functions
            Match match = _RegexService.FunctionMatcher().Match(line);

            if (match.Success)
            {
                string functionName = match.Groups[1].Value;
                string parameters = match.Groups[3].Value;

                EsoUIFunction func = new EsoUIFunction(functionName);

                // Find end of function
                int startPosition = i;
                int endPosition = FindEndOfFunction(lines, i);

                // Extract return statement if any
                string returnType = ExtractReturnType(lines, startPosition, endPosition);

                foreach (string p in parameters.Split(','))
                {
                    func.AddArgument(p.Trim(), "Unknown");
                }

                func.AddReturn(returnType, "Unknown");

                foreach (string codeline in lines.Skip(startPosition).Take(endPosition - startPosition + 1))
                {
                    func.AddCode(codeline);
                }

                Results.Functions.Add(func);
            }

            //// globals
            match = _RegexService.GlobalMatcher().Match(line);

            if (match.Success)
            {
                string globalName = match.Groups[1].Value;
                string globalValue = match.Groups[3].Value;
                string globalType = "unknown";

                if (globalValue.Trim() != "{")
                {
                    if (double.TryParse(globalValue, out double _))
                    {
                        globalType = "number";
                    }

                    if (int.TryParse(globalValue, out int _))
                    {
                        globalType = "integer";
                    }

                    if (globalValue.Contains('"'))
                    {
                        globalType = "string";
                        globalValue = globalValue.Replace("\"", "");
                    }

                    EsoUIGlobal global = new EsoUIGlobal
                    {
                        Name = globalName,
                        StringValue = globalValue,
                        Type = globalType
                    };

                    Results.Globals.Add(global);
                }
            }
        }
    }

    private int FindEndOfFunction(string[] lines, int startPosition)
    {
        int depth = 0;

        for (int i = startPosition; i < lines.Length; i++)
        {
            switch (true)
            {
                case true when _RegexService.FunctionKeywordMatcher().IsMatch(lines[i]):
                case true when _RegexService.IfKeywordMatcher().IsMatch(lines[i]):
                case true when _RegexService.DoKeywordMatcher().IsMatch(lines[i]):
                    depth++;
                    break;
                case true when _RegexService.EndKeywordMatcher().IsMatch(lines[i]):
                    depth--;
                    break;
            }

            if (depth == 0) return i;
        }

        return lines.Length - 1;
    }

    private static string ExtractReturnType(string[] lines, int startPosition, int endPosition)
    {
        for (int i = startPosition; i <= endPosition; i++)
        {
            string line = lines[i];

            if (line.Trim().StartsWith("return "))
            {
                string returnValue = line.Trim().Substring(7);

                return returnValue;
            }
        }

        return null;
    }
}

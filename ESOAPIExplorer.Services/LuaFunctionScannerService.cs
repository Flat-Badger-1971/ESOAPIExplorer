using ESOAPIExplorer.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ESOAPIExplorer.Services;

public class LuaFunctionScanner(IRegexService _RegexService) : ILuaFunctionScanner
{
    public string FolderPath { get; set; }
    private readonly Dictionary<string, LuaFunctionDetails> _FunctionDetailsList = [];

    public IDictionary<string, LuaFunctionDetails> ScanFolderForLuaFunctions()
    {

        foreach (string file in Directory.EnumerateFiles(FolderPath, "*.lua", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(file);
            ScanFileForFunctions(content);
        }

        return _FunctionDetailsList;
    }

    private void ScanFileForFunctions(string fileContent)
    {
        string[] lines = fileContent.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            Match match = _RegexService.FunctionMatcher().Match(line);

            if (match.Success)
            {
                string functionName = match.Groups[1].Value;
                string parameters = match.Groups[3].Value;

                // Find end of function
                int startPosition = i;
                int endPosition = FindEndOfFunction(lines, i);

                // Extract return statement if any
                string returnType = ExtractReturnType(lines, startPosition, endPosition);

                _FunctionDetailsList[functionName] = (new LuaFunctionDetails
                {
                    Name = functionName,
                    Parameters = parameters,
                    ReturnType = returnType,
                    Code = lines.Skip(startPosition).Take(endPosition - startPosition + 1)
                });
            }
        }
    }

    private int FindEndOfFunction(string[] lines, int startPosition)
    {
        int depth = 0;

        for (int i = startPosition; i < lines.Length; i++)
        {
            switch (true) {
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

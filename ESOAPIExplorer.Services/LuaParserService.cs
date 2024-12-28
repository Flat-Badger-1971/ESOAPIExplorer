using ESOAPIExplorer.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ESOAPIExplorer.Services;

public class LuaParserService(IRegexService regexService) : ILuaParserService
{
    public Dictionary<string, EsoUIGlobalValue> ParseLuaContent(string luaContent)
    {
        Dictionary<string, EsoUIGlobalValue> result = [];
        Match match = regexService.SectionMatcher().Match(luaContent);

        while (match.Success)
        {
            string section = match.Groups[1].Value;
            string content = match.Groups[2].Value;

            if (section.EndsWith("Constants") || section.EndsWith("SI_String_Constants"))
            {
                MatchCollection constantMatches = regexService.ConstantMatcher().Matches(content);

                foreach (Match constantMatch in constantMatches)
                {
                    string key = constantMatch.Groups[1].Value;
                    string value = constantMatch.Groups[2].Value;

                    EsoUIGlobalValue globalValue = new EsoUIGlobalValue
                    {
                        Name = key,
                        Code = $"{key} = {value}"
                    };

                    if (int.TryParse(value, out int intValue))
                    {
                        globalValue.IntValue = intValue;
                    }
                    else if (double.TryParse(value, out double doubleValue))
                    {
                        globalValue.DoubleValue = doubleValue;
                    }
                    else
                    {
                        globalValue.StringValue = value.Trim('"');
                    }

                    result[key] = globalValue;
                }
            }

            match = match.NextMatch();
        }

        return result;
    }
}


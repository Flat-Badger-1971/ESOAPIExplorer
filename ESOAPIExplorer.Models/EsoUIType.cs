using System.Text.RegularExpressions;

namespace ESOAPIExplorer.Models;

public partial class EsoUIType
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsObject { get; set; } = false;

    public EsoUIType(string name, string type = null)
    {
        if (name.StartsWith('['))
        {
            Match match = TypeMatcher().Match(name);

            Name = match.Groups[1].Value ?? name;
            Type = match.Groups[2].Value ?? name;
        }
        else
        {
            Name = name;
            Type = type ?? name;
        }
    }

    [GeneratedRegex(@"\[(.+)\|\#(.+)\]", RegexOptions.Compiled)]
    private static partial Regex _TypeMatcher();
    public static Regex TypeMatcher() => _TypeMatcher();
}

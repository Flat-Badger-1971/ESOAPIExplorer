using System.Text.RegularExpressions;

namespace ESOAPIExplorer.Models;

public class EsoUIType
{
    public string Name { get; set; }
    public string Type { get; set; }

    public EsoUIType(string name, string type = null)
    {
        if (name.StartsWith("["))
        {
            Match match = Regex.Match(name, @"\[(.+)\|\#(.+)\]");

            Name = match.Groups[1].Value ?? name;
            Type = match.Groups[2].Value ?? name;
        }
        else
        {
            Name = name;
            Type = type ?? name;
        }
    }
}

using System.Data.Common;

namespace ESOAPIExplorer.Models;

public class EsoUIArgument
{
    public int Id { get; set; }
    public string Name { get; set; }
    public EsoUIType Type { get; set; }

    public EsoUIArgument(string name, EsoUIType type, int id)
    {
        Id = id;
        Name = name;
        Type = type;
    }
}

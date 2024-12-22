namespace ESOAPIExplorer.Models;

public class EsoUIArgument(string name, EsoUIType type, int id)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public EsoUIType Type { get; set; } = type;
}

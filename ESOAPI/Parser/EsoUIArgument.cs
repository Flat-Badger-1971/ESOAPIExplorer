namespace ESOAPI.Parser;

public class EsoUIArgument
{
    public string Name { get; set; }
    public EsoUIType Type { get; set; }

    public EsoUIArgument(string name, EsoUIType type)
    {
        Name = name;
        Type = type;
    }
}

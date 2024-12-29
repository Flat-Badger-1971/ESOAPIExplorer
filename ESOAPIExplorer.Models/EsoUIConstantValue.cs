namespace ESOAPIExplorer.Models;

public class EsoUIConstantValue
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Type { get; set; }

    public EsoUIConstantValue() { }

    public EsoUIConstantValue(string name, EsoUIGlobalValue value)
    {
        Name = name;
        Value = value?.DisplayValue;
        Type = value?.ValueType();
    }
}

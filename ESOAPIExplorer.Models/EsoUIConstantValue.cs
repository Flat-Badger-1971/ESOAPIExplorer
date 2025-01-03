namespace ESOAPIExplorer.Models;

public class EsoUIConstantValue
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Type { get; set; }
    public string StringValue { get; set; }

    public EsoUIConstantValue() { }

    public EsoUIConstantValue(string name, EsoUIGlobalValue value)
    {
        Name = name;
        StringValue = value.DisplayString;
        Type = value?.ValueType();
        Value = value?.DisplayValue;
    }
}

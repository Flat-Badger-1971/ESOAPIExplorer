namespace ESOAPIExplorer.Models;

public class EsoUIGlobalConstantValue(string name, EsoUIGlobalValue value)
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value?.DisplayValue;
    public string Type { get; set; } = value?.ValueType();
}

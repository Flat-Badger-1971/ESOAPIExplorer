namespace ESOAPIExplorer.Models;

public class EsoUIGlobalValue
{
    private string _displayValue;

    public string Name { get; set; }
    public int? IntValue { get; set; }
    public double? DoubleValue { get; set; }
    public string StringValue {  get; set; }
    public string Code { get; set; }

    public string ValueType()
    {
        if (IntValue.HasValue)
        {
            return "integer";
        }

        if (DoubleValue.HasValue)
        {
            return "number";
        }

        return "string";
    }

    public string DisplayValue
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_displayValue))
            {
                return _displayValue;
            }

            string type = ValueType();

            switch (type)
            {
                case "integer":
                    return IntValue.Value.ToString();
                case "number":
                    return DoubleValue.Value.ToString();
                default:
                    return StringValue;
            }
        }

        set
        {
            _displayValue = value;
        }
    }
}

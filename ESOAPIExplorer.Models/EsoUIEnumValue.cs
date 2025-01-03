﻿namespace ESOAPIExplorer.Models;

public class EsoUIEnumValue
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Type { get; set; }

    public EsoUIEnumValue() { }

    public EsoUIEnumValue(string name, EsoUIGlobalValue value)
    {
        Name = name;
        Value = value?.DisplayValue;
        Type = value?.ValueType();
    }
}

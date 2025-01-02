using ESOAPIExplorer.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ESOAPIExplorer.ViewModels;
public static class CodeGenerator
{
    public static void GenerateClassFile(Dictionary<string, EsoUIGlobalValue> dictionary, string outputPath)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Concurrent;");
        sb.AppendLine();
        sb.AppendLine("namespace ESOAPIExplorer.Models;");
        sb.AppendLine();
        sb.AppendLine("public static class ConstantValues");
        sb.AppendLine("{");
        sb.AppendLine("    public static ConcurrentDictionary<string, EsoUIGlobalValue> Values { get => _Values; }");
        sb.AppendLine("    private static ConcurrentDictionary<string, EsoUIGlobalValue> _Values;");
        sb.AppendLine($"    private const int _NumEntries = {dictionary.Count};");
        sb.AppendLine();
        sb.AppendLine("    public static void PopulateDictionary()");
        sb.AppendLine("    {");

        foreach (KeyValuePair<string, EsoUIGlobalValue> kvp in dictionary)
        {
            string code = $" Code = \"{kvp.Key} = ";

            sb.Append($"        _Values.TryAdd(\"{kvp.Key}\", new EsoUIGlobalValue ");
            sb.Append("{ Name = \"");
            sb.Append($"{kvp.Key}\", ");
            
            if (kvp.Value.IntValue.HasValue)
            {
                sb.Append($"IntValue = {kvp.Value.IntValue},");
                code = $"{code}{kvp.Value.IntValue}";
            }
            else if (kvp.Value.DoubleValue.HasValue)
            {
                sb.Append($"DoubleValue = {kvp.Value.DoubleValue},");
                code = $"{code}{kvp.Value.DoubleValue}";
            }
            else if (!string.IsNullOrEmpty(kvp.Value.StringValue))
            {
                sb.Append($"StringValue = \"{kvp.Value.StringValue}\",");
                code = $"{code}\\\"{kvp.Value.StringValue}\\\"";
            }

            sb.Append($" {code}\"");
            sb.AppendLine("});");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        AddInitialiser(sb);
        sb.AppendLine();
        AddRetreiver(sb);

        sb.AppendLine("}");

        File.WriteAllText(outputPath, sb.ToString());
    }

    private static void AddInitialiser(StringBuilder sb)
    {
        sb.AppendLine("    public static void InitialiseConstants()");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_Values == null)");
        sb.AppendLine("        {");
        sb.AppendLine("            int initialCapacity = Utility.NextPrime(_NumEntries);");
        sb.AppendLine("            int concurrencyLevel = Environment.ProcessorCount * 2;");
        sb.AppendLine("            _Values = new ConcurrentDictionary<string, EsoUIGlobalValue>(concurrencyLevel, initialCapacity);");
        sb.AppendLine("            PopulateDictionary();");
        sb.AppendLine("         }");
        sb.AppendLine("    }");
    }

    private static void AddRetreiver(StringBuilder sb)
    {
        sb.AppendLine("    public static EsoUIGlobalValue GetConstantValue(string constant)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_Values.TryGetValue(constant, out EsoUIGlobalValue value))");
        sb.AppendLine("        {");
        sb.AppendLine("            return value;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
    }
}

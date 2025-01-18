using ESOAPIExplorer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESOAPIExplorer.Services;

public class LuaCheckRcGeneratorService(IESODocumentationService esoDocumentationService) : ILuaCheckRcGeneratorService
{
    private static void AddObject(StringBuilder rc, string name, EsoUIObject obj)
    {
        rc.AppendLine($"    [\"{name}\"] = {{");
        rc.AppendLine("        fields = {");

        foreach (string func in obj.FunctionList)
        {
            rc.AppendLine($"            {func} = {{read_only = true}},");
        }

        rc.AppendLine("        }");
        rc.AppendLine("    },");
    }

    public StringBuilder Generate()
    {
        Dictionary<string, bool> Added = [];

        StringBuilder rc = new StringBuilder("std = \"max\"\n");
        rc.AppendLine("max_line_length = 160\n");
        rc.AppendLine("read_globals = {");

        // Objects
        EsoUIDocumentation docs = esoDocumentationService.Documentation;
        rc.AppendLine($"    -- API Version {docs.ApiVersion}");
        rc.AppendLine("    -- Objects");

        foreach (KeyValuePair<string, EsoUIObject> obj in docs.Objects.Where(o => o.Value.ElementType != APIElementType.ALIAS))
        {
            string instanceName = obj.Value.InstanceName;

            if (!string.IsNullOrWhiteSpace(instanceName))
            {
                if (!Added.ContainsKey(instanceName))
                {
                    AddObject(rc, instanceName, obj.Value);
                    Added[instanceName] = true;
                }
            }

            if (!Added.ContainsKey(obj.Key))
            {
                AddObject(rc, obj.Key, obj.Value);
                Added[obj.Key] = true;
            }
        }

        // Constants
        rc.AppendLine("    -- Constants");

        foreach (EsoUIEnumValue constant in docs.Globals.Where(g => g.Key != "Globals").SelectMany(i => i.Value))
        {
            rc.AppendLine($"    \"{constant.Name}\",");
        }

        foreach (KeyValuePair<string, EsoUIConstantValue> constant in docs.Constants)
        {
            rc.AppendLine($"    \"{constant.Key}\",");
        }

        // Functions
        rc.AppendLine("    -- Functions");

        foreach (KeyValuePair<string, EsoUIFunction> func in docs.Functions)
        {
            rc.AppendLine($"    \"{func.Key}\",");
        }

        // Events
        rc.AppendLine("    -- Events");

        foreach (KeyValuePair<string, EsoUIEvent> esoevent in docs.Events)
        {
            rc.AppendLine($"    \"{esoevent.Key}\",");
        }

        // Fragments
        rc.AppendLine("    -- Fragments");

        foreach (KeyValuePair<string, bool> fragment in docs.Fragments)
        {
            rc.AppendLine($"    \"{fragment.Key}\",");
        }

        // Aliases
        rc.AppendLine("    -- Aliases");

        foreach (KeyValuePair<string, EsoUIObject> alias in docs.Objects.Where(o => o.Value.ElementType == APIElementType.ALIAS))
        {
            rc.AppendLine($"    \"{alias.Key}\",");
        }

        // Miscellanous
        //rc.AppendLine("    -- Miscellanous");
        //rc.AppendLine("    \"unpack\",");

        rc.AppendLine("}");

        return rc;
    }
}

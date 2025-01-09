using ESOAPIExplorer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESOAPIExplorer.Services;

public class LuaCheckRcGeneratorService(IESODocumentationService esoDocumentationService) : ILuaCheckRcGeneratorService
{
    public StringBuilder Generate()
    {
        Dictionary<string, bool> Added = [];

        StringBuilder rc = new StringBuilder("std = \"min\"\n");
        rc.AppendLine("max_line_length = 160\n");
        rc.AppendLine("read_globals = {");

        // Objects
        EsoUIDocumentation docs = esoDocumentationService.Documentation;
        rc.AppendLine($"    -- API Version {docs.ApiVersion}");
        rc.AppendLine("    -- Objects");

        foreach (var obj in docs.Objects)
        {
            string instanceName = !string.IsNullOrWhiteSpace(obj.Value.InstanceName) ? obj.Value.InstanceName : obj.Key;

            if (!Added.ContainsKey(instanceName))
            {
                rc.AppendLine($"    [\"{instanceName}\"] = {{");
                rc.AppendLine("        fields = {");

                foreach (string func in obj.Value.FunctionList)
                {
                    rc.AppendLine($"            {func} = {{read_only = true}},");
                }

                rc.AppendLine("        }");
                rc.AppendLine("    },");

                Added[instanceName] = true;
            }
        }

        // Constants
        rc.AppendLine("    -- Constants");

        foreach (EsoUIEnumValue constant in docs.Globals.Where(g => g.Key != "Globals").SelectMany(i => i.Value))
        {
            rc.AppendLine($"    \"{constant.Name}\",");
        }

        foreach (System.Collections.Generic.KeyValuePair<string, EsoUIConstantValue> constant in docs.Constants)
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

        // Miscellanous
        rc.AppendLine("    -- Miscellanous");
        rc.AppendLine("    \"unpack\",");

        rc.AppendLine("}");

        return rc;
    }
}

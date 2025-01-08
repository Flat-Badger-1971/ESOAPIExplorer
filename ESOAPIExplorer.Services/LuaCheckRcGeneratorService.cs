using ESOAPIExplorer.Models;
using System.Collections.Generic;
using System.Text;

namespace ESOAPIExplorer.Services;

public class LuaCheckRcGeneratorService(IESODocumentationService esoDocumentationService) : ILuaCheckRcGeneratorService
{
    public void Generate()
    {
        StringBuilder rc = new StringBuilder("std = \"min\"");

        rc.AppendLine("max_line_length = 160");
        rc.AppendLine("globals = {\"_G\"}");
        rc.AppendLine();
        rc.AppendLine("read_globals = {");

        // TODO: Add all aentries
        // Objects
        EsoUIDocumentation docs = esoDocumentationService.Documentation;

        foreach (KeyValuePair<string, EsoUIObject> obj in docs.Objects)
        {
            rc.AppendLine($"    [\"{obj.Key}\"] = {{");
            rc.AppendLine("        fields = {");

            foreach (string func in obj.Value.FunctionList)
            {
                rc.AppendLine($"        {func} = {{read_only = true}},");
            }

            rc.AppendLine("        }");
            rc.AppendLine("    },");
        }

        // constants

        // functions

        // events

        // misc

        rc.AppendLine("}");
    }
}

using ESOAPIExplorer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESOAPIExplorer.Services;

public class LuaLanguageServerDefinitionsGeneratorService(IESODocumentationService esoDocumentationService) : ILuaLanguageServerDefinitionsGeneratorService
{
    public StringBuilder Generate()
    {
        StringBuilder definitions = new StringBuilder("---@meta");

        // Objects
        EsoUIDocumentation docs = esoDocumentationService.Documentation;
        definitions.AppendLine($"-- API Version {docs.ApiVersion}");
        definitions.AppendLine("-- Objects");

        foreach (KeyValuePair<string, EsoUIObject> obj in docs.Objects.Where(o => o.Value.ElementType != APIElementType.ALIAS))
        {
            string instanceName = obj.Value.InstanceName;

            if (!string.IsNullOrWhiteSpace(instanceName))
            {
                AddObject(definitions, instanceName, obj.Value);
            }

            AddObject(definitions, obj.Key, obj.Value);
        }

        // Constants
        definitions.AppendLine("-- Constants");

        foreach (EsoUIEnumValue constant in docs.Globals.Where(g => g.Key != "Globals").SelectMany(i => i.Value))
        {
            definitions.AppendLine($"---@type {constant.Name}");
        }

        foreach (KeyValuePair<string, EsoUIConstantValue> constant in docs.Constants)
        {
            definitions.AppendLine($"---@type {constant.Key}");
        }

        // Functions
        definitions.AppendLine("-- Functions");

        foreach (KeyValuePair<string, EsoUIFunction> func in docs.Functions)
        {
            definitions.AppendLine($"---@function {func.Key}");
        }

        // Events
        definitions.AppendLine("-- Events");

        foreach (KeyValuePair<string, EsoUIEvent> esoevent in docs.Events)
        {
            definitions.AppendLine($"---@event {esoevent.Key}");
        }

        // Fragments
        definitions.AppendLine("-- Fragments");

        foreach (KeyValuePair<string, bool> fragment in docs.Fragments)
        {
            definitions.AppendLine($"---@fragment {fragment.Key}");
        }

        // Aliases
        definitions.AppendLine("-- Aliases");

        foreach (KeyValuePair<string, EsoUIObject> alias in docs.Objects.Where(o => o.Value.ElementType == APIElementType.ALIAS))
        {
            definitions.AppendLine($"---@alias {alias.Key}");
        }

        return definitions;
    }

    private static void AddObject(StringBuilder definitions, string name, EsoUIObject obj)
    {
        definitions.AppendLine($"---@class {name}");

        foreach (string func in obj.FunctionList)
        {
            definitions.AppendLine($"---@field {func} function");
        }

        definitions.AppendLine();
    }
}

using System.Text.RegularExpressions;

namespace ESOAPIExplorer.Services;

public partial class RegexService : IRegexService
{
    [GeneratedRegex(@"(\d+)$")]
    private static partial Regex _ApiVersionMatcher();

    [GeneratedRegex(@"\*(.+)\* _(.+)_")]
    private static partial Regex _ArgumentMatcher();

    [GeneratedRegex(@"\[""(.*?)""\]\s*=\s*(.*?),")]
    private static partial Regex _ConstantMatcher();

    [GeneratedRegex(@"\bdo\b")]
    private static partial Regex _DoKeywordMatcher();

    [GeneratedRegex(@"\bend\b")]
    private static partial Regex _EndKeywordMatcher();

    [GeneratedRegex(@"\* (.+)$")]
    private static partial Regex _EnumMatcher();

    [GeneratedRegex(@"h5\. (.+)$")]
    private static partial Regex _EnumNameMatcher();

    [GeneratedRegex(@"^\*\ ([^\s]+)(\s\((.+)\))?")]
    private static partial Regex _EventMatcher();

    [GeneratedRegex(@"([^\s]+)( \*(.+)\*)?")]
    private static partial Regex _FunctionAccessMatcher();

    [GeneratedRegex(@"\bfunction\b")]
    private static partial Regex _FunctionKeywordMatcher();

    [GeneratedRegex(@"function\s+((ZO_|zo_)\w+)\((.*)\)")]
    private static partial Regex _FunctionMatcher();

    [GeneratedRegex(@"^\* (.+)\((.*)\)$")]
    private static partial Regex _FunctionNameMatcher();

    [GeneratedRegex(@"\*\*\ _Returns:_ (.+)$")]
    private static partial Regex _FunctionReturnMatcher();

    [GeneratedRegex(@"^((ZO_|zo_)\w+)\s=\s(.+)")]
    private static partial Regex _GlobalMatcher();

    [GeneratedRegex(@"\bif\b")]
    private static partial Regex _IfKeywordMatcher();

    [GeneratedRegex(@"h3\. (.+)$")]
    private static partial Regex _ObjectNameMatcher();

    [GeneratedRegex(@"^function (.+):(.+)\(([.\.]+)\)")]
    private static partial Regex _ObjectTypeMatcher();

    [GeneratedRegex(@"\[""(Constants|SI_String_Constants)""\]\s*=\s*{\s*((?:.*?,\s*)+)\s*}", RegexOptions.Multiline)]
    private static partial Regex _SectionMatcher();

    [GeneratedRegex(@"^\*\ (.+) \*(.+)\*$")]
    private static partial Regex _XMLAttributeMatcher();

    [GeneratedRegex(@"\*(.+)\* _(.+)_")]
    private static partial Regex _XMLAttributeNameMatcher();

    [GeneratedRegex(@"\* _attribute:_ (.+)$")]
    private static partial Regex _XMLAttributeTypeMatcher();

    [GeneratedRegex(@"h5. (.+)$")]
    private static partial Regex _XMLElementNameMatcher();

    [GeneratedRegex(@"^\*\ \[(.+): (.+)\|#(.+)\]$")]
    private static partial Regex _XMLLineTypeMatcher();

    [GeneratedRegex(@"\* ScriptArguments: (.+)$")]
    private static partial Regex _XMLScriptArgumentMatcher();

    public Regex ApiVersionMatcher() => _ApiVersionMatcher();
    public Regex ArgumentMatcher() => _ArgumentMatcher();
    public Regex ConstantMatcher() => _ConstantMatcher();
    public Regex DoKeywordMatcher() => _DoKeywordMatcher();
    public Regex EndKeywordMatcher() => _EndKeywordMatcher();
    public Regex EnumMatcher() => _EnumMatcher();
    public Regex EnumNameMatcher() => _EnumNameMatcher();
    public Regex EventMatcher() => _EventMatcher();
    public Regex FunctionAccessMatcher() => _FunctionAccessMatcher();
    public Regex FunctionKeywordMatcher() => _FunctionKeywordMatcher();
    public Regex FunctionMatcher() => _FunctionMatcher();
    public Regex FunctionNameMatcher() => _FunctionNameMatcher();
    public Regex FunctionReturnMatcher() => _FunctionReturnMatcher();
    public Regex GlobalMatcher() => _GlobalMatcher();
    public Regex IfKeywordMatcher() => _IfKeywordMatcher();
    public Regex ObjectNameMatcher() => _ObjectNameMatcher();
    public Regex ObjectTypeMatcher() => _ObjectTypeMatcher();
    public Regex SectionMatcher() => _SectionMatcher();
    public Regex XMLAttributeMatcher() => _XMLAttributeMatcher();
    public Regex XMLAttributeNameMatcher() => _XMLAttributeNameMatcher();
    public Regex XMLAttributeTypeMatcher() => _XMLAttributeTypeMatcher();
    public Regex XMLElementNameMatcher() => _XMLElementNameMatcher();
    public Regex XMLLineTypeMatcher() => _XMLLineTypeMatcher();
    public Regex XMLScriptArgumentMatcher() => _XMLScriptArgumentMatcher();
}

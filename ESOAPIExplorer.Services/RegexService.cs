using System.Text.RegularExpressions;

namespace ESOAPIExplorer.Services;

public partial class RegexService : IRegexService
{
    [GeneratedRegex(@"function\s+((ZO_|zo_)\w+)\((.*)\)")]
    private static partial Regex _FunctionMatcher();

    [GeneratedRegex(@"^\* (.+)\((.*)\)$")]
    private static partial Regex _FunctionNameMatcher();

    [GeneratedRegex(@"([^\s]+)( \*(.+)\*)?")]
    private static partial Regex _FunctionAccessMatcher();

    [GeneratedRegex(@"\*\*\ _Returns:_ (.+)$")]
    private static partial Regex _FunctionReturnMatcher();

    [GeneratedRegex(@"(\d+)$")]
    private static partial Regex _ApiVersionMatcher();

    [GeneratedRegex(@"h5\. (.+)$")]
    private static partial Regex _EnumNameMatcher();

    [GeneratedRegex(@"\* (.+)$")]
    private static partial Regex _EnumMatcher();

    [GeneratedRegex(@"\*(.+)\* _(.+)_")]
    private static partial Regex _ArgumentMatcher();

    [GeneratedRegex(@"^\*\ ([^\s]+)(\s\((.+)\))?")]
    private static partial Regex _EventMatcher();

    [GeneratedRegex(@"^\*\ (.+) \*(.+)\*$")]
    private static partial Regex _XMLAttributeMatcher();

    [GeneratedRegex(@"h5. (.+)$")]
    private static partial Regex _XMLElementNameMatcher();

    [GeneratedRegex(@"\* _attribute:_ (.+)$")]
    private static partial Regex _XMLAttributeTypeMatcher();

    [GeneratedRegex(@"\*(.+)\* _(.+)_")]
    private static partial Regex _XMLAttributeNameMatcher();

    [GeneratedRegex(@"\* ScriptArguments: (.+)$")]
    private static partial Regex _XMLScriptArgumentMatcher();

    [GeneratedRegex(@"^\*\ \[(.+): (.+)\|#(.+)\]$")]
    private static partial Regex _XMLLineTypeMatcher();

    [GeneratedRegex(@"\bfunction\b")]
    private static partial Regex _FunctionKeywordMatcher();

    [GeneratedRegex(@"\bif\b")]
    private static partial Regex _IfKeywordMatcher();

    [GeneratedRegex(@"\bdo\b")]
    private static partial Regex _DoKeywordMatcher();

    [GeneratedRegex(@"\bend\b")]
    private static partial Regex _EndKeywordMatcher();

    public Regex FunctionMatcher() => _FunctionMatcher();
    public Regex FunctionNameMatcher() => _FunctionNameMatcher();
    public Regex FunctionAccessMatcher() => _FunctionAccessMatcher();
    public Regex FunctionReturnMatcher() => _FunctionReturnMatcher();
    public Regex ApiVersionMatcher() => _ApiVersionMatcher();
    public Regex EnumNameMatcher() => _EnumNameMatcher();
    public Regex EnumMatcher() => _EnumMatcher();
    public Regex ArgumentMatcher() => _ArgumentMatcher();
    public Regex EventMatcher() => _EventMatcher();
    public Regex XMLAttributeMatcher() => _XMLAttributeMatcher();
    public Regex XMLElementNameMatcher() => _XMLElementNameMatcher();
    public Regex XMLAttributeTypeMatcher() => _XMLAttributeTypeMatcher();
    public Regex XMLAttributeNameMatcher() => _XMLAttributeNameMatcher();
    public Regex XMLScriptArgumentMatcher() => _XMLScriptArgumentMatcher();
    public Regex FunctionKeywordMatcher() => _FunctionKeywordMatcher();
    public Regex IfKeywordMatcher() => _IfKeywordMatcher();
    public Regex DoKeywordMatcher() => _DoKeywordMatcher();
    public Regex EndKeywordMatcher() => _EndKeywordMatcher();
    public Regex XMLLineTypeMatcher() => _XMLLineTypeMatcher();
}

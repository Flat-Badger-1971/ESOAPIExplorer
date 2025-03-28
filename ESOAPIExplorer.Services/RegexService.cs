using System.Text.RegularExpressions;

namespace ESOAPIExplorer.Services;

public partial class RegexService : IRegexService
{
    [GeneratedRegex(@"^((?![^=]*\.)[A-Zz]\S+)\s*=\s*(?![^=]*\d)(.*)")]
    private static partial Regex _AliasMatcher();

    [GeneratedRegex(@"^\*\s(.+)\((.*)\)")]
    private static partial Regex _ApiParamSplitMatcher();

    [GeneratedRegex(@"(\d+)$")]
    private static partial Regex _ApiVersionMatcher();

    [GeneratedRegex(@"\*(.+)\* _(.+)_")]
    private static partial Regex _ArgumentMatcher();

    [GeneratedRegex(@"^(is|can|should)[A-Z]+")]
    private static partial Regex _BooleanMatcher();

    [GeneratedRegex(@"^\s*([A-Z_]+)\s+=\s+ZO_CallbackObject.New\(self\)")]
    private static partial Regex _CallbackObjectMatcher();

    [GeneratedRegex(@"^(ZO_.+)\s=\sZO_ColorDef:New\(")]
    private static partial Regex _ColorDefMatcher();

    [GeneratedRegex(@"\[""(.*?)""\]\s*=\s*(.*?),")]
    private static partial Regex _ConstantMatcher();

    [GeneratedRegex(@"\bdo\b")]
    private static partial Regex _DoKeywordMatcher();

    [GeneratedRegex(@"\bend\b[^\)]")]
    private static partial Regex _EndKeywordMatcher();

    [GeneratedRegex(@"\* (.+)$")]
    private static partial Regex _EnumMatcher();

    [GeneratedRegex(@"h5\. (.+)$")]
    private static partial Regex _EnumNameMatcher();

    [GeneratedRegex(@"^\s+\""(.+)\"",\s\-\-\s(.+)$")]
    private static partial Regex _EsoStringMatcher();

    [GeneratedRegex(@"^\*\ ([^\s]+)(\s\((.+)\))?")]
    private static partial Regex _EventMatcher();

    [GeneratedRegex(@"\s*((?:[A-Z_]*FRAGMENT))\s=\s")]
    private static partial Regex _FragmentMatcher();

    [GeneratedRegex(@"([^\s]+)( \*(.+)\*)?")]
    private static partial Regex _FunctionAccessMatcher();

    [GeneratedRegex(@"^\bfunction\b")]
    private static partial Regex _FunctionKeywordMatcher();

    [GeneratedRegex(@"^function\s+(\w+)\((.*)\)")]
    private static partial Regex _FunctionMatcher();

    [GeneratedRegex(@"^\* (.+)\((.*)\)$")]
    private static partial Regex _FunctionNameMatcher();

    [GeneratedRegex(@"\*\*\ _Returns:_ (.+)$")]
    private static partial Regex _FunctionReturnMatcher();

    [GeneratedRegex(@"^((ZO_|zo_)\w+)\s=\s(.+)")]
    private static partial Regex _GlobalMatcher();

    [GeneratedRegex(@"\bif\b")]
    private static partial Regex _IfKeywordMatcher();

    [GeneratedRegex(@"\s*([A-Z_]+)\s=\s(.+)[:\.]+(New|Initialize)\(.*\)")]
    private static partial Regex _InstanceMatcher();

    [GeneratedRegex(@"^SafeAddString\(([^,]+),\s""([^""]+)""")]
    private static partial Regex _LocaleStringMatcher();

    [GeneratedRegex(@"^\d+$|.*[iI][dD]$|^num.+")]
    private static partial Regex _NumberMatcher();

    [GeneratedRegex(@"h3\. (.+)$")]
    private static partial Regex _ObjectNameMatcher();

    [GeneratedRegex(@"^function (.+):(.+)\((.*)\)")]
    private static partial Regex _ObjectTypeMatcher();

    [GeneratedRegex(@"\s\*(.+)\*\s")]
    private static partial Regex _ScopeMatcher();

    [GeneratedRegex(@"\[""(Constants|SI_String_Constants)""\]\s*=\s*{\s*((?:.*?,\s*)+)\s*}", RegexOptions.Multiline)]
    private static partial Regex _SectionMatcher();

    [GeneratedRegex(@"^\s*([A-Z_]+)\s+=\s+self")]
    private static partial Regex _SelfAssignmentMatcher();

    [GeneratedRegex(@"^\s*(?:local\s+)?(.+)\s+=\s+(.+):Subclass\(")]
    private static partial Regex _SubclassMatcher();

    [GeneratedRegex(@"^[A-Z_]+$")]
    private static partial Regex _UppercaseMatcher();

    public Regex AliasMatcher() => _AliasMatcher();
    public Regex ApiParamSplitMatcher() => _ApiParamSplitMatcher();
    public Regex ApiVersionMatcher() => _ApiVersionMatcher();
    public Regex ArgumentMatcher() => _ArgumentMatcher();
    public Regex BooleanMatcher() => _BooleanMatcher();
    public Regex CallbackObjectMatcher() => _CallbackObjectMatcher();
    public Regex ColorDefMatcher() => _ColorDefMatcher();
    public Regex ConstantMatcher() => _ConstantMatcher();
    public Regex DoKeywordMatcher() => _DoKeywordMatcher();
    public Regex EndKeywordMatcher() => _EndKeywordMatcher();
    public Regex EnumMatcher() => _EnumMatcher();
    public Regex EnumNameMatcher() => _EnumNameMatcher();
    public Regex EsoStringMatcher() => _EsoStringMatcher();
    public Regex EventMatcher() => _EventMatcher();
    public Regex FragmentMatcher() => _FragmentMatcher();
    public Regex FunctionAccessMatcher() => _FunctionAccessMatcher();
    public Regex FunctionKeywordMatcher() => _FunctionKeywordMatcher();
    public Regex FunctionMatcher() => _FunctionMatcher();
    public Regex FunctionNameMatcher() => _FunctionNameMatcher();
    public Regex FunctionReturnMatcher() => _FunctionReturnMatcher();
    public Regex GlobalMatcher() => _GlobalMatcher();
    public Regex IfKeywordMatcher() => _IfKeywordMatcher();
    public Regex InstanceMatcher() => _InstanceMatcher();
    public Regex LocaleStringMatcher() => _LocaleStringMatcher();
    public Regex NumberMatcher() => _NumberMatcher();
    public Regex ObjectNameMatcher() => _ObjectNameMatcher();
    public Regex ObjectTypeMatcher() => _ObjectTypeMatcher();
    public Regex ScopeMatcher() => _ScopeMatcher();
    public Regex SectionMatcher() => _SectionMatcher();
    public Regex SelfAssignmentMatcher() => _SelfAssignmentMatcher();
    public Regex SubclassMatcher() => _SubclassMatcher();
    public Regex UppercaseMatcher() => _UppercaseMatcher();
}

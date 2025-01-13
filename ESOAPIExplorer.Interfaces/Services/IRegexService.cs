using System.Text.RegularExpressions;

namespace ESOAPIExplorer.Services
{
    public interface IRegexService
    {
        public Regex AliasMatcher();
        public Regex ApiVersionMatcher();
        public Regex ArgumentMatcher();
        public Regex CallbackObjectMatcher();
        public Regex ColorDefMatcher();
        public Regex ConstantMatcher();
        public Regex DoKeywordMatcher();
        public Regex EndKeywordMatcher();
        public Regex EnumMatcher();
        public Regex EnumNameMatcher();
        public Regex EsoStringMatcher();
        public Regex EventMatcher();
        public Regex FragmentMatcher();
        public Regex FunctionAccessMatcher();
        public Regex FunctionKeywordMatcher();
        public Regex FunctionMatcher();
        public Regex FunctionNameMatcher();
        public Regex FunctionReturnMatcher();
        public Regex GlobalMatcher();
        public Regex IfKeywordMatcher();
        public Regex InstanceMatcher();
        public Regex LocaleStringMatcher();
        public Regex ObjectNameMatcher();
        public Regex ObjectTypeMatcher();
        public Regex SectionMatcher();
        public Regex SelfAssignmentMatcher();
        public Regex SubclassMatcher();
    }
}

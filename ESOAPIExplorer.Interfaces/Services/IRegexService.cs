using System.Text.RegularExpressions;

namespace ESOAPIExplorer.Services
{
    public interface IRegexService
    {
        public Regex FunctionMatcher();
        public Regex FunctionNameMatcher();
        public Regex FunctionAccessMatcher();
        public Regex FunctionReturnMatcher();
        public Regex ApiVersionMatcher();
        public Regex EnumNameMatcher();
        public Regex EnumMatcher();
        public Regex ArgumentMatcher();
        public Regex EventMatcher();
        public Regex XMLAttributeMatcher();
        public Regex XMLElementNameMatcher();
        public Regex XMLAttributeTypeMatcher();
        public Regex XMLAttributeNameMatcher();
        public Regex XMLScriptArgumentMatcher();
        public Regex XMLLineTypeMatcher();
        public Regex FunctionKeywordMatcher();
        public Regex IfKeywordMatcher();
        public Regex DoKeywordMatcher();
        public Regex EndKeywordMatcher();
    }
}

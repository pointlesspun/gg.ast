/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.common;
using gg.ast.core;
using gg.ast.core.rules;

using static gg.ast.common.ShortHandRules;

namespace gg.ast.interpreter
{
    public class InterpreterConfig
    {
        public class ConfigTags
        {
            public string Main { get; set; } = "main";

            public string Interpreter { get; set; } = "intepreter";


            public string Identifier { get; set; } = "identifier";

            public string RuleList { get; set; } = "ruleList";
            
            public string Rule { get; set; } = "rule";

            public string RuleVisibility { get; set; } = "rule visibility";

            public string RuleTag { get; set; } = "identifier";
            
            public string RuleValue { get; set; } = "ruleValue";

            public string UnaryValue { get; set; } = "unaryValue";

            public string RuleTerminator { get; set; } = "ruleTerminator";

            public string RuleReference { get; set; } = "ruleReference";
            
            public string Literal { get; set; } = "literal";

            public string RuleGroup { get; set; } = "group";

            public string Sequence { get; set; } = "sequence";

            public string SequenceWithoutSeparator { get; set; } = "sequence without separator";

            public string SequenceSeparator { get; set; } = "sequence separator";
            
            public string Or { get; set; } = "or-rule";
            
            public string OrSeparator { get; set; } = "or-rule separator";
            
            public string Repeat { get; set; } = "repeat";

            public string RepeatNoWhitespace { get; set; } = "repeat no whitespace";

            public string RepeatExact { get; set; } = "repeat exactly N";
            
            public string RepeatZeroOrMore { get; set; } = "repeat zero or more";

            public string RepeatZeroOrOne { get; set; } = "zero or one";


            public string RepeatOneOrMore = "repeat one or more";
            
            public string RepeatNOrMore { get; set; } = "repeat N or more";
            
            public string RepeatNoMoreThanN { get; set; } = "repeat no more than N";
            
            public string RepeatBetweenNandM { get; set; } = "repeat between N and M";

            public string Group { get; set; } = "group";

            public string BeginGroup { get; set; } = "begin group";

            public string EndGroup { get; set; } = "end group";

            public string Unnamed { get; set; } = "(unnamed)";

            public string RuleSeparator { get; set; } = "rule separator";

            public string MatchAnyCharacter { get; set; } = "match any character";

            public string MatchCharactersInRange { get; set; } = "match characters in range";

            public string MatchCharactersInEnumeration { get; set; } = "match characters in enumeration";

            public string Not { get; set; } = "not";

            public string NotAndSkip { get; set; } = "not&skip";

            public string Whitespace { get; set; } = "whitespace";

            public string WhitespaceShortHand { get; set; } = "ws";

            public string UseList { get; set; } = "usingStatements";

            public string UseFile { get; set; } = "use file";
        }

        public class ConfigTokens
        {
            public string Whitespace { get; set; } = " \r\n\t";
            
            public string OrValueListSepatator { get; set; } = "|";
                        
            public string BeginGroup { get; set; } = "(";

            public string EndGroup { get; set; } = ")";

            public string BeginRepeat { get; set; } = "[";

            public string EndRepeat { get; set; } = "]";

            public string BeginRepeatNoWhitespace { get; set; } = "<";

            public string EndRepeatNoWhitespace { get; set; } = ">";

            public string SequenceValueListSepatator { get; set; } = ",";
            
            public string RuleSeparator { get; set; } = "=";

            public string RepeatZeroOrOne { get; set; } = "?";

            public string RepeatZeroOrMore { get; set; } = "*";

            public string RepeatOneOrMore { get; set; } = "+";

            public string RuleTerminator { get; set; } = ";";

            public string RuleVisibility { get; set; } = "#";

            public string MatchAnyCharacter { get; set; } = "$";

            public string MatchCharactersInRangeDelimiter { get; set; } = "`";

            public string MatchCharactersInEnumerationDelimiter { get; set; } = "'";

            public string Not { get; set; } = "!";

            public string NotAndSkip { get; set; } = "~";

            public string Use { get; set; } = "using";
        }

        public ConfigTags Tags { get; private set; } = new ConfigTags();

        public ConfigTokens Tokens { get; private set; } = new ConfigTokens();

        /// <summary>
        /// Whitespace used by the interpreter rules
        /// </summary>
        public IRule WhiteSpace { get; set; } = CreateWhiteSpaceRule();

        /// <summary>
        /// Whitespace used by the value map to assign to generated rules
        /// </summary>
        public IRule ValueMapWhiteSpace { get; set; } = CreateWhiteSpaceRule();
        
        /// <summary>
        /// If set to true unnamed rules inside groups and metarules will be hidden
        /// </summary>
        public bool HideUnnamedRules { get; set; } = true;

        public static IRule CreateWhiteSpaceRule(CommentsConfig config = null, NodeVisiblity visibility = NodeVisiblity.Hidden)
        {
            var preprocessorConfig = config ?? new CommentsConfig();

            var whitespaceSelection = new OrRule()
            {
                Visibility = NodeVisiblity.Hidden,
                Subrules = new IRule[]
                {
                    CommentsRules.CreateMultilineCommentRule(preprocessorConfig, visibility: visibility),
                    CommentsRules.CreateSinglelineCommentRule(preprocessorConfig, visibility: visibility),
                    CreateWhitespaceRule(visibility: visibility)
                }
            };

            return new RepeatRule()
            {
                Visibility = NodeVisiblity.Hidden,
                Min = 0,
                Max = -1,
                Subrule = whitespaceSelection
            };
        }
    }
}

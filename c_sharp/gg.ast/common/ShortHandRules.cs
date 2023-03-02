/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.core;
using gg.ast.core.rules;

namespace gg.ast.common
{
    /// <summary>
    /// Common short hand rules, ie rules which are preconfigured for common use cases.
    /// </summary>
    public static class ShortHandRules
    {
        /// <summary>
        /// Create a standard or customizable whitespace rule
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="characters"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public static IRule CreateWhitespaceRule(
            string tag = "whitespace",
            string characters = " \n\t\r", 
            RuleVisiblity visibility = RuleVisiblity.Hidden)
        {
            return new CharRule()
            {
                Tag = tag,
                Characters = characters,
                Visibility = visibility,
                MatchCharacters = CharRule.MatchType.InEnumeration,
                Min = 1,
                Max = -1
            };
        }

        public static IRule Optional(
            IRule other, 
            IRule whitespace = null, 
            RuleVisiblity visibility = RuleVisiblity.Transitive,
            string tag = null)
        {
            return new RepeatRule()
            {
                Tag = tag ?? "optional " + other.Tag,
                Subrule = other,
                Visibility = visibility,
                Min = 0,
                Max = 1,
                WhiteSpaceRule = whitespace
            };
        }

        public static IRule ZeroOrMore(
            IRule other, 
            IRule whitespace = null,
            RuleVisiblity visibility = RuleVisiblity.Transitive,
            string tag = null)
        {
            return new RepeatRule()
            {
                Tag = tag ?? "zero or more " + other.Tag,
                Subrule = other,
                Visibility = visibility,
                Min = 0,
                Max = -1,
                WhiteSpaceRule = whitespace
            };
        }

        public static IRule OneOrMore(
            IRule other,
            IRule whitespace = null,
            RuleVisiblity visibility = RuleVisiblity.Transitive,
            string tag = null)
        {
            return new RepeatRule()
            {
                Tag = tag ?? "one or more " + other.Tag,
                Subrule = other,
                Visibility = visibility,
                Min = 1,
                Max = -1,
                WhiteSpaceRule = whitespace
            };
        }

        public static IRule SelectFrom(
            params IRule[] others)
        {
            return new OrRule()
            {
                Tag = "select from",
                Subrules = others,
                Visibility = RuleVisiblity.Transitive,
                WhiteSpaceRule = null
            };
        }
    }
}

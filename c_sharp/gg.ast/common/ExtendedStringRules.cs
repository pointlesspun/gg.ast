/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.core;
using gg.ast.core.rules;

namespace gg.ast.common
{
    public static class ExtendedStringRules
    {
        public static IRule CreateStringRule(
            string tag = null,
            string delimiters = "\"",
            RuleVisiblity visibility = RuleVisiblity.Visible) =>
            TypeRules.CreateStringRule(
                CreateEscapeRule(),
                escapeCharacterEnumeration: "\\",
                delimiters: delimiters,
                visibility: visibility,
                tag: tag ?? TypeRules.Tags.String
            );

        public static IRule CreateEscapeRule(RuleVisiblity visibility = RuleVisiblity.Hidden, IRule wsRule = null)
        {
            var whitespaceRule = wsRule ?? ShortHandRules.CreateWhitespaceRule();

            var unicodeOrEscape = new OrRule()
            {
                Visibility = RuleVisiblity.Hidden,
                Tag = TypeRules.Tags.EscapeCharacter,
                Subrules = new IRule[]
                {
                    // unicode
                    TypeRules.CreateEscapeRule(
                        TypeRules.CreateHexCharactersRule(min: 4, max: 4),
                        letterEnumeration: "u",
                        escapeCharacters: null,
                        abortOnCriticalFailue: false
                    ),
                    new CharRule()
                    {
                        Max = 1,
                        Min = 1,
                        MatchCharacters = CharRule.MatchType.InEnumeration,
                        Characters = "nrtve\"\\"
                    }
                }
            };

            return new SequenceRule()
            {
                Tag = TypeRules.Tags.EscapeSequence,
                Visibility = visibility,
                WhiteSpaceRule = whitespaceRule,
                Subrules = new IRule[]
                {
                    new CharRule()
                    {
                        Visibility = RuleVisiblity.Hidden,
                        Characters = "\\",
                        Min = 1,
                        Max = 1,
                        MatchCharacters = CharRule.MatchType.InEnumeration
                    },
                    new CriticalRule()
                    {
                        Tag = unicodeOrEscape.Tag,
                        Subrule = unicodeOrEscape
                    }
                }
            };
        }
    }
}

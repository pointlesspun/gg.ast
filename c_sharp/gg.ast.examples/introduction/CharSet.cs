/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

using gg.ast.core;
using gg.ast.core.rules;
using gg.ast.interpreter;

using static gg.ast.util.FileCache;

namespace gg.ast.examples.introduction
{
    public class CharSet
    {
        public static IRule AnyCharacter()
        {
            return new CharRule()
            {
                Tag = "anyCharacter",
                MatchCharacters = CharRule.MatchType.Any
            };
        }

        public static IRule AToZSet()
        {
            return new CharRule()
            {
                Tag = "aToZSet",
                MatchCharacters = CharRule.MatchType.InRange,
                Characters = "az"
            };
        }

        public static IRule WideSet()
        {
            return new CharRule()
            {
                Tag = "wideSet",
                MatchCharacters = CharRule.MatchType.InRange,
                Characters = "azAZ09"
            };
        }

        public static IRule AbcEnumeration()
        {
            return new CharRule()
            {
                Tag = "wideSet",
                MatchCharacters = CharRule.MatchType.InEnumeration,
                Characters = "abc"
            };
        }

        public static IRule NotAbcEnumeration()
        {
            return new CharRule()
            {
                Tag = "wideSet",
                MatchCharacters = CharRule.MatchType.NotInEnumeration,
                Characters = "abc"
            };
        }

        public static Dictionary<string, IRule> CharSetSpecFile(string specFile = "introduction/charsets.spec")
        {
            var rules = LoadTextFile(specFile);
            return new ParserFactory().ParseRules(rules);
        }
    }
}

/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

using gg.ast.core;
using gg.ast.core.rules;
using gg.ast.interpreter;

using static gg.ast.util.FileCache;

namespace gg.ast.examples.introduction
{
    public class Sequences
    {
        public static IRule HelloWorldSequence()
        {
            return new SequenceRule()
            {
                Tag = "helloWorld",
                WhiteSpaceRule = null,
                Subrules = new IRule[]
                {
                    new LiteralRule()
                    {
                        Tag = "hello",
                        Characters = "hello"
                    },
                    new LiteralRule()
                    {
                        Tag = "world",
                        Characters = "world"
                    },
                }
            };
        }

        public static IRule HelloSpaciousWorldSequence()
        {
            return new SequenceRule()
            {
                Tag = "helloSpaciousWorld",
                WhiteSpaceRule = new CharRule()
                {
                    Characters = " \t\r\n",
                    MatchCharacters = CharRule.MatchType.InEnumeration
                },
                Subrules = new IRule[]
                {
                    new LiteralRule()
                    {
                        Tag = "hello",
                        Characters = "hello"
                    },
                    new LiteralRule()
                    {
                        Tag = "world",
                        Characters = "world"
                    },
                }
            };
        }

        public static Dictionary<string, IRule> SequencesSpecFile(string specFile = "introduction/sequences.spec")
        {
            var rules = LoadTextFile(specFile);
            return new ParserFactory().ParseRules(rules);
        }
    }
}


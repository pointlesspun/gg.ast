/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

using gg.ast.core;
using gg.ast.core.rules;
using gg.ast.interpreter;

namespace gg.ast.examples.introduction
{
    public class NotExample
    {
        public static IRule NotFoo(int skip = 0)
        {
            var foo = new LiteralRule()
            {
                Characters = "foo"
            };

            var notFoo = new NotRule()
            {
                Subrule = foo,
                Skip = skip
            };

            return new RepeatRule()
            {
                Subrule = notFoo,
                Min = 0,
                Max = -1
            };
        }       


        public static IRule CreateCommentRule()
        {
            var commentStart = new LiteralRule()
            {
                Characters = "/*"
            };

            var commentEnd = new LiteralRule()
            {
                Characters = "*/"
            };

            var notEndSequence = new SequenceRule()
            {
                Subrules = new IRule[]
                {
                    new NotRule()
                    {
                        Subrule = commentEnd
                    },
                    new CharRule()
                    {
                        MatchCharacters = CharRule.MatchType.Any,
                        Min = 1,
                        Max = 1
                    }
                }
            };

            var notEndOfComment = new RepeatRule()
            {
                Subrule = notEndSequence,
                Min = 0,
                Max = -1
            };

            return new SequenceRule()
            {
                Tag = "comment",
                Subrules = new IRule[]
                {
                    commentStart,
                    notEndOfComment,
                    commentEnd
                }
            };
        }

        public static Dictionary<string, IRule> LoadSpecFileRules(string specFile = "introduction/not.spec")
        {
            return new ParserFactory().ParseFileRules(specFile);
        }
    }
}


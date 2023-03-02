/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.core;
using gg.ast.core.rules;

namespace gg.ast.common
{
    public static class CommentsRules
    {
        public static IRule CreateSinglelineCommentRule(CommentsConfig config, RuleVisiblity visibility = RuleVisiblity.Hidden)
        {
            return new SequenceRule()
            {
                Tag = config.Tags.SingleLineComment,
                Visibility = visibility,
                Subrules = new IRule[]
                {
                    new LiteralRule()
                    {
                        Tag = config.Tags.SingleLineCommentBegin,
                        Characters = config.Tokens.SingleLineCommentBegin
                    },
                    new CharRule()
                    {
                        Tag = config.Tags.SingleLineCommentCharacters,
                        Characters = config.Tokens.SingleLineCommentEnd,
                        MatchCharacters = CharRule.MatchType.NotInEnumeration
                    },
                    new RepeatRule()
                    {
                        Tag = "optional",
                        Subrule = new LiteralRule()
                        {
                            Tag = "eoln",
                            Characters = "\n"
                        },
                        Visibility = RuleVisiblity.Hidden,
                        Min = -1,
                        Max = 1
                    }
                }
            };
        }

        public static IRule CreateMultilineCommentRule(CommentsConfig config, RuleVisiblity visibility = RuleVisiblity.Hidden)
        {
            var commentStart = new LiteralRule()
            {
                Tag = config.Tags.MultiLineCommentBegin,
                Characters = config.Tokens.MultiLineCommentBegin
            };

            var commentEnd = new LiteralRule()
            {
                Tag = config.Tags.MultiLineCommentEnd,
                Characters = config.Tokens.MultiLineCommentEnd
            };

            var notEndOfComment = new RepeatRule()
            {
                Subrule = new NotRule()
                {
                    Subrule = commentEnd,
                    Skip = 1
                },
                Visibility = RuleVisiblity.Hidden,
                Min = 0,
                Max = -1
            };

            return new SequenceRule()
            {
                Tag = config.Tags.MultiLineComment,
                Visibility = visibility,
                Subrules = new IRule[]
                {
                    commentStart,
                    notEndOfComment,
                    commentEnd
                }
            };
        }

        public static IRule CreateDocumentCharactersRule(CommentsConfig config, RuleVisiblity visibility = RuleVisiblity.Hidden)
        {
            var singleLineCommentBegin = new LiteralRule()
            {
                Tag = config.Tags.SingleLineCommentBegin,
                Characters = config.Tokens.SingleLineCommentBegin
            };
            var multiLineCommentBegin = new LiteralRule()
            {
                Tag = config.Tags.MultiLineCommentBegin,
                Characters = config.Tokens.MultiLineCommentBegin
            };

            var documentCharacters = new CharRule()
            {
                Tag = config.Tags.DocumentCharacters,
                Characters = config.Tokens.CommentStart,
                MatchCharacters = CharRule.MatchType.NotInEnumeration
            };

            var singleOrMultilineComment = new OrRule()
            {
                Tag = config.Tags.NotComment,
                Visibility = RuleVisiblity.Hidden,
                Subrules = new IRule[] {
                    singleLineCommentBegin,
                    multiLineCommentBegin
                }
            };

            var notComment = new NotRule()
            {
                Tag = config.Tags.NotComment,
                Skip = 1
            }
            .Bind(singleOrMultilineComment);

            // a document character is either
            //
            // - a series of character until '/'
            // - or skip an individual '/' character if it is not // or /*
            return new OrRule()
            {
                Tag = config.Tags.DocumentCharacters,
                Visibility = visibility,
                Subrules = new IRule[] {
                    documentCharacters,
                    notComment
                }
            };
        }

        public static IRule CreateDocumentTextRule(CommentsConfig config, RuleVisiblity visibility = RuleVisiblity.Visible)
        {
            var singleLineCommentBegin = new LiteralRule()
            {
                Tag = config.Tags.SingleLineCommentBegin,
                Characters = config.Tokens.SingleLineCommentBegin
            };
            var multiLineCommentBegin = new LiteralRule()
            {
                Tag = config.Tags.MultiLineCommentBegin,
                Characters = config.Tokens.MultiLineCommentBegin
            };

            var isComment = new OrRule()
            {
                Tag = config.Tags.CommentStart,
                Visibility = RuleVisiblity.Hidden,
                Subrules = new IRule[] {
                    singleLineCommentBegin,
                    multiLineCommentBegin
                }
            };

            return new RepeatRule()
            {
                Subrule = new NotRule()
                {
                    Subrule = isComment,
                    Skip = 1
                },
                Visibility = visibility,
                Min = 0,
                Max = -1
            };
        }
    }
}

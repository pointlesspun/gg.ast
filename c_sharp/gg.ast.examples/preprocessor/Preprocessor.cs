/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Text;

using gg.ast.common;
using gg.ast.core;
using gg.ast.core.rules;

using static gg.ast.common.CommentsRules;

namespace gg.ast.examples.preprocessor
{
    /// <summary>
    /// Some example functions around the concept of a pre-processor
    /// </summary>
    public static class Preprocessor
    {
        public static IRule CreatePreprocessorRule(CommentsConfig config, RuleVisiblity visibility = RuleVisiblity.Visible)
        {
            var documentParts = new OrRule()
            {
                Tag = config.Tags.DocumentParts,
                Visibility = RuleVisiblity.Transitive,
                Subrules = new IRule[]
                {
                    CreateMultilineCommentRule(config, visibility),
                    CreateSinglelineCommentRule(config, visibility),
                    CreateDocumentTextRule(config, visibility)
                }
            };

            return new RepeatRule()
            {
                Tag = config.Tags.Document,
                Subrule = documentParts,
                Visibility = visibility,
                Min = -1,
                Max = -1
            };
        }

        public static IRule CreatePreprocessor(CommentsConfig config = null)
        {
            return CreatePreprocessorRule(config ?? new CommentsConfig());
        }

        public static string RemoveComments(string input, CommentsConfig config = null)
        {
            var builder = new StringBuilder();
            var preprocessorConfig = config ?? new CommentsConfig();
            var result = CreatePreprocessor(preprocessorConfig).Parse(input);

            if (result.IsSuccess && result.Nodes != null)
            {
                foreach (var node in result.Nodes)
                {
                    if (node.Children != null)
                    {
                        RemoveCommentsFromNode(node, input, builder, preprocessorConfig);
                    }
                }
            }

            return builder.ToString();
        }

        private static void RemoveCommentsFromNode(
            AstNode node,
            string input,
            StringBuilder builder,
            CommentsConfig config)
        {
            foreach (var child in node.Children)
            {
                if (child.Tag == config.Tags.DocumentText)
                {
                    builder.Append(input.AsSpan(child.StartIndex, child.Length));
                }
            }
        }
    }
}

/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
using System.Text;

using gg.ast.util;

namespace gg.ast.core
{
    /// <summary>
    /// Various extension methods
    /// </summary>
    public static class AstExtensions
    {
        /// <summary>
        /// Map the parse result to a value 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result">The result of a parse operation </param>
        /// <param name="text">The text used in parsing</param>
        /// <param name="map">A map defining a map from (tag, text) onto a value</param>
        /// <returns></returns>
        public static T Map<T>(this ParseResult result, string text, ValueMap map)
        {
            return map.Map<T>(text, result.Nodes[0]);
        }        

        /// <summary>
        /// Create a list of nodes from the current node up to the root of the tree
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<AstNode> GetPathToRoot(this AstNode node)
        {
            var result = new List<AstNode>();

            if (node != null)
            {
                var current = node;

                while (current.Parent != null)
                {
                    result.Insert(0, current);
                    current = current.Parent;
                }

                result.Insert(0, current);
            }

            return result;
        }

        /// <summary>
        /// Prints the tree to stdout
        /// </summary>
        /// <param name="node"></param>
        /// <param name="output"></param>
        /// <param name="text"></param>
        /// <param name="indent"></param>
        /// <param name="indentIncrement"></param>
        /// <param name="maxLength"></param>
        /// <param name="shortTextLength"></param>
        public static void PrintTree(
            this AstNode node,
            Action<string> output,
            string text,
            int indent = 0,
            int indentIncrement = 2,
            int maxLength = 50,
            int shortTextLength = 6
        )
        {
            output("".PadLeft(indent));
            output($"{node.Rule.Tag}: ({node.StartIndex}...{node.StartIndex + node.Length})");

            int availableCharacters = maxLength - indent;

            if (node.Length > availableCharacters)
            {
                output($"'{text.SubStringNoLineBreaks(node.StartIndex, shortTextLength, "\\n")}...{text.SubStringNoLineBreaks(node.StartIndex + node.Length - shortTextLength, shortTextLength, "\\n")}'");
            }
            else
            {
                output($"'{text.SubStringNoLineBreaks(node.StartIndex, node.Length, "\\n")}'");
            }

            output("\n");

            if (node.Children != null && node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    child.PrintTree(output, text, indent + indentIncrement, indentIncrement, maxLength, shortTextLength);
                }
            }
        }        

        /// <summary>
        /// Short hand to get the rule's tag
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string GetTag(this AstNode node) => node.Rule.Tag;

        /// <summary>
        /// Go through a rule graph and collect all unique rules and subrules
        /// </summary>
        /// <param name="rule">The start of the graph</param>
        /// <param name="collectedRules">Optional result container, will be created if none is provided</param>
        /// <returns>A HashSet of all rules in this rule graph</returns>
        public static HashSet<IRule> CollectRules(this IRule rule, HashSet<IRule> collectedRules = null)
        {
            collectedRules ??= new HashSet<IRule>();

            if (!collectedRules.Contains(rule))
            {
                collectedRules.Add(rule);

                if (rule is IMetaRule metaRule && metaRule.Subrule != null)
                {
                    CollectRules(metaRule.Subrule, collectedRules);
                }
                else if (rule is IRuleGroup group && group.Subrules != null)
                {
                    group.Subrules.ForEachIndexed((subrule, _) => CollectRules(subrule, collectedRules));    
                }
            }

            return collectedRules;
        }

        public static string PrintRuleTree(
            this IRule rule, 
            int indent = 0, 
            int indentIncrement = 4, 
            StringBuilder b = null, 
            HashSet<IRule> visited = null)
        {
            var visitedRules = visited ?? new HashSet<IRule>();
            var builder = b ?? new StringBuilder();
            
            visitedRules.Add(rule);
            builder.Append(GetRuleString(rule).AddPrefix(indent));

            if (rule is IRuleGroup ruleGroup)
            {
                foreach (var subrule in ruleGroup)
                {
                    if (subrule != null)
                    {
                        if (!visitedRules.Contains(subrule))
                        {
                            PrintRuleTree(subrule, indent + indentIncrement, indentIncrement, builder, visitedRules);
                        }
                        else
                        {
                            builder.Append(GetRuleString(subrule).AddPrefix(indent + indentIncrement));
                        }
                    }
                }
            }
            else if (rule is IMetaRule metaRule)
            {
                if (metaRule.Subrule != null)
                {
                    if (!visitedRules.Contains(metaRule.Subrule))
                    {
                        PrintRuleTree(metaRule.Subrule, indent + indentIncrement, indentIncrement, builder, visitedRules);
                    }
                    else
                    {
                        builder.Append(GetRuleString(metaRule.Subrule).AddPrefix(indent + indentIncrement));
                    }
                }
            }

            return builder.ToString();

            static String GetRuleString(IRule rule) => rule.GetType().Name + ": " + rule.Tag + "\n";
        }
    }
}

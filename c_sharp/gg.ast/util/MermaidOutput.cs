/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;
using System.IO;
using System.Text;

using gg.ast.core;
using gg.ast.core.rules;
using gg.ast.interpreter;

namespace gg.ast.util
{
    /// <summary>
    /// Output a rule set to a mermaid diagram https://github.com/mermaid-js
    /// </summary>
    public class MermaidOutput
    {
        /// <summary>
        /// Export the nodes in DepthFirst or BreadthFirst order (which at the moment
        /// Mermaid gleefully ignores for the most part).
        /// </summary>
        public enum Order
        {
            DepthFirst,
            BreadthFirst
        }

        /// <summary>
        /// Indentation of the diagram at the first node
        /// </summary>
        public int StartIndentation { get; set; } = 0;

        /// <summary>
        /// Number of spaces the indentation increases when going down one layer in the rule tree
        /// </summary>
        public int IndentationIncrement { get; set; } = 2;

        public bool CullNotVisibleNodes { get; set; } = false;

        public bool AllowDuplicates { get; set; } = false;

        public bool ShowReferenceRules { get; set; } = false;

        /// <summary>
        /// Writes a mermaid chart using the given rule to a file.
        /// </summary>
        /// <param name="rule">The rule for which the diagram has to be build</param>
        /// <param name="file">Optional filename, if no filename is given the tag of the rule will be used</param>
        /// <param name="chartType">Mermaid chart type (defaults to flowchart top-down)</param>
        /// <param name="order">Order in which the nodes have to be written to the file depth first or breadth first</param>
        public void ToMDFile(IRule rule, string file = null, string chartType= "flowchart TD", Order order = Order.DepthFirst)
        {
            var fileName = file ?? rule.Tag + ".md";
            var text = $"```mermaid\n{chartType}\n{ToMermaidChart(rule, order)}\n```";
            File.WriteAllText(fileName, text);
        }

        public void ToChart(IRule rule, string file = null, string chartType = "flowchart TD", Order order = Order.DepthFirst)
        {
            var fileName = file ?? rule.Tag + ".md";
            File.WriteAllText(fileName, chartType + "\n" + ToMermaidChart(rule, order));
        }

        /// <summary>
        /// Transforms the given rule into a mermaid chart
        /// </summary>
        /// <param name="rule">The rule for which the diagram has to be build</param>
        /// <param name="order">Order in which the nodes have to be written to the file depth first or breadth first</param>
        /// <returns>a string containing the chart</returns>
        public string ToMermaidChart(IRule rule, Order order = Order.DepthFirst)
        {
            return order switch
            {
                Order.DepthFirst => ToStringDepthFirst(rule, indentation: StartIndentation),
                Order.BreadthFirst => ToStringBreadthFirst(rule),
                _ => "unknown order"
            };
        }

        /// <summary>
        /// Writes the rule tree to the string builder in a depth first order.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="parentId"></param>
        /// <param name="builder"></param>
        /// <param name="visitedRules"></param>
        /// <param name="indentation"></param>
        /// <returns></returns>
        private string ToStringDepthFirst(
            IRule rule, 
            string parentId = "", 
            StringBuilder builder = null, 
            Dictionary<IRule, string> visitedRules = null,  
            int indentation = 0)
        {
            builder ??= new StringBuilder();
            visitedRules ??= new Dictionary<IRule, string>();

            var includeNode = (ShowReferenceRules || !(rule is ReferenceRule)) 
                            && (!CullNotVisibleNodes || rule.Visibility == NodeVisiblity.Visible);
            var ruleIdString = parentId + "_" + rule.Id;
            
            // if this node has been added to the graph before connect the parent to the other
            // instance
            if (visitedRules.TryGetValue(rule, out string value))
            {
                builder.AppendLine($"{parentId} --> {value}".AddPrefix(indentation + IndentationIncrement));
            }
            else
            {
                if (includeNode)
                {
                    // connect the parent to this node (if any)
                    if (!string.IsNullOrEmpty(parentId))
                    {
                        builder.AppendLine($"{parentId} --> {ruleIdString}".AddPrefix(indentation));
                    }

                    // specify the node's text
                    var ruleText = $"{ruleIdString}(\"{CreateRuleText(rule)}\")".AddPrefix(indentation);
                    builder.AppendLine(ruleText);
                    visitedRules[rule] = ruleIdString;
                }

                // add subrules of a IRulegroup
                if (rule is IRuleGroup group && group.Subrules != null && group.Subrules.Length > 0)
                {   
                    for (var i = 0; i < group.Subrules.Length; i++ )
                    {
                        var subrule = group.Subrules[i];
                        var subruleVisitedNodes = AllowDuplicates ? new Dictionary<IRule, string>(visitedRules) : visitedRules;
                        var subruleParent = includeNode ? ruleIdString : parentId;
                        
                        ToStringDepthFirst(subrule, subruleParent, builder, subruleVisitedNodes, indentation + IndentationIncrement);
                    }
                }
                // add subrules of a IMetaRule
                else if (rule is IMetaRule metaRule && metaRule.Subrule != null)
                {
                    var subruleVisitedNodes = AllowDuplicates ? new Dictionary<IRule, string>(visitedRules) : visitedRules;
                    var subruleParent = includeNode ? ruleIdString : parentId;

                    ToStringDepthFirst(metaRule.Subrule, subruleParent, builder, subruleVisitedNodes, indentation + IndentationIncrement);
                }
            }

            return builder.ToString();
        }

        private string ToStringBreadthFirst(IRule root) 
        {
            var visitedRules = new Dictionary<IRule, string>();
            var openList = new List<(IRule rule, string parentId, int indentation, int index)>() { (root, "", 0, 0) };
            var builder = new StringBuilder();

            while (openList.Count > 0)
            {
                var (rule, parentId, indentation, index) = openList[0];
                openList.RemoveAt(0);

                var id = parentId + "_" + index;
                var ruleName = CreateRuleName(parentId, index);

                if (visitedRules.TryGetValue(rule, out string value))
                {
                    builder.AppendLine($"rule_{parentId} --> rule_{value}".AddPrefix(indentation + IndentationIncrement));
                }
                else
                {
                    if (parentId != "")
                    {
                        builder.AppendLine($"rule_{parentId} --> {ruleName}".AddPrefix(indentation));
                    }

                    var ruleText = $"{ruleName}(\"{CreateRuleText(rule)}\")".AddPrefix(indentation);
                    builder.AppendLine(ruleText);
                    visitedRules[rule] = id;

                    if (rule is IRuleGroup group && group.Subrules != null && group.Subrules.Length > 0)
                    {
                        for (var i = 0; i < group.Subrules.Length; i++)
                        {
                            openList.Add((group.Subrules[i], id, indentation + IndentationIncrement, i));
                        }
                    }
                    else if (rule is IMetaRule metaRule && metaRule.Subrule != null)
                    {
                        openList.Add((metaRule.Subrule, id, indentation + IndentationIncrement, 0));
                    }
                }
            }

            return builder.ToString();
        }

        private static string CreateRuleName(string parentId, int child)
        {
            return $"rule_{parentId}_{child}";
        }

        private static string CreateRuleText(IRule rule)
        {
            var tag = rule.Tag.IndexOf("(unnamed)") < 0 ? rule.Tag : "";

            var typeText = rule switch
            {
                LiteralRule lit => $"&quot;{lit.Characters}&quot;",
                OrRule _ => $"{tag}(or)",
                SequenceRule _ => $"{tag}(sequence)",
                RepeatRule _ => rule.Tag,
                NotRule _ => "not",
                CharRule chr => CharRuleString(chr),
                CriticalRule _ => "critical",
                ReferenceRule reference => $"{reference.Reference}(ref)",
                _ => "unknown type"
            };

            if (rule is IRange range)
            {
                if (range.Min != 1 || range.Max != 1)
                {
                    var min = range.Min > 0 ? range.Min.ToString() : "";
                    var max = range.Max > 0 ? range.Max.ToString() : "";
                    typeText += $"[{min}..{max}]";
                }
            }

            return typeText;
        }

        private static string CharRuleString(CharRule rule)
        {
            string match = rule.MatchCharacters switch
            {
                CharRule.MatchType.InRange => "range",
                CharRule.MatchType.Any => "$",
                CharRule.MatchType.InMultiRange => "range",
                CharRule.MatchType.InEnumeration => "enum",
                CharRule.MatchType.NotInEnumeration => "not in enum",
                _ => ""
            };

            return rule.Characters == null 
                ? match 
                : match + " '" + Escape(rule.Characters) + "'";
        }

        private static string Escape(string characters)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < characters.Length; i++)
            {
                var c = characters[i] switch
                {
                    '\n' => "\\eoln;",
                    '\r' => "\\linefeed;",
                    '\t' => "\\tab;",
                    '\"' => "\\quote;",
                    _ => characters[i].ToString()
                };
                        
                builder.Append(c);
            }

            return builder.ToString();  
        }
    }
}

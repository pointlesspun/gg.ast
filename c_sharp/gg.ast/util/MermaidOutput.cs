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
        // class names used in the rules
        private static readonly string ReferenceClassName = "referenceRule";
        private static readonly string LiteralClassName = "literalRule";
        private static readonly string CharClassName = "charRule";

        // styling for each of the rules
        private static readonly string NodeClasses = 
            $"classDef {ReferenceClassName} fill:#EEEEEF,stroke:none\n"
            + $"classDef {LiteralClassName} fill:#EEEEAA,stoke:AAAA99\n"
            + $"classDef {CharClassName} fill:#EEEEAA,stroke:none\n";

        /// <summary>
        /// Indentation of the diagram at the first node
        /// </summary>
        public int StartIndentation { get; set; } = 0;

        /// <summary>
        /// Number of spaces the indentation increases when going down one layer in the rule tree
        /// </summary>
        public int IndentationIncrement { get; set; } = 2;

        /// <summary>
        /// If set to true all rules with NodeVisibility set to Transitive or Hidden will
        /// be removed from the diagram
        /// </summary>
        public bool CullNotVisibleNodes { get; set; } = false;

        /// <summary>
        /// If set to true nodes that have been visited in other branches before will be shown
        /// as a full branch instead of a reference 
        /// </summary>
        public bool AllowDuplicateBranches { get; set; } = false;

        /// <summary>
        /// IF set to true the diagram will show reference rules
        /// </summary>
        public bool ShowReferenceRules { get; set; } = true;

        /// <summary>
        /// Writes a mermaid chart using the given rule to a file.
        /// </summary>
        /// <param name="rule">The rule for which the diagram has to be build</param>
        /// <param name="file">Optional filename, if no filename is given the tag of the rule will be used</param>
        /// <param name="chartType">Mermaid chart type (defaults to flowchart top-down)</param>
        /// <param name="order">Order in which the nodes have to be written to the file depth first or breadth first</param>
        public void ToMDFile(IRule rule, string file = null, string chartType= "flowchart TD")
        {
            var fileName = file ?? rule.Tag + ".md";
            var text = $"```mermaid\n{chartType}\n{ToMermaidChart(rule)}\n```";
            File.WriteAllText(fileName, text);
        }

        /// <summary>
        /// Saves the chart to a .mmc file
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="file"></param>
        /// <param name="chartType"></param>
        /// <param name="order"></param>
        public void ToChartFile(IRule rule, string file = null, string chartType = "flowchart TD")
        {
            var fileName = file ?? rule.Tag + ".mmc";
            File.WriteAllText(fileName, chartType + "\n" + ToMermaidChart(rule));
        }

        /// <summary>
        /// Transforms the given rule into a mermaid chart
        /// </summary>
        /// <param name="rule">The rule for which the diagram has to be build</param>
        /// <param name="order">Order in which the nodes have to be written to the file depth first or breadth first</param>
        /// <returns>a string containing the chart</returns>
        public string ToMermaidChart(IRule rule)
        {
            var builder = new StringBuilder();

            BuildGraph(rule, builder);

            return builder.ToString() + "\n" + NodeClasses;
        }
         
        /// <summary>
        /// Writes the rule tree to the string builder in a depth first order.
        /// </summary>
        /// <param name="rule">The current rule to map to a diagram</param>
        /// <param name="parentId">The full id of a parent consisting of a string forming of Rule.Ids forming the path 
        /// from the parent to the root</param>
        /// <param name="parentHash">The hashcode of the parent</param>
        /// <param name="builder">String builder holding the graph</param>
        /// <param name="visitedRules">Rules which have been mapped to a graph before (keeping track of loops)</param>
        /// <param name="indentation">Current </param>
        /// <returns></returns>
        private void BuildGraph(
            IRule rule,
            StringBuilder builder,
            string parentId = "", 
            string parentHash = "",
            Dictionary<IRule, string> visitedRules = null,  
            int indentation = 0)
        {
            visitedRules ??= new Dictionary<IRule, string>();

            var includeNode = (ShowReferenceRules || !(rule is ReferenceRule)) 
                            && (!CullNotVisibleNodes || rule.Visibility == NodeVisiblity.Visible);
            var ruleIdString = parentId + "_" + rule.Id;

            // we need a custom hash of the current string to get a hash to specific node at 
            // a specific location in the tree which this does. If not (and we're using for instance
            // the rule id) the graph gets too clever and starts reuse the same nodes
            var ruleHash     = ruleIdString.GetHashCode().ToString("x4");

            // if this node has been added to the graph before connect the parent to the other
            // instance
            if (visitedRules.TryGetValue(rule, out string value))
            {
                builder.AppendLine($"{parentHash} -.-> {value}".AddPrefix(indentation + IndentationIncrement));
            }
            else
            {
                if (includeNode)
                {
                    // connect the parent to this node (if any)
                    if (!string.IsNullOrEmpty(parentId))
                    {
                        builder.AppendLine($"{parentHash} --> {ruleHash}".AddPrefix(indentation));
                    }

                    // specify the node's text
                    var ruleText = $"{ruleHash}{CreateRuleMarkup(rule)}".AddPrefix(indentation);
                    builder.AppendLine(ruleText);
                    visitedRules[rule] = ruleHash;
                }

                var subruleParent = includeNode ? ruleIdString : parentId;
                var subruleParentHash = includeNode ? ruleHash : parentHash;

                // add subrules of a IRulegroup
                if (rule is IRuleGroup group && group.Subrules != null && group.Subrules.Length > 0)
                {   
                    for (var i = 0; i < group.Subrules.Length; i++ )
                    {
                        var subrule = group.Subrules[i];
                        var subruleVisitedNodes = AllowDuplicateBranches ? new Dictionary<IRule, string>(visitedRules) : visitedRules;
        
                        BuildGraph(subrule, builder, subruleParent, subruleParentHash, subruleVisitedNodes, indentation + IndentationIncrement);
                    }
                }
                // add subrules of a IMetaRule
                else if (rule is IMetaRule metaRule && metaRule.Subrule != null)
                {
                    var subruleVisitedNodes = AllowDuplicateBranches ? new Dictionary<IRule, string>(visitedRules) : visitedRules;

                    BuildGraph(metaRule.Subrule, builder, subruleParent, subruleParentHash, subruleVisitedNodes, indentation + IndentationIncrement);
                }
            }
        }        

        //
        // Breadth first declarations don't really work in mermaid at the moment... 
        // revisit at a later stage.
        /*private string ToStringBreadthFirst(IRule root) 
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

                    var ruleText = $"{ruleName}{CreateRuleMarkup(rule)}".AddPrefix(indentation);
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
        }*/
        
        /// <summary>
        /// Determine shape and styling of the node
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static string CreateRuleMarkup(IRule rule)
        {
            return rule switch
            {
                ReferenceRule _ => $"[/\"{CreateRuleLabel(rule)}\"/]:::{ReferenceClassName}",
                LiteralRule _ => $"[\"{CreateRuleLabel(rule)}\"]:::{LiteralClassName}",
                CharRule _ => $"[\"{CreateRuleLabel(rule)}\"]:::{CharClassName}",
                _ => $"[\"{CreateRuleLabel(rule)}\"]"
            };
        }

        private static string CreateRuleLabel(IRule rule)
        {
            var tag = rule.Tag.IndexOf("(unnamed)") < 0 ? rule.Tag : "";

            var typeText = rule switch
            {
                LiteralRule lit => $"&quot;{lit.Characters}&quot;",
                OrRule _ => $"{tag}(or)",
                SequenceRule _ => $"{tag}(sequence)",
                RepeatRule _ => rule.Tag,
                NotRule _ => "not",
                CharRule chr => CharRuleLabel(chr),
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

        /// <summary>
        /// Specific mappings for a CharRule
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static string CharRuleLabel(CharRule rule)
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
                : match + " '" + UnEscape(rule.Characters) + "'";
        }

        /// <summary>
        /// Replace invisible characters with visible characters
        /// </summary>
        /// <param name="characters"></param>
        /// <returns></returns>
        private static string UnEscape(string characters)
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

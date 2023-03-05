/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;

using gg.ast.core;
using gg.ast.core.rules;

using gg.ast.common;
using gg.ast.util;

namespace gg.ast.interpreter
{
    public class InterpreterValueMap 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="existingValueMap"></param>
        /// <param name="referenceRuleList">Stores all reference rules created via this valuemap. This way
        /// reference rules can easily be retrieved and resolved at a later time.</param>
        /// <returns></returns>
        public static ValueMap CreateValueMap(
            InterpreterConfig config,
            List<ReferenceRule> referenceRuleList,
            ValueMap existingValueMap = null)
        {
            var interpreterValueMap = existingValueMap ?? TypeRules.CreateValueMap();

            interpreterValueMap[config.Tags.RuleTag] = (str, node) =>
                str.AsSpan(node.StartIndex, node.Length).ToString();

            interpreterValueMap[config.Tags.Literal] = (str, node) => 
                CreateLiteralRule(str, node);

            interpreterValueMap[config.Tags.RuleReference] = (str, node) =>
                CreateReferenceRule(config, str, node, referenceRuleList);

            interpreterValueMap[config.Tags.Sequence] = (str, node) =>
                CreateRuleGroup<SequenceRule>(config, interpreterValueMap, str, node);

            interpreterValueMap[config.Tags.SequenceWithoutSeparator] = (str, node) =>
                CreateRuleGroup<SequenceRule>(config, interpreterValueMap, str, node, false);

            interpreterValueMap[config.Tags.Or] = (str, node) =>
                CreateRuleGroup<OrRule>(config, interpreterValueMap, str, node);

            interpreterValueMap[config.Tags.MatchAnyCharacter] = (str, node) =>
                CreateCharRule(config, str, node, CharRule.MatchType.Any);

            interpreterValueMap[config.Tags.MatchCharactersInRange] = (str, node) =>
                CreateCharRule(config, str, node, CharRule.MatchType.InMultiRange);

            interpreterValueMap[config.Tags.MatchCharactersInEnumeration] = (str, node) =>
                CreateCharRule(config, str, node, CharRule.MatchType.InEnumeration);

            interpreterValueMap[config.Tags.Repeat] = (str, node) =>
                CreateRepeatRule(config, interpreterValueMap, str, node, config.ValueMapWhiteSpace);
            
            interpreterValueMap[config.Tags.RepeatNoWhitespace] = (str, node) =>
                CreateRepeatRule(config, interpreterValueMap, str, node, null);

            interpreterValueMap[config.Tags.RuleValue] = (str, node) =>
                CreateRuleValue(config, interpreterValueMap, str, node);

            interpreterValueMap[config.Tags.UnaryValue] = (str, node) =>
                CreateRuleValue(config, interpreterValueMap, str, node);

            interpreterValueMap[config.Tags.Rule] = (str, node) =>
                CreateRule(interpreterValueMap, str, node);

            return interpreterValueMap;
        }

        private static IRule CreateReferenceRule(
            InterpreterConfig config, 
            string str, 
            AstNode node,
            List<ReferenceRule> referenceRuleList)
        {
            var result = new ReferenceRule()
            {
                Tag = $"{config.Tags.Unnamed} {config.Tags.RuleReference}",
                Reference = str.AsSpan(node.StartIndex, node.Length).ToString(),
                Visibility = NodeVisiblity.Visible
            };
                
            referenceRuleList.Add(result);

            return result;

        }

        private static IRule CreateLiteralRule(string str, AstNode node) 
        {
            var characters = str.AsSpan(node.StartIndex + 1, node.Length - 2).ToString();

            return new LiteralRule()
            {
                Tag = $"\"{characters}\"",
                Characters = characters.ReplaceEscapeCharacters(),
            };
        }       

        private static IRule CreateRuleGroup<T>(
        InterpreterConfig config, 
        ValueMap map, string str, 
        AstNode node, 
        bool hasWhitespace = true) where T : IRuleGroup
        {
            var rule = Activator.CreateInstance<T>();

            rule.WhiteSpaceRule = hasWhitespace ? config.ValueMapWhiteSpace : null;
            rule.Tag = $"{config.Tags.Unnamed} {config.Tags.RuleGroup}";
            rule.Visibility = NodeVisiblity.Transitive;

            if (node.Children != null && node.Children.Count > 0)
            {
                var subrules = new IRule[node.Children.Count];
                var idx = 0;

                foreach (var value in node.Children)
                {
                    var subrule = map.Map<IRule>(str, value);
                    subrules[idx] = subrule;

                    // if the subrule is not a reference, make it hidden 
                    // (that's "how" this is meant to work) 
                    if (config.HideUnnamedRules && IsUnnamedRule(subrule))
                    {
                        subrule.Visibility = NodeVisiblity.Transitive;
                    }

                    idx++;
                }

                rule.Subrules = subrules;
            }

            return rule;
        }

        /// <summary>
        /// An unnamed rule is a rule belonging to a IRuleGroup which is not
        /// explicitely named in the interpreter script. Often this
        /// means it will not produce ast nodes.
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        private static bool IsUnnamedRule(IRule rule)
        {
            if (rule is ReferenceRule)
            {
                return false;
            }

            if (rule is IMetaRule metaRule && metaRule.Subrule is ReferenceRule)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create from a composition of (not) rulevalue (repeat) a single or composite rule.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="map"></param>
        /// <param name="str"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static IRule CreateRuleValue(InterpreterConfig config, ValueMap map, string str, AstNode node)
        {
            var (ruleIndex, notIndex, repeatIndex) = FindRuleValueIndices(config, node);
            
            IRule ruleValue = (IRule)map[node[ruleIndex].Tag](str, node[ruleIndex]);
            
            NotRule notRule = notIndex < 0 
                ? null 
                : CreateNotRule(config, ruleValue, node[notIndex].Tag);

            RepeatRule repeatRule = repeatIndex < 0 
                ? null 
                : CreateRepeatRule(config, map, str, ruleValue, notRule, node[repeatIndex]);

            // some more custom code to deal with the build in properties of
            // the char rule
            if (notIndex >= 0 
                && ruleValue is CharRule charRule 
                && charRule.MatchCharacters == CharRule.MatchType.InEnumeration)
            {
                charRule.MatchCharacters = CharRule.MatchType.NotInEnumeration;
            }

            return repeatRule ?? (notRule ?? ruleValue);
        }

        /// <summary>
        /// Create a not rule for a RuleValue.
        /// </summary>
        /// <param name="ruleValue"></param>
        /// <returns>A configured NotRule or null if no not rule needs to/can be created.</returns>
        private static NotRule CreateNotRule(InterpreterConfig config, IRule ruleValue, string tag) 
        {
            if (ruleValue is CharRule charRule)
            {
                if (charRule.MatchCharacters == CharRule.MatchType.InEnumeration)
                {
                    // the not part is already defined in the charrule
                    // itself don't need to do anything
                    return null;
                }
            }
            
            return new NotRule()
            {
                Subrule = ruleValue,
                Visibility = NodeVisiblity.Transitive,
                Skip = tag == config.Tags.NotAndSkip ? 1 : 0
            };
        }
        
        private static RepeatRule CreateRepeatRule(
            InterpreterConfig config,
            ValueMap map, 
            string str,
            IRule ruleValue, 
            IRule notRule,
            AstNode repeatDefinition)
        {
            // charRule already has a build in repeat component, so we
            // don't need to create a separate rule for this
            if (ruleValue is CharRule charRule)
            {
                SetRangeMinMax(config, map, str, charRule, repeatDefinition[0]);
                return null;
            }
            else if (ruleValue is ReferenceRule refRule && refRule.Subrule is CharRule referenceCharRule)
            {
                SetRangeMinMax(config, map, str, referenceCharRule, repeatDefinition[0]);
                return null;
            }
            else
            {
                // wrap the inline rule inside a repeat rule                    
                var repeatRule = (RepeatRule)map[repeatDefinition.Tag](str, repeatDefinition);

                repeatRule.Subrule = notRule ?? ruleValue;
                repeatRule.Visibility = NodeVisiblity.Transitive;
                return repeatRule;
            }
        }


        /// <summary>
        /// Given a rule value composition which consists of 'optional(not) rulevalue optional(repeat)', 
        /// find the indices. If an optional component (not or repeat) is not present, the index will
        /// be -1.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static (int ruleIndex, int notIndex, int repeatIndex) FindRuleValueIndices(InterpreterConfig config, AstNode node)
        {
            var ruleIndex = -1;
            var notIndex = -1;
            var repeatIndex = -1;

            // if there is only one child, it must be the ruleValue
            if (node.Children.Count == 1)
            {
                ruleIndex = 0;
            }
            // with two children we need to figure out which one is the not 
            // or repeat 
            else if (node.Children.Count == 2)
            {
                if (node.Children[0].Tag == config.Tags.Not || node.Children[0].Tag == config.Tags.NotAndSkip)
                {
                    notIndex = 0;
                    ruleIndex = 1;
                }
                else
                {
                    ruleIndex = 0;
                    repeatIndex = 1;
                }
            }
            // both the not and repeat are present
            else
            {
                notIndex = 0;
                ruleIndex = 1;
                repeatIndex = 2;
            }

            return (ruleIndex, notIndex, repeatIndex);
        }

        private static IRule CreateRule(ValueMap map, string str, AstNode node)
        {
            var visibilityIndex = node.Children.Count == 2 ? -1 : 0;
            var tagIndex = visibilityIndex + 1;
            var ruleIndex = tagIndex + 1;

            var tag = (string)map[node[tagIndex].Tag](str, node[tagIndex]);
            var rule = (IRule)map[node[ruleIndex].Tag](str, node[ruleIndex]);

            rule.Tag = tag;

            if (visibilityIndex >= 0)
            {
                var visibilityNode = node[visibilityIndex];

                if (visibilityNode.Length == 1)
                {
                    rule.Visibility = NodeVisiblity.Transitive;
                }
                else if (visibilityNode.Length == 2)
                {
                    rule.Visibility = NodeVisiblity.Hidden;
                }
                else
                {
                    throw new ParseException(visibilityNode, visibilityNode.Rule, str, visibilityNode.StartIndex);
                }
            }
            else
            {
                // by default named rules produce visible ast nodes
                // (just because that's how the interpreter was intended to work)
                rule.Visibility = NodeVisiblity.Visible;
            }

            return rule;
        }

        private static IRule CreateCharRule(
            InterpreterConfig config, 
            string str, 
            AstNode node,
            CharRule.MatchType match)
        {
            var result = new CharRule()
            {
                MatchCharacters = match,
                Min = 1,
                Max = 1,
            };

            switch (match)
            {
                case CharRule.MatchType.Any:
                    result.Tag = $"{config.Tags.Unnamed} {config.Tags.MatchAnyCharacter}";
                    break;
                case CharRule.MatchType.InMultiRange:
                    result.Tag = $"{config.Tags.Unnamed} {config.Tags.MatchCharactersInRange}";
                    result.Characters = str.Substring(node.StartIndex + 1, node.Length - 2).ReplaceEscapeCharacters();
                    break;
                case CharRule.MatchType.InEnumeration:
                    result.Tag = $"{config.Tags.Unnamed} {config.Tags.MatchCharactersInEnumeration}";
                    result.Characters = str.Substring(node.StartIndex + 1, node.Length - 2).ReplaceEscapeCharacters();
                    break;
                default:
                    throw new ArgumentException("unknown case " + match);
            }

            return result;
        }

        private static IRule CreateRepeatRule(
            InterpreterConfig config, 
            ValueMap map, 
            string str, 
            AstNode node, 
            IRule whitespace)
        {
            var rule = new RepeatRule()
            {
                Tag = config.Tags.Repeat,
                WhiteSpaceRule = whitespace,
                Visibility = NodeVisiblity.Transitive
            };

            SetRangeMinMax(config, map, str, rule, node[0]);
            
            return rule;
        }
        
        private static void SetRangeMinMax(
            InterpreterConfig config, 
            ValueMap map, 
            string str,
            IRange rangedRule,
            AstNode repeatDefinition)
        {
            if (repeatDefinition.Tag == config.Tags.RepeatBetweenNandM)
            {
                rangedRule.Min = map.Map<int>(str, repeatDefinition[0]);
                rangedRule.Max = map.Map<int>(str, repeatDefinition[1]);
            }
            else if (repeatDefinition.Tag == config.Tags.RepeatExact)
            {
                rangedRule.Min = map.Map<int>(str, repeatDefinition[0]);
                rangedRule.Max = rangedRule.Min;
            }
            else if (repeatDefinition.Tag == config.Tags.RepeatNoMoreThanN)
            {
                rangedRule.Min = -1;
                rangedRule.Max = map.Map<int>(str, repeatDefinition[0]);
            }
            else if (repeatDefinition.Tag == config.Tags.RepeatNOrMore)
            {
                rangedRule.Min = map.Map<int>(str, repeatDefinition[0]);
                rangedRule.Max = -1;
            }
            else if (repeatDefinition.Tag == config.Tags.RepeatOneOrMore)
            {
                rangedRule.Min = 1;
                rangedRule.Max = -1;
            }
            else if (repeatDefinition.Tag == config.Tags.RepeatZeroOrMore)
            {
                rangedRule.Min = -1;
                rangedRule.Max = -1;
            }
            else if (repeatDefinition.Tag == config.Tags.RepeatZeroOrOne)
            {
                rangedRule.Min = -1;
                rangedRule.Max = 1;
            }
            else
            {
                throw new ArgumentException($"Unknown repeat tag {repeatDefinition.Tag}");
            }
        }
    }
}

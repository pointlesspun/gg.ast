/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core;
using gg.ast.core.rules;

using gg.ast.interpreter;

using static gg.ast.interpreter.InterpreterRules;
using static gg.ast.tests.TestUtil;

namespace gg.ast.tests.interpreter
{
    [TestClass]
    public class InterpreterRuleTests
    {
        
        [TestMethod]
        public void LiteralRuleGreenPath()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string tag, string literal)[] {
                ("foo= \"literal\";", "foo", "literal"),
                ("_bar= \"abc\";", "_bar", "abc"),
                ("@123  =\"123\";", "@123", "123"),
                ("foo= \"\\\"literal\\\"\";", "foo", "\"literal\""),
                ("1_Abc  =  \"\\\"xx\\\"\";", "1_Abc","\"xx\"")
            };

            RunGreenPathTests(InterpreterRule(new InterpreterConfig()), examples.Select(v => v.text), (result, idx) =>
            {
                if (result.IsSuccess
                    && result.Nodes[0].Tag == config.Tags.Rule
                    && result.Nodes[0].Children.Count == 2
                    && result.Nodes[0][0].Tag == config.Tags.RuleTag
                    && result.Nodes[0][1].Tag == config.Tags.RuleValue
                    && result.Nodes[0][1][0].Tag == config.Tags.Literal)
                {
                    var rule = valueMap.Map<LiteralRule>(examples[idx].text, result.Nodes[0]);

                    return rule != null
                        && rule.Tag == examples[idx].tag
                        && rule.Characters == examples[idx].literal;
                }

                return false;
            });
        }

        [TestMethod]
        public void RuleRefRuleGreenPath()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string tag, string reference)[] {
                ("foo= ref;", "foo", "ref"),
                ("_bar = _long_ref;", "_bar", "_long_ref"),
                ("@123  =123;", "@123", "123"),
            };

            RunGreenPathTests(InterpreterRule(new InterpreterConfig()), examples.Select(v => v.text), (result, idx) =>
            {
                if (result.IsSuccess
                    && result.Nodes[0].Tag == config.Tags.Rule
                    && result.Nodes[0].Children.Count == 2
                    && result.Nodes[0][0].Tag == config.Tags.RuleTag
                    && result.Nodes[0][1].Tag == config.Tags.RuleValue
                    && result.Nodes[0][1][0].Tag == config.Tags.RuleReference)
                {
                    var reference = valueMap.Map<ReferenceRule>(examples[idx].text, result.Nodes[0]);

                    return reference != null
                        && reference.Tag == examples[idx].tag
                        && reference.Reference == examples[idx].reference;
                }

                return false;
            });
        }

        [TestMethod]
        public void GroupGreenPath()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string[] tags)[] {
                ("(ref)", new string[] { config.Tags.RuleReference }),
                ("( \"lit\" )", new string[] { config.Tags.Literal }),
                ("(ref, \"lit\")", new string[] { config.Tags.Sequence }),
                ("( ref \"lit\")", new string[] { config.Tags.SequenceWithoutSeparator }),
                ("( ref , \"lit\", another_ref )", new string[] { config.Tags.Sequence }),
                ("(ref , \"lit\", (another_ref, ref))", new string[] { config.Tags.Sequence }),
            };

            RunGreenPathTests(GroupingRule(new InterpreterConfig()), examples.Select(v => v.text), (result, idx) =>
            {
                if (result.Nodes[0].Tag == config.Tags.RuleValue
                    && result.Nodes[0].Children.Count == examples[idx].tags.Length)
                {
                    var children = result.Nodes[0].Children;

                    for (var i = 0; i < children.Count; i++)
                    {
                        if (children[i].Tag != examples[idx].tags[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            });
        }


        [TestMethod]
        public void SequenceRuleGreenPath()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string[] tags)[] {
                (" ref, \"lit\" ", new string[] { config.Tags.RuleReference, config.Tags.Literal  }),
                ("ref , \"lit\", (another_ref, \"lit2\") ", new string[] { config.Tags.RuleReference, config.Tags.Literal, config.Tags.RuleValue }),
                ("ref , \"lit\", ref,ref,ref", new string[] { config.Tags.RuleReference, config.Tags.Literal, config.Tags.RuleReference, config.Tags.RuleReference, config.Tags.RuleReference}),
            };

            RunGreenPathTests(SequenceRule(new InterpreterConfig()), examples.Select(v => v.text), (result, idx) =>
            {
                if (result.Nodes[0].Tag == config.Tags.Sequence
                    && result.Nodes[0].Children.Count == examples[idx].tags.Length)
                {
                    var children = result.Nodes[0].Children;

                    for (var i = 0; i < children.Count; i++)
                    {
                        if (children[i].Tag != config.Tags.UnaryValue || children[i][0].Tag != examples[idx].tags[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            });
        }


        [TestMethod]
        public void NoSeparatorSequenceRuleGreenPath()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string[] tags)[] {
                ("ref \"lit\"  ", new string[] { config.Tags.RuleReference, config.Tags.Literal  }),
                ("ref   \"lit\" ref  \nref \t  ref", new string[] { config.Tags.RuleReference, config.Tags.Literal, config.Tags.RuleReference, config.Tags.RuleReference, config.Tags.RuleReference}),
                ("ref (inner_seq, seq)", new string[] { config.Tags.RuleReference, config.Tags.RuleValue }),
                ("ref \n\"lit\" \t ( another_ref, \"lit2\")  ", new string[] { config.Tags.RuleReference, config.Tags.Literal, config.Tags.RuleValue }),
                ("ref \n ( another_ref \"lit2\") ", new string[] { config.Tags.RuleReference, config.Tags.RuleValue}),
            };

            var rule = NoSeparatorSequenceRule(config);

            Debug.WriteLine(rule.PrintRuleTree());

            RunGreenPathTests(rule, examples.Select(v => v.text), (result, idx) =>
            {
                if (result.Nodes[0].Tag == config.Tags.SequenceWithoutSeparator
                    && result.Nodes[0].Children.Count == examples[idx].tags.Length)
                {
                    var children = result.Nodes[0].Children;

                    for (var i = 0; i < children.Count; i++)
                    {
                        if (children[i].Tag != config.Tags.UnaryValue || children[i][0].Tag != examples[idx].tags[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            });
        }

        [TestMethod]
        public void SequenceRuleRedPath()
        {
            var config = new InterpreterConfig();

            var invalidExamples = new string[]
            {
                "",
                "ref",
                " ref",
                "ref |"
            };

            var exceptionExamples = new (string text, string tag)[] {
                ("ref, ", config.Tags.UnaryValue),
                ("ref , \"lit\",", config.Tags.UnaryValue),
                ("ref , \"lit\",", config.Tags.UnaryValue),
                ("ref , \"lit\",(ref, ", config.Tags.UnaryValue),
                ("ref , \"lit\",(ref,ref :)", config.Tags.EndGroup),
            };

            RunRedPathTests(SequenceRule(config),
                invalidExamples: invalidExamples,
                exceptionExamples: exceptionExamples);
        }

        [TestMethod]
        public void OrRuleGreenPath()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string[] tags)[] {
                (" ref | \"lit\" ", new string[] { config.Tags.RuleReference, config.Tags.Literal  }),
                ("ref  |  \"lit\"| (another_ref, \"lit2\") ", new string[] { config.Tags.RuleReference, config.Tags.Literal, config.Tags.RuleValue }),
                ("ref[1..2]  | \"lit\" | ref|ref|ref)", new string[]
                {
                    config.Tags.RuleReference, config.Tags.Literal, config.Tags.RuleReference, config.Tags.RuleReference, config.Tags.RuleReference
                }),
            };

            RunGreenPathTests(CreateOrRule(config), examples.Select(v => v.text), (result, idx) =>
            {
                if (result.IsSuccess
                    && result.Nodes[0].Tag == config.Tags.Or
                    && result.Nodes[0].Children.Count == examples[idx].tags.Length)
                {
                    var children = result.Nodes[0].Children;

                    for (var i = 0; i < children.Count; i++)
                    {
                        if (children[i].Tag != config.Tags.UnaryValue || children[i][0].Tag != examples[idx].tags[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            });
        }

        [TestMethod]
        public void RepeatGreenPath()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string parentTag, string tag, int min, int max)[] {
                
                ("[]", config.Tags.Repeat, config.Tags.RepeatZeroOrMore, -1, -1 ),
                ("[4]", config.Tags.Repeat, config.Tags.RepeatExact, 4, 4),
                ("[1..]", config.Tags.Repeat, config.Tags.RepeatNOrMore, 1, -1),
                ("[..4]", config.Tags.Repeat, config.Tags.RepeatNoMoreThanN, -1, 4),
                ("[1..4]", config.Tags.Repeat, config.Tags.RepeatBetweenNandM, 1, 4),
                
                ("<>", config.Tags.RepeatNoWhitespace, config.Tags.RepeatZeroOrMore, -1, -1 ),
                ("<4>", config.Tags.RepeatNoWhitespace, config.Tags.RepeatExact, 4, 4),
                ("<1..>", config.Tags.RepeatNoWhitespace, config.Tags.RepeatNOrMore, 1, -1),
                ("<..4>", config.Tags.RepeatNoWhitespace, config.Tags.RepeatNoMoreThanN, -1, 4),
                ("<1..4>", config.Tags.RepeatNoWhitespace, config.Tags.RepeatBetweenNandM, 1, 4),

                ("?", config.Tags.RepeatNoWhitespace, config.Tags.RepeatZeroOrOne, -1, 1 ),
                ("*", config.Tags.RepeatNoWhitespace, config.Tags.RepeatZeroOrMore, -1, -1),
                ("+", config.Tags.RepeatNoWhitespace, config.Tags.RepeatOneOrMore, 1, -1),
            };

            RunGreenPathTests(CreateRepeatRule(config), examples.Select(v => v.text), (result, idx) =>
            {
                var (text, parentTag, tag, min, max) = examples[idx];

                if (result.IsSuccess
                    && result.Nodes[0].Tag == parentTag
                    && result.Nodes[0].Children.Count == 1
                    && result.Nodes[0][0].Tag == tag)
                {
                    var repeatRule = valueMap.Map<RepeatRule>(text, result.Nodes[0]);

                    return repeatRule.Min == min && repeatRule.Max == max;
                }

                return false;
            });
        }

        [TestMethod]
        public void RepeatValueGreenPath()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string tag, int min, int max)[] {
                ("foo[]", config.Tags.RepeatZeroOrMore, -1, -1 ),
                ("foo[4]", config.Tags.RepeatExact, 4, 4),
                ("foo[1..]", config.Tags.RepeatNOrMore, 1, -1),
                ("foo[..4]", config.Tags.RepeatNoMoreThanN, -1, 4),
                ("foo[1..4]", config.Tags.RepeatBetweenNandM, 1, 4),
                ("foo*", config.Tags.RepeatZeroOrMore, -1, -1),
                ("foo+", config.Tags.RepeatOneOrMore, 1, -1),
                ("foo?", config.Tags.RepeatZeroOrOne, -1, 1),
            };

            RunGreenPathTests(RuleValue(config), examples.Select(v => v.text), (result, idx) =>
            {
                var (text, tag, min, max) = examples[idx];

                if (result.Nodes[0].Tag == config.Tags.RuleValue)
                {
                    var repeatRule = valueMap.Map<RepeatRule>(text, result.Nodes[0]);

                    return repeatRule != null
                        && repeatRule.Subrule is ReferenceRule
                        && repeatRule.Min == examples[idx].min
                        && repeatRule.Max == examples[idx].max;
                }

                return false;
            });
        }

        [TestMethod]
        public void RuleListGreenPath()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, (string tag, string ruleValueTag)[] expectedResult)[] {
                ("foo= ref;", new (string tag, string className)[] {
                    // expect a rule tagged "foo" defined as a rule reference
                    ("foo", config.Tags.RuleReference)
                }),
                ("r1= ref;\nr2= \"lit\" ;", new (string tag, string className)[] { 
                    // expect a rule tagged "r1" defined as a rule reference
                    ("r1", config.Tags.RuleReference),
                    
                    // expect a rule tagged "r2" defined as a literal                    
                    ("r2", config.Tags.Literal)
                })
            };

            RunGreenPathTests(CreateRuleListRule(config), examples.Select(v => v.text), (result, idx) =>
            {
                if (result.IsSuccess
                    && result.Nodes[0].Tag == config.Tags.RuleList
                    && result.Nodes[0].Children.Count == examples[idx].expectedResult.Length)
                {
                    for (var i = 0; i < result.Nodes[0].Children.Count; i++)
                    {
                        var (tag, ruleValueTag) = examples[idx].expectedResult[i];
                        var child = result.Nodes[0].Children[i];

                        // check if the rule composition is correct
                        if (child.Tag == config.Tags.Rule && child[0].Tag == config.Tags.RuleTag)
                        {
                            // get the tag name
                            var ruleTagName = (string)valueMap[config.Tags.RuleTag](examples[idx].text, child[0]);

                            if (ruleTagName != tag || child[1][0].Tag != ruleValueTag)
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                return false;
            });
        }

    }
}

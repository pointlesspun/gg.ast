/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
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
    public class NotRuleTests
    {
        [TestMethod]
        public void NotUnaryValueTest()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, int childCount, string tag, string notTag, Type[] expectedTypes)[] {

                ("~\"lit\"", 2, config.Tags.Literal, config.Tags.NotAndSkip,
                        new Type[] { typeof(NotRule), typeof(LiteralRule) }),

                 ("!\"lit\"", 2, config.Tags.Literal, config.Tags.Not,
                        new Type[] { typeof(NotRule), typeof(LiteralRule) }),

                ("!\"lit\" []", 3, config.Tags.Literal, config.Tags.Not,
                        new Type[] { typeof(RepeatRule), typeof(NotRule), typeof(LiteralRule) }),

                // expect one child here as both the not and repeat will be folded into the
                // char rule 
                ("!'enumeration'[]", 3, config.Tags.MatchCharactersInEnumeration, config.Tags.Not,
                         new Type[] { typeof(CharRule) }),

                ("!`range`[]", 3, config.Tags.MatchCharactersInRange, config.Tags.Not,
                         new Type[] { typeof(NotRule), typeof(CharRule) }),

                ("!$[]", 3, config.Tags.MatchAnyCharacter, config.Tags.Not,
                         new Type[] { typeof(NotRule), typeof(CharRule) }),

                ("!(a, b)[]", 3, config.Tags.RuleValue, config.Tags.Not,
                         new Type[] { typeof(RepeatRule), typeof(NotRule), typeof(SequenceRule) }),
            };

            RunGreenPathTests(RuleValue(new InterpreterConfig()), examples.Select(v => v.text), (result, idx) =>
            {
                var (text, childCount, tag, notTag, types) = examples[idx];

                if (result.Nodes[0].Tag == config.Tags.RuleValue
                    && result.Nodes[0].Children.Count == childCount
                    && result.Nodes[0][0].Tag == notTag
                    && result.Nodes[0][1].Tag == tag)
                {
                    var rule = valueMap.Map(result.Nodes[0].Tag, examples[idx].text, result.Nodes[0]);

                    // go over the rule's children to see if they match the expected types
                    if (rule != null)
                    {
                        var current = (IRule)rule;
                        foreach (var expectType in types)
                        {
                            if (current.GetType() != expectType)
                            {
                                return false;
                            }

                            if (current is IMetaRule metaRule)
                            {
                                current = metaRule.Subrule;
                            }
                        }
                    }

                    return true;
                }

                return false;
            });
        }

        /// <summary>
        ///  Reproduce a bug where a not starting a sequence would generate a 'not' followed by a 
        ///  'sequence' instead of a 'sequence' with a 'not literal' and 'anychar'. This test
        ///  should check if it is fixed.
        /// </summary>
        [TestMethod]
        public void NotUnarySequenceTest()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());
            var rule = RuleValue(new InterpreterConfig());
            var text = "!foo $";
            var result = rule.Parse(text);

            var generatedRule = (IRule) valueMap.Map(result.Nodes[0].Tag, text, result.Nodes[0]);

            // previously this would be a not rule
            Assert.IsTrue(generatedRule is SequenceRule);
        }
    }
}
/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core.rules;
using gg.ast.interpreter;

using static gg.ast.interpreter.InterpreterRules;

using static gg.ast.tests.TestUtil;

namespace gg.ast.tests.interpreter
{
    [TestClass]
    public class CharacterRuleTests
    {
        /// <summary>
        /// Create a rule for a "match any character" rule and defined repeats.
        /// </summary>
        [TestMethod]
        public void AnyCharTest()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, int childCount, int min, int max)[] {
                // no repeat, should only match 1 character
                ("$", 1, 1, 1),

                // endless repeats, but no minimum
                ("$[]", 2, -1, -1),

                // exactly 4 repeats
                ("$[4]", 2, 4, 4),
                ("$[..4]", 2, -1, 4),
                ("$[1..4]", 2, 1, 4),
                ("$[1..]", 2, 1, -1),
                ("$?", 2, -1, 1),
                ("$*", 2, -1, -1),
                ("$+", 2, 1, -1),
            };

            RunGreenPathTests(RuleValue(new InterpreterConfig()), examples.Select(v => v.text), (result, idx) =>
            {
                if (result.Nodes[0].Tag == config.Tags.RuleValue
                    && result.Nodes[0].Children.Count == examples[idx].childCount
                    && result.Nodes[0][0].Tag == config.Tags.MatchAnyCharacter)
                {
                    var rule = valueMap.Map<CharRule>(examples[idx].text, result.Nodes[0]);

                    return rule != null
                        && rule.Min == examples[idx].min
                        && rule.Max == examples[idx].max;
                }

                return false;
            });
        }

        /// <summary>
        /// Create a rule to match a range of characters.
        /// </summary>
        [TestMethod]
        public void CharacterRangeTest()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string[] validInput, string[] invalidInput)[] {
                ("`az`", new string[] { "a", "b", "x", "z"}, new string[] { "9", "A", "Z", "_" }),
                ("`azAZ`", new string[] { "a", "z", "A", "Z"}, new string[] { "9", "_" }),
                ("`09`[3]", new string[] { "123", "009" }, new string[] { "9", "90a", "a91", "92" }),
                ("`09` [1..4]", new string[] { "1", "12", "341", "0091" }, new string[] { "", "a91"  }),
            };

            RunGreenPathTests(RuleValue(new InterpreterConfig()), examples.Select(v => v.text), (result, idx) =>
            {
                if (result.Nodes[0].Tag == config.Tags.RuleValue
                    && result.Nodes[0][0].Tag == config.Tags.MatchCharactersInRange)
                {
                    var rule = valueMap.Map<CharRule>(examples[idx].text, result.Nodes[0]);

                    foreach (var validText in examples[idx].validInput)
                    {
                        var inputResult = rule.Parse(validText);
                        if (!inputResult.IsSuccess || inputResult.CharactersRead < validText.Length)
                        {
                            return false;
                        }
                    }

                    foreach (var invalidText in examples[idx].invalidInput)
                    {
                        var inputResult = rule.Parse(invalidText);
                        if (inputResult.IsSuccess)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// Create a rule to match a character in an enumeration.
        /// </summary>
        [TestMethod]
        public void CharacterIEnumetationTest()
        {
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());

            var examples = new (string text, string[] validInput, string[] invalidInput)[] {
                ("'abc'", new string[] { "a", "b", "c"}, new string[] { "d", "A", "Z", "_" }),
                ("'\\n\\t\\r '", new string[] { " ", "\r", "\t", "\n"}, new string[] { "a", "A", "Z", "_" }),
                ("'abc123'", new string[] { "a", "b", "c", "1", "2", "3"}, new string[] { "9", "_", "d" }),
                ("'09'[3]", new string[] { "090", "009" }, new string[] { "9", "90", "919", "001" }),
                ("'0123' [1..4]", new string[] { "1230", "1111", "31", "123" }, new string[] { "", "432"  }),
            };

            RunGreenPathTests(RuleValue(new InterpreterConfig()), examples.Select(v => v.text), (result, idx) =>
            {
                if (result.Nodes[0].Tag == config.Tags.RuleValue
                    && result.Nodes[0][0].Tag == config.Tags.MatchCharactersInEnumeration)
                {
                    var rule = valueMap.Map<CharRule>(examples[idx].text, result.Nodes[0]);

                    foreach (var validText in examples[idx].validInput)
                    {
                        var inputResult = rule.Parse(validText);
                        if (!inputResult.IsSuccess || inputResult.CharactersRead < validText.Length)
                        {
                            return false;
                        }
                    }

                    foreach (var invalidText in examples[idx].invalidInput)
                    {
                        var inputResult = rule.Parse(invalidText);
                        if (inputResult.IsSuccess)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            });
        }
    }
}

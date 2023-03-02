/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.examples.introduction;
using gg.ast.core.rules;
using gg.ast.common;
using gg.ast.core;
using gg.ast.util;

namespace gg.ast.examples.test.introduction
{
    [TestClass]
    public class RepeatTest
    {       
        private static readonly IRule HelloWorldRule = new LiteralRule()
        {
            Tag = "helloWorld",
            Characters = "hello world"
        };

        private static readonly IRule WhiteSpaceRule = ShortHandRules.CreateWhitespaceRule();
        
        private static readonly Dictionary<string, IRule> specRules = RepeatExample.LoadSpecFileRules();

        private static readonly string helloWorldText = "hello world";
        private static readonly string hiWeldText = "hi weld";

        /// <summary>
        /// Test the given rules against the given testData. Parse both rules against (all the) testData.text
        /// and asserts if outcome matches the expected outcomes.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="specFileRuleName"></param>
        /// <param name="testData"></param>
        static void TestRules(IEnumerable<(string text, bool isSuccess, int expectedLength)> testData, params object[] rules)
        {
            foreach (var (text, isSuccess, expectedLength) in testData)
            {
                foreach (var ruleReference in rules)
                {
                    var rule = ruleReference is string ruleName
                                ? specRules[ruleName] 
                                : (IRule) ruleReference;

                    var result = rule.Parse(text);

                    Assert.IsTrue(result.IsSuccess == isSuccess);
                    Assert.IsTrue(result.CharactersRead == expectedLength);
                }
            }
        }

        [TestMethod]
        public void ZeroOrMoreTest()
        {
            var testData = new List<(string text, bool isSuccess, int expectedLength)>() {
                ("", true, 0),
                (helloWorldText, true, helloWorldText.Length),
                (hiWeldText, true, 0),
                // Last addition of +2 and not +3 is because the trailing whitespace character is not read
                (helloWorldText.Repeat(3, " "), true, helloWorldText.Length * 3 + 2),
            };

            var zeroOrMore = new RepeatRule()
            {
                Subrule = HelloWorldRule,
                Min = 0,
                Max = -1,
                WhiteSpaceRule = WhiteSpaceRule
            };

            TestRules(testData, zeroOrMore, "explicitWs.zeroOrMore");
        }

        [TestMethod]
        public void ZeroOrMoreNoWhitespaceTest()
        {
            var testData = new List<(string text, bool isSuccess, int expectedLength)>() {
                ("", true, 0),
                (helloWorldText, true, helloWorldText.Length),
                (hiWeldText, true, 0),
                (helloWorldText.Repeat(3, " "), true, helloWorldText.Length),
                (helloWorldText.Repeat(3, ""), true, helloWorldText.Length * 3),
            };

            var zeroOrMore = new RepeatRule()
            {
                Subrule = HelloWorldRule,
                Min = 0,
                Max = -1,
            };

            TestRules(testData, zeroOrMore, "zeroOrMore", "explicitNoWs.zeroOrMore");
        }

        [TestMethod]
        public void ZeroOrOneTest()
        {
            var testData = new (string text, bool isSuccess, int expectedLength)[] {
                ("", true, 0),
                (helloWorldText, true, helloWorldText.Length),
                (hiWeldText, true, 0),
                (helloWorldText.Repeat(3, " "), true, helloWorldText.Length),
            };

            var zeroOrOne = new RepeatRule()
            {
                Subrule = HelloWorldRule,
                Min = 0,
                Max = 1,
                WhiteSpaceRule = WhiteSpaceRule
            };

            TestRules(testData, zeroOrOne, "explicitWs.zeroOrOne", "explicitNoWs.zeroOrOne", "zeroOrOne");
        }

        [TestMethod]
        public void OneOrMoreTest()
        {
            var testData = new (string text, bool isSuccess, int expectedLength)[] {
                ("", false, 0),
                (helloWorldText, true, helloWorldText.Length),
                (hiWeldText, false, 0),
                // Last addition of +2 and not +3 is because the trailing whitespace character is not read
                (helloWorldText.Repeat(3, " "), true, helloWorldText.Length * 3 + 2),
            };

            var oneOrOne = new RepeatRule()
            {
                Subrule = HelloWorldRule,
                Min = 1,
                Max = -1,
                WhiteSpaceRule = WhiteSpaceRule
            };

            TestRules(testData, oneOrOne, "explicitWs.oneOrMore");
        }

        [TestMethod]
        public void OneOrMoreNoWhitespaceTest()
        {
            var testData = new (string text, bool isSuccess, int expectedLength)[] {
                ("", false, 0),
                (helloWorldText, true, helloWorldText.Length),
                (hiWeldText, false, 0),
                (helloWorldText.Repeat(3, " "), true, helloWorldText.Length),
                (helloWorldText.Repeat(3, ""), true, helloWorldText.Length * 3),
            };

            var oneOrOne = new RepeatRule()
            {
                Subrule = HelloWorldRule,
                Min = 1,
                Max = -1,
            };

            TestRules(testData, oneOrOne, "explicitNoWs.oneOrMore", "oneOrMore");
        }

        [TestMethod]
        public void OneOrTwoTest()
        {
            var testData = new (string text, bool isSuccess, int expectedLength)[] {
                ("", false, 0),
                (helloWorldText, true, helloWorldText.Length),
                (hiWeldText, false, 0),
                // Last addition of +1 and not +2 is because the trailing whitespace character is not read
                (helloWorldText.Repeat(3, " "), true, helloWorldText.Length * 2 + 1),
            };

            var oneOrOne = new RepeatRule()
            {
                Subrule = HelloWorldRule,
                Min = 1,
                Max = 2,
                WhiteSpaceRule = WhiteSpaceRule
            };

            TestRules(testData, oneOrOne, "explicitWs.oneOrTwo");
        }

        [TestMethod]
        public void OneOrTwoNoWhitespaceTest()
        {
            var testData = new (string text, bool isSuccess, int expectedLength)[] {
                ("", false, 0),
                (helloWorldText, true, helloWorldText.Length),
                (hiWeldText, false, 0),
                (helloWorldText + hiWeldText, true, helloWorldText.Length),
                (helloWorldText.Repeat(3, " "), true, helloWorldText.Length),
                (helloWorldText.Repeat(3, ""), true, helloWorldText.Length * 2),
            };

            var oneOrOne = new RepeatRule()
            {
                Subrule = HelloWorldRule,
                Min = 1,
                Max = 2,
            };

            TestRules(testData, oneOrOne, "explicitNoWs.oneOrTwo");
        }

        [TestMethod]
        public void TwoTest()
        {
            var testData = new (string text, bool isSuccess, int expectedLength)[] {
                ("", false, 0),
                (helloWorldText, false, 0),
                (hiWeldText, false, 0),
                // Last addition of +1 and not +2 is because the trailing whitespace character is not read
                (helloWorldText.Repeat(2, " "), true, helloWorldText.Length * 2 + 1),
                (helloWorldText.Repeat(3, " "), true, helloWorldText.Length * 2 + 1),
            };

            var oneOrOne = new RepeatRule()
            {
                Subrule = HelloWorldRule,
                Min = 2,
                Max = 2,
                WhiteSpaceRule = WhiteSpaceRule
            };

            TestRules(testData, oneOrOne, "explicitWs.two");
        }


        [TestMethod]
        public void TwoTestNoWhitespace()
        {
            var testData = new (string text, bool isSuccess, int expectedLength)[] {
                ("", false, 0),
                (helloWorldText, false, 0),
                (hiWeldText, false, 0),
                (helloWorldText + hiWeldText, false, 0),
                (helloWorldText.Repeat(2, " "), false, 0),
                (helloWorldText.Repeat(2, ""), true, helloWorldText.Length * 2),
                (helloWorldText.Repeat(3, ""), true, helloWorldText.Length * 2),
            };

            var oneOrOne = new RepeatRule()
            {
                Subrule = HelloWorldRule,
                Min = 2,
                Max = 2,
            };

            TestRules(testData, oneOrOne, "explicitNoWs.two");
        }
    }
}


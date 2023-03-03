/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core;
using gg.ast.core.rules;

using static gg.ast.common.TypeRules;
using static gg.ast.tests.TestUtil;

namespace gg.ast.tests.common
{
    [TestClass]
    public class TypesTest
    {
        public static IRule DefaultWhitespace = ast.common.ShortHandRules.CreateWhitespaceRule();
        private static readonly ValueMap _valueMap = CreateValueMap();

        [TestMethod]
        public void IntegerGreenPath()
        {
            var testData = new (string text, int expectedLength, int expectedValue)[] {
                ("-1234 ", 5, -1234),
                ("1234", 4, 1234),
                ("+1234", 5, 1234),
                ("12.34", 2, 12),
            };

            RunGreenPathTests(CreateIntegerRule(), testData.Select(v => v.text), (result, idx) =>
            {
                var (text, expectedLength, expectedValue) = testData[idx];

                return result.CharactersRead == expectedLength
                        && _valueMap.Map<int>(text, result.Nodes[0]) == expectedValue;
            });
        }



        [TestMethod]
        public void IntegerRedPath()
        {
            // xxx to do: add exceptions for strings starting with "-", "+"
            RunRedPathTests(CreateIntegerRule(), new string[] { "", "a1234" });
        }


        [TestMethod]
        public void DecimalTest()
        {
            var text = new string[] { "0.1234", "-1234.0 ", "+1234.0567", "-12.-34", "1234" };
            var valueMap = CreateValueMap();
            var rule = CreateDecimalRule();
            var parseResult = rule.Parse(text[0]);

            Assert.IsTrue(parseResult.IsSuccess);
            Assert.IsTrue(parseResult.CharactersRead == 6);

            var value = parseResult.Map<double>(text[0], valueMap);
            Assert.IsTrue(Math.Abs(value - 0.1234) <= 0.0001);

            parseResult = rule.Parse(text[1], 0);
            Assert.IsTrue(parseResult.IsSuccess);
            Assert.IsTrue(parseResult.CharactersRead == 7);

            value = parseResult.Map<double>(text[1], valueMap);
            Assert.IsTrue(Math.Abs(value + 1234.0) <= 0.0001);

            parseResult = rule.Parse(text[2], 0);
            Assert.IsTrue(parseResult.IsSuccess);
            Assert.IsTrue(parseResult.CharactersRead == 10);

            value = parseResult.Map<double>(text[2], valueMap);
            Assert.IsTrue(Math.Abs(value - 1234.0567) <= 0.0001);

            Assert.IsFalse(rule.Parse(text[3], 0).IsSuccess);
            Assert.IsFalse(rule.Parse(text[4], 0).IsSuccess);
        }


        [TestMethod]
        public void ExponentTest()
        {
            var text = new string[] { "1234E1", "0.1234E1", "0.1234e12", "-0.1234e-123", "-1234.0", "1234.05e", "1234e" };
            var expectedValues = new double[] { 1234E1, 0.1234E1, 0.1234e12, -0.1234e-123 };
            var rule = CreateExponentRule();
            var valueMap = CreateValueMap();

            for (var i = 0; i < expectedValues.Length; i++)
            {
                var result = rule.Parse(text[i]);
                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.CharactersRead == text[i].Length);
                var value = result.Map<double>(text[i], valueMap);
                Assert.IsTrue(Math.Abs(value - expectedValues[i]) <= 0.0001);
            }

            for (var i = expectedValues.Length; i < text.Length; i++)
            {
                var result = rule.Parse(text[i]);
                Assert.IsFalse(result.IsSuccess);
            }
        }

        [TestMethod]
        public void NumberTest()
        {
            var text = new string[] { "1234E1", "-12.34", "+42" };
            var expectedTags = new string[] { Tags.Exponent, Tags.Decimal, Tags.Integer };
            var rule = CreateNumberRule();

            RunGreenPathTests(rule, text, (result, idx) =>
                result.IsSuccess
                && result.CharactersRead == text[idx].Length
                && result.Nodes[0].Rule.Tag == expectedTags[idx]
            );
        }


        [TestMethod]
        public void HexTest()
        {
            var rule = CreateHexCharactersRule();
            var validExamples = new string[] { "00", "1", "99e", "eEef", "f00A", "D018" };
            var invalidExamples = new string[] { "x12", "G00", "-05Ef", "" };

            RunGreenPathTests(rule, validExamples);
            ParseRuleExpectFailure(rule, invalidExamples);
        }


        [TestMethod]
        public void UnicodeEscapeTest()
        {
            var min = 4;
            var max = 4;
            var rule = CreateEscapeRule(CreateHexCharactersRule(min: min, max: max, abortOnFailure: true));

            var validExamples = new string[] { "\\U1234", "\\u0001", "\\ud018 ", "\\U1a2f34" };
            var invalidExamples = new string[] { "U", "", " \\u1234" };
            var exceptionExamples = new string[] { "\\", "\\U123", "\\u", "\\R1234", "\\u123-", "\\u123g" };

            RunGreenPathTests(rule, validExamples, (result, _) => result.CharactersRead == max + 2);
            ParseRuleExpectFailure(rule, invalidExamples);
            ParseRuleExpectException(rule, exceptionExamples, (exception) => exception.Rule.Tag == Tags.EscapeSpecification || exception.Rule.Tag == Tags.HexString);
        }

        [TestMethod]
        public void HexEscapeTest()
        {
            var rule = CreateEscapeRule(CreateHexCharactersRule(min: 2, max: 2, abortOnFailure: true), letterEnumeration: "Xx");
            var validExamples = new string[] { "\\x12", "\\Xa1" };
            var invalidExamples = new string[] { " \\x2", "" };
            var exceptionExamples = new string[] { "\\x2", "\\", "\\x1", "\\X", "\\XF", "\\x0g", "\\a0010" };

            RunGreenPathTests(rule, validExamples);
            ParseRuleExpectFailure(rule, invalidExamples);
            ParseRuleExpectException(rule, exceptionExamples, e => e.Rule.Tag == Tags.EscapeSpecification || e.Rule.Tag == Tags.HexString);
        }

        [TestMethod]
        public void EscapeTest()
        {
            var rule = CreateEscapeRule(letterEnumeration: "abtrvfen\\", abortOnCriticalFailue: true);
            var validExamples = new string[] { "\\a", "\\b", "\\t", "\\r", "\\v", "\\f", "\\e", "\\n", "\\\\" };
            var invalidExamples = new string[] { " \\a", "foo", "", "&2" };
            var exceptionExamples = new string[] { "\\", "\\U123", "\\A" };

            //RunGreenPathTests(rule, validExamples);
            //ParseRuleExpectFailure(rule, invalidExamples);
            ParseRuleExpectException(rule, exceptionExamples, (e) => e.Rule.Tag == Tags.EscapeSpecification);
        }


        [TestMethod]
        public void StringTest()
        {
            var tag = "strTest";
            var rule = CreateStringRule(tag: tag);
            var validExamples = new string[] { "\"foo\"", "'bar 123'", "`BAZ #$%\\`", "\"\"", "''", "``" };
            var invalidExamples = new string[] { "foo\"", "bar'", "baz`", "" };
            var exceptionExamples = new string[] { "'", "\"foo", "'bar", "`baz", "\"\\", "`", "\"this is a very long text well above 20 \\Q characters I think" };

            RunGreenPathTests(rule, validExamples, (result, idx) => result.Nodes[0].Rule.Tag.IndexOf(tag) == 0);
            ParseRuleExpectFailure(rule, invalidExamples);
            ParseRuleExpectException(rule, exceptionExamples, e => e.Rule.Tag == Tags.CloseString || e.Rule.Tag == Tags.StringCharacters);
        }


        [TestMethod]
        public void StringWithEscapeCharactersTest()
        {
            var description = "is string";
            var escapeCharacters = "\\";
            var escapeRule = CreateEscapeRule(letterEnumeration: "abtrvfen\\\"", escapeCharacters: escapeCharacters);
            var rule = CreateStringRule(escapeRule, escapeCharacters, tag: description);

            var validExamples = new string[] { "\"foo\"", "'bar 123'", "`BAZ #$%\\\\`", "\"\"", "''", "``", "'\\n'", "'\\t hello world'" };
            var invalidExamples = new string[] { "foo\"", "bar'", "baz`", "" };
            var exceptionExamples = new string[] { "\"\\Q characters I think 123456790123456789", "'", "\"foo", "'bar", "`baz", "\"\\", "`", "'\\y boo'" };

            foreach (var text in validExamples)
            {
                var result = rule.Parse(text);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.CharactersRead == text.Length);
                Assert.IsTrue(result.Nodes[0].Rule.Tag.IndexOf(description) == 0);
            }

            foreach (var text in invalidExamples)
            {
                Assert.IsFalse(rule.Parse(text).IsSuccess);
            }

            foreach (var text in exceptionExamples)
            {
                try
                {
                    rule.Parse(text);
                    Assert.Fail();
                }
                catch (ParseException e)
                {
                    Debug.WriteLine(e.Message);
                    Assert.IsTrue(e.Rule.Tag == Tags.CloseString || e.Rule.Tag == Tags.StringCharacters || e.Rule.Tag == Tags.EscapeSpecification);
                }
            }
        }

        [TestMethod]
        public void BooleanTest()
        {
            var rule = CreateBooleanRule();
            var valueMap = CreateValueMap();
            var greenTests = new (string text, bool expectedValue)[] {
                ("true", true), ("True", true), ("TRUE", true), ("false", false), ("False", false), ("FALSE", false)
            };

            var redTests = new string[] { "tru", "fLSE", "" };

            foreach (var (text, expectedValue) in greenTests)
            {
                var result = rule.Parse(text);
                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.Map<bool>(text, valueMap) == expectedValue);
            }

            foreach (var redTest in redTests)
            {
                Assert.IsFalse(rule.Parse(redTest).IsSuccess);
            }
        }

        [TestMethod]
        public void WhiteSpaceTest()
        {
            var testData = new string[]
            {
                "foo", // 0
                "foobar", // 1
                "foo bar", // 2
                "foo \n bar", // 3
                "\tfoo \n bar xxx", // 4
                "\t \nfoo \n bar xxx", // 5
                "fooxbar", // 6
                "xxxfooxxxbar", // 7
                "xxx foo xxx bar", // 8
            };

            var whiteSpaceX = new RepeatRule()
            {
                Tag = "repeat x",
                WhiteSpaceRule = null,
                Visibility = NodeVisiblity.Hidden,
                Min = -1,
                Max = -1,
                Subrule = new LiteralRule() { Tag = "x", Characters = "x" }
            };

            var whiteSpaceXXX = new RepeatRule()
            {
                Tag = "repeat xxx",
                WhiteSpaceRule = null,
                Visibility = NodeVisiblity.Hidden,
                Min = -1,
                Max = -1,
                Subrule = new LiteralRule() { Tag = "xxx", Characters = "xxx" }
            };

            var combinedWhiteSpaceXXX = new RepeatRule()
            {
                Tag = "repeat xxx",
                WhiteSpaceRule = null,
                Visibility = NodeVisiblity.Hidden,
                Min = -1,
                Max = -1,
                Subrule = new OrRule()
                {
                    Subrules = new IRule[] {
                        new LiteralRule() { Tag = "xxx", Characters = "xxx" },
                        DefaultWhitespace
                    }
                }
            };

            var expectedInputOutput = new (IRule rule, bool[] expectedResult)[]
            {
                (null, new bool[] { false, true, false, false, false, false, false, false, false } ),
                (DefaultWhitespace, new bool[] { false, true, true, true, true, true, false, false, false } ),
                (whiteSpaceX, new bool[]   { false, true, false, false, false, false, true, true, false } ),
                (whiteSpaceXXX, new bool[] { false, true, false, false, false, false, false, true, false } ),
                (combinedWhiteSpaceXXX, new bool[] { false, true, true, true, true, true, false, true, true } ),
            };

            foreach (var (rule, expectedResult) in expectedInputOutput)
            {
                RunTests(CreateSequence(rule), testData, (idx) => expectedResult[idx]);
            }

            static SequenceRule CreateSequence(IRule whiteSpaceRule)
            {
                return new SequenceRule()
                {
                    Tag = "test sequence",
                    //WhiteSpaceCharacters = whiteSpaceCharacters,
                    WhiteSpaceRule = whiteSpaceRule,
                    Subrules = new IRule[]
                {
                    new LiteralRule() {
                        Tag = "foo",
                        Characters = "foo"
                    },

                    new LiteralRule() {
                        Tag = "bar",
                        Characters = "bar"
                    },
                }
                };
            }
        }
    }
}

/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core;
using gg.ast.common;
using gg.ast.util;

using static gg.ast.examples.json.JsonRules;
using static gg.ast.tests.TestUtil;

namespace gg.ast.examples.test.json
{
    [TestClass]
    public class JsonRulesTests
    {
        /// <summary>
        /// Test parsing the basic json types: number, string, boolean and null
        /// </summary>
        [TestMethod]
        public void ValueTest()
        {
            var valueMap = CreateValueMap();
            var rule = CreateValueRule();

            var validExamples = new string[] { "1234", "\"str\"", "false", "null" };
            var expectedValues = new object[] { 1234.0, "str", false, null };
            var expectedTags = new object[] { Tags.Number, Tags.String, Tags.Boolean, Tags.Null };

            RunGreenPathTests(rule, validExamples, (result, idx) =>
            {
                var token = result.Nodes[0];
                var value = valueMap.Map(token.Tag, validExamples[idx], token);

                return token.Tag == Tags.Null
                    ? value == null
                    : expectedValues[idx].Equals(value);
            });
        }

        /// <summary>
        /// Parse a json list of values
        /// </summary>
        [TestMethod]
        public void ValueListGreenPathTest()
        {
            var rule = CreateValueListRule();
            var validExamples = new string[] {
                "1234",
                "1234, true",
                "1234, true, \"str\"",
                // no separator after number so only one element should be returned
                "1234 true, \"str\"",
                // no separator after boolean so only two elements should be returned
                "1234, true \"str\""
            };
            var expectedTags = new string[][]
            {
                new string[] { Tags.Number },
                new string[] { Tags.Number, Tags.Boolean },
                new string[] { Tags.Number, Tags.Boolean, Tags.String },
                new string[] { Tags.Number },
                new string[] { Tags.Number, Tags.Boolean },
            };

            RunGreenPathTests(rule, validExamples, (result, idx) =>
            {
                var token = result.Nodes[0];

                if (token.Tag != Tags.ValueList)
                {
                    return false;
                }

                var tags = expectedTags[idx];
                for (var i = 0; i < tags.Length; i++)
                {
                    if (tags[i] != token[i].Tag)
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        /// <summary>
        /// Parse a json list of values which should fail or throw an exception
        /// </summary>
        [TestMethod]
        public void ValueListRedPathTest()
        {
            var exceptions =
                new (string text, string tag)[] {
                    // no follow up after separator
                    ("123,", Tags.Value),
                    // invalid value after comma
                    ("123, fail", Tags.Value),
                };

            var failures =
                new string[] {
                    // no data
                    ""
                };

            RunRedPathTests(CreateValueListRule(), failures, exceptions);
        }

        /// <summary>
        /// Parse valid examples of an array
        /// </summary>
        [TestMethod]
        public void ArrayTest()
        {
            var validExamples = new (string text, int childCount, string[] tags)[] {
                ("[]", 0, null),
                ("[ 1234 ]", 1, new string[] { Tags.Number }),
                ("[1234,true]", 2, new string[] { Tags.Number, Tags.Boolean }),
                ("[ 1234,true , [\"foo\", false], 0.2]", 4, new string[] { Tags.Number, Tags.Boolean, Tags.Array, Tags.Number })
            };

            var rule = CreateArrayRule();

            foreach (var (text, childCount, tags) in validExamples)
            {
                var result = rule.Parse(text);
                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.Nodes[0].Tag == Tags.Array);
                Assert.IsTrue(result.Nodes[0].Children.Count == childCount);

                for (var i = 0; i < childCount; i++)
                {
                    Assert.IsTrue(result.Nodes[0][i].Tag == tags[i]);
                }
            }
        }

        /// <summary>
        /// Test a json property rule limited to values of string, number, boolean or null
        /// </summary>
        [TestMethod]
        public void BasicPropertyTest()
        {
            var rule = CreatePropertyRule();
            var samples = CreateBasicPropertyTestSamples();
            var invalidSamples = CreatePropertyInvalidSamples();
            var (text, exceptionTags) = CreatePropertyExceptionSamples();

            RunGreenPathTests(rule, samples.text, (result, idx) =>
            {
                var token = result.Nodes[0];
                return token.Tag == Tags.Property
                        && token[0].Tag == Tags.PropertyName
                        && token[1].Tag == samples.tags[idx];
            });

            RunRedPathTests(rule, invalidSamples, text, exceptionTags);
        }

        /// <summary>
        /// Test a json property rule with all allowed values
        /// </summary>
        [TestMethod]
        public void CorePropertyTest()
        {
            // create a parse property rule which can handle the basic properties (str, number, bool, null)
            // as well a objects and arrays
            var rule = CreatePropertyRule(propertyValueParser: CreateCoreRules().valueRule);
            var (text, tags) = CreateBasicPropertyTestSamples();
            var extendedSamples = CreateExtendPropertyTestSamples();
            var invalidSamples = CreatePropertyInvalidSamples();
            var exceptionSamples = CreatePropertyExceptionSamples();

            RunGreenPathTests(rule, text, (result, idx) =>
            {
                var token = result.Nodes[0];
                return token.Tag == Tags.Property
                        && token[0].Tag == Tags.PropertyName
                        && token[1].Tag == tags[idx];
            });

            RunGreenPathTests(rule, extendedSamples.text, (result, idx) =>
            {
                var token = result.Nodes[0];
                return token.Tag == Tags.Property
                        && token[0].Tag == Tags.PropertyName
                        && token[1].Tag == extendedSamples.tags[idx];
            });

            RunRedPathTests(rule, invalidSamples, exceptionSamples.text, exceptionSamples.exceptionTags);
        }

        /// <summary>
        /// Test a list of properties (with basic values)
        /// </summary>
        [TestMethod]
        public void PropertyListTest()
        {
            var rule = CreatePropertyListRule();

            var testData = new (string text, string[] tags)[]
            {
                ("\"foo\": 1234", new string[] { Tags.Number }),
                ("\"foo\": 1234, \"bar\": \"xxx\", \"qaz\": true", new string[] { Tags.Number, Tags.String, Tags.Boolean }),
            };

            RunGreenPathTests(rule, testData.Select(data => data.text), (result, idx) =>
            {

                return result.IsSuccess
                    && result.CharactersRead == testData[idx].text.Length
                    && result.Nodes[0].Children.Count == testData[idx].tags.Length
                    && result.Nodes[0].Tag == Tags.PropertyList
                    && result.Nodes[0].Children.AllIndexed((child, childIndex) =>
                        child.Tag == Tags.Property
                        && child[0].Tag == Tags.PropertyName
                        && child[1].Tag == testData[idx].tags[childIndex]
                        );
            });
        }

        /// <summary>
        /// Tests a valid json string with various escape characters
        /// </summary>
        [TestMethod]
        public void JsonStringTest()
        {
            var rule = ExtendedStringRules.CreateStringRule();
            var validStrings = new string[] { "\"\"", "\"foo\"", "\"\\\"foo\\\"\"", "\"uni\\u1234\"", "\"uni\\u123a\"" };
            var invalidStrings = new string[] { "", " \"str\"" };
            var exceptionExamples = new (string, string)[] {
                ("\"foo", TypeRules.Tags.CloseString),
                ("\"foo\\\"", TypeRules.Tags.CloseString),
                ("\"uni\\Y1234\"", Tags.EscapeCharacter)
            };

            RunGreenPathTests(rule, validStrings, (result, idx) =>
            {
                return result.IsSuccess
                    && result.CharactersRead == validStrings[idx].Length
                    && result.Nodes[0].GetTag() == Tags.String;
            });

            RunRedPathTests(rule, invalidStrings, exceptionExamples);
        }

        /// <summary>
        /// Test various full blown json objects
        /// </summary>
        [TestMethod]
        public void ObjectTest()
        {
            var rule = CreateObjectRule();
            var valueMap = CreateValueMap();

            var testParameters = new (string text, int childCount, string[] propertyValueTags, Action<string, ParseResult> additionalTest)[]
            {
                ("{}", 0, null, null),
                ("{ \"foo\": 1234 }", 1, new string[] { Tags.Number }, null),
                ("{ \"foo\": 1, \"bar\": true }", 2, new string[] { Tags.Number, Tags.Boolean }, null),
                ("{ \"foo\": 1, \"bar\": { \"baz\": 42 } }", 2, new string[] { Tags.Number, Tags.Object }, (text, r) =>
                {
                    var jsonObj = valueMap.Map<Dictionary<string, object>>(text, r.Nodes[0]);

                    Assert.IsNotNull(jsonObj);
                    Assert.IsTrue(jsonObj.Count == 2);
                    Assert.IsTrue((double)jsonObj["foo"] == 1);

                    var innerObj = (Dictionary<string, object>)jsonObj["bar"];

                    Assert.IsNotNull(innerObj);
                    Assert.IsTrue(innerObj.Count == 1);
                    Assert.IsTrue((double)innerObj["baz"] == 42);
                }),
                ("{\"foo\":[1,2,3,4]}", 1, new string[] { Tags.Array }, (text, r) =>
                {
                    var jsonObj = valueMap.Map<Dictionary<string, object>>(text, r.Nodes[0]);
                    var innerArray = ((object[])jsonObj["foo"]).Cast<double>().ToArray();

                    for (var i = 0; i < innerArray.Length; i++)
                    {
                        Assert.IsTrue(innerArray[i] == i + 1);
                    }
                }),
                ("{\"foo\": \"\\\"bar\\\"\"}", 1, new string[] { Tags.String }, null),
                ("{\"foo\": [], \"bar\": \"r/science\"}", 2, new string[] { Tags.Array, Tags.String }, null)
            };

            foreach (var (text, childCount, propertyValueTags, additionalTest) in testParameters)
            {
                var result = rule.Parse(text);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.Nodes[0].Tag == Tags.Object);
                Assert.IsTrue(result.Nodes[0].Children.Count == childCount);

                for (var i = 0; i < childCount; i++)
                {
                    Assert.IsTrue(result.Nodes[0][i].Tag == Tags.Property);
                    Assert.IsTrue(result.Nodes[0][i][0].Tag == Tags.PropertyName);
                    Assert.IsTrue(result.Nodes[0][i][1].Tag == propertyValueTags[i]);
                }

                additionalTest?.Invoke(text, result);
            }
        }

        /// <summary>
        /// Integration test against the donut data set.
        /// </summary>
        [TestMethod]
        public void DonutsFileTest()
        {
            var (_, nodes) = ReadJsonFile("json/donuts.json");

            var data = nodes[0];

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Rule.Tag == Tags.Object);
            Assert.IsTrue(data[0].Rule.Tag == Tags.Property);
            Assert.IsTrue(data[0][0].Rule.Tag == Tags.PropertyName);
            Assert.IsTrue(data[0][1].Rule.Tag == Tags.Object);

            //data.ToString((s) => System.Diagnostics.Debug.Write(s), text, 0, 4, 120, 6);
        }

        /// <summary>
        /// Integration test against the r/science data set.
        /// </summary>
        [TestMethod]
        public void ScienceFileTest()
        {

            var (_, nodes) = ReadJsonFile("json/science.json");

            var data = nodes[0];

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Rule.Tag == Tags.Object);
            Assert.IsTrue(data[0].Rule.Tag == Tags.Property);
            Assert.IsTrue(data[0][0].Rule.Tag == Tags.PropertyName);
            Assert.IsTrue(data[0][1].Rule.Tag == Tags.String);

            //data.ToString((s) => System.Diagnostics.Debug.Write(s), text, 0, 4, 120, 6);

            //var valueMap = CreateValueMap();
            //valueMap.Map<object>(text, data);
        }

        #region Private methods

        private static (string[] text, string[] tags) CreateBasicPropertyTestSamples()
        {
            return (
                new string[] {
                    "\"number\": 1234",
                    "\"string\": \"foo\"",
                    "\"bool\": true",
                    "\"null\": null",
                    // leading with a space should work due to skip whitespace
                    " \"string\": \"foo\"",
                    // empty key is still valid
                    "\"\": 1234"
                },
                new string[]
                {
                    Tags.Number,
                    Tags.String,
                    Tags.Boolean,
                    Tags.Null,
                    Tags.String,
                    Tags.Number,
                }
            );
        }

        private static (string[] text, string[] tags) CreateExtendPropertyTestSamples()
        {
            return (
                new string[] {
                    "\"array\"  : [ 1, 2, 3 ] ",
                    "\"empty_array\":[ 1, 2, 3 ]",
                },
                new string[]
                {
                    Tags.Array,
                    Tags.Array,
                }
            );
        }

        private static string[] CreatePropertyInvalidSamples()
        {
            return new string[] {
                // no opening "
                "number\": 1234", 
                // empty
                "",
            };
        }

        private static (string[] text, string[] exceptionTags) CreatePropertyExceptionSamples()
        {
            var exceptionExamples = new string[] {
                // no closing " of property name
                "\"number: 1234",
                // no :
                "\"number\" 1234",
                // missing property
                "\"number\" : ",
                // invalid property
                "\"number\" :  ...",
            };

            var expectedExceptionTags = new string[] {
                TypeRules.Tags.CloseString,
                Tags.PropertySeparator,
                Tags.Value,
                Tags.Value
            };

            return (exceptionExamples, expectedExceptionTags);
        }

        #endregion
    }
}

/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core;

namespace gg.ast.tests
{
    public static class TestUtil
    {
        public static void RunTests(IRule rule, IEnumerable<string> samples, Func<int, bool> expectedResultFunction = null, Func<ParseResult, int, bool> testFunction = null)
        {
            for (var i = 0; i < samples.Count(); i++)
            {
                var text = samples.ElementAt(i);

                try
                {
                    var result = rule.Parse(text);
                    var expectedResult = expectedResultFunction == null || expectedResultFunction(i);

                    Assert.IsTrue(result.IsSuccess == expectedResult);

                    testFunction?.Invoke(result, i);
                }
                catch (ParseException e)
                {
                    Debug.WriteLine(e.Message);
                    Assert.Fail();
                }
            }
        }

        public static void RunGreenPathTests(IRule rule, IEnumerable<string> samples, Func<ParseResult, int, bool> testFunction = null)
        {
            for (var i = 0; i < samples.Count(); i++)
            {
                var text = samples.ElementAt(i);

                try
                {
                    var result = rule.Parse(text);

                    Assert.IsTrue(result.IsSuccess);

                    if (testFunction == null)
                    {
                        Assert.IsTrue(result.CharactersRead == text.Length);
                    }
                    else 
                    {
                        Assert.IsTrue(testFunction(result, i));
                    }
                }
                catch (ParseException e)
                {
                    Debug.WriteLine(e.Message);
                    Assert.Fail();
                }
            }
        }

        public static void ParseRuleExpectFailure(IRule rule, IEnumerable<string> samples)
        {
            foreach (var text in samples)
            {
                try
                {
                    var result = rule.Parse(text);
                    Assert.IsFalse(result.IsSuccess);
                }
                catch (ParseException e)
                {
                    Debug.WriteLine(e.Message);
                    Assert.Fail();

                }
            }
        }

        public static void ParseRuleExpectException(IRule rule, IEnumerable<string> samples, Func<ParseException, bool> isExpectedException)
        {
            foreach (var text in samples)
            {
                try
                {
                    rule.Parse(text);
                    Assert.Fail();
                }
                catch (ParseException e)
                {
                    Assert.IsTrue(isExpectedException(e));
                    Debug.WriteLine(e.Message);
                }
            }
        }

        public static void ParseRuleExpectException(
            IRule rule, 
            IEnumerable<string> samples, 
            Func<ParseException, int, bool> isExpectedException)
        {
            for (var i = 0; i < samples.Count(); i++)
            {
                var text = samples.ElementAt(i);

                try
                {
                    var result = rule.Parse(text);
                    result.Nodes[0].ToString((s) => Debug.Write(s), text);
                    Assert.Fail();
                }
                catch (ParseException e)
                {
                    Debug.WriteLine(e.Message);
                    Assert.IsTrue(isExpectedException(e, i));
                }
            }
        }

        public static void RunRedPathTests(
            IRule rule, 
            string[] invalidExamples = null, 
            (string text, string tag)[] exceptionExamples = null)
        {
            if (invalidExamples != null)
            {
                ParseRuleExpectFailure(rule, invalidExamples);
            }

            if (exceptionExamples != null)
            {
                ParseRuleExpectException(rule, exceptionExamples.Select(sample => sample.text), (exception, idx) =>
                {
                    return exception.Rule.Tag == exceptionExamples[idx].tag;
                });
            }
        }

        public static void RunRedPathTests(IRule rule, string[] invalidExamples, string[] exceptionExamples, string[] expectedExceptionTags)
        {

            ParseRuleExpectFailure(rule, invalidExamples);
            ParseRuleExpectException(rule, exceptionExamples, (exception, idx) =>
            {
                return exception.Rule.Tag == expectedExceptionTags[idx];
            });
        }
    }
}

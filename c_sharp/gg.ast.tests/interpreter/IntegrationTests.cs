/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Linq;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core;
using gg.ast.core.rules;

using gg.ast.interpreter;

namespace gg.ast.tests.interpreter
{
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void HelloIntepreterWorldGreenPath()
        {
            var inputText = "hello interpreter world";
            var helloWorldTag = "helloWorld";
            var helloWorldScript = $"{helloWorldTag}= \"{inputText}\";";
            var rule = new ParserFactory().Parse(helloWorldScript);

            var result = rule.Parse(inputText);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Nodes[0].Tag == helloWorldTag);
            Assert.IsTrue(result.Nodes[0].Length == inputText.Length);
        }

        [TestMethod]
        public void HelloReferenceGreenPath()
        {
            var inputText = "hello ref world";

            var interpreter = new ParserFactory().ParseFile("data/helloReference.txt");
            var result = interpreter.Parse(inputText);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Nodes[0].Tag == "helloWorld");
            Assert.IsTrue(result.Nodes[0].Length == inputText.Length);
        }

        [TestMethod]
        public void HelloSequenceGreenPath()
        {
            var inputText = new string[]
            {
                "hello sequence world",
                " hello  sequenceworld",
                "hellosequenceworld",
                "hello \n sequence \n \t \r world"
            };

            var interpreter = new ParserFactory().ParseFile("data/helloSequence.txt");

            foreach (var input in inputText)
            {
                var result = interpreter.Parse(input);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.Nodes[0].Tag == "helloSequenceWorld");
                Assert.IsTrue(result.Nodes[0].Length == input.Length);
                Assert.IsTrue(result.Nodes[0].Children.Count == 3);
                Assert.IsTrue(result.Nodes[0].Children.All(child => child.Rule is LiteralRule));
            }
        }

        [TestMethod]
        public void HelloSequenceNoSeparatorTest()
        {
            var testData = new (string text, bool isSuccess)[]
            {
                ("hello sequence world", false),
                ("hellosequenceworld", true),
            };

            var interpreter = new ParserFactory().ParseFile("data/helloSequenceNoSeparator.txt");

            foreach (var (text, isSuccess) in testData)
            {
                var result = interpreter.Parse(text);

                Assert.IsTrue(result.IsSuccess == isSuccess);

                if (result.IsSuccess)
                {
                    Assert.IsTrue(result.Nodes[0].Tag == "helloSequenceWorld");
                    Assert.IsTrue(result.Nodes[0].Length == text.Length);
                    Assert.IsTrue(result.Nodes[0].Children.Count == 3);
                    Assert.IsTrue(result.Nodes[0].Children.All(child => child.Rule is LiteralRule));
                }
            }
        }

        [TestMethod]
        public void HelloSequenceWithCommentsGreenPath()
        {
            var inputText = new string[]
            {
                "hello /* comment */ sequence world",
                " hello  sequenceworld // comment",
                "// comment \r\n hellosequence// ... \nworld",
                "/* ! */ hello /**/ \n sequence \n \t \r world /* . */"
            };

            var interpreter = new ParserFactory().ParseFile("data/helloSequenceWithComments.txt");

            foreach (var input in inputText)
            {
                var result = interpreter.Parse(input);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.Nodes[0].Tag == "helloSequenceWorld");
                Assert.IsTrue(result.Nodes[0].Children.Count == 3);
                Assert.IsTrue(result.Nodes[0].Children.All(child => child.Rule is LiteralRule));
            }
        }

        [TestMethod]
        public void HelloOrGreenPath()
        {
            var inputText = new string[]
            {
                "hello",
                "or",
                "world",
            };

            var interpreter = new ParserFactory().ParseFile("data/helloOr.txt");

            foreach (var input in inputText)
            {
                var result = interpreter.Parse(input);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.Nodes[0].Tag == "helloOrWorld");
                Assert.IsTrue(result.Nodes[0].Length == input.Length);
                Assert.IsTrue(result.Nodes[0].Rule is OrRule);
                Assert.IsTrue(result.Nodes[0].Children.Count == 1);
                Assert.IsTrue(result.Nodes[0].Children.All(child => child.Rule is LiteralRule));
            }
        }

        [TestMethod]
        public void HelloRepeatGreenPath()
        {
            var inputText = new (string text, int childCount, int tokenCount)[]
            {
                // due to the way the location of the repeat in rules, _each_ 'hello' counts as 1 child
                // whereas all 'repeat' and 'world' count as 1 child
                // note that for the sum of all children we need to include the whitespace in between
                // repeats
                ("hello world world", 2, 3),
                ("hello hello world  world", 3, 4),
                ("hello repeat world world", 3, 4),
                ("hello hello repeat world  world", 4, 5),
                ("hello repeat  repeat    \nrepeatrepeat worldworld", 6, 7),
            };

            TestRepeatRule("data/helloRepeatedSequence.txt", inputText);
        }

        [TestMethod]
        public void HelloRepeatGreenWithImplicitWhitespacePath()
        {
            // same data as HelloRepeatGreenPath except less tokens
            var inputText = new (string text, int childCount, int tokenCount)[]
            {
                ("hello world world", 2, 3),
                ("hello hello world  world", 3, 4),
                ("hello repeat world world", 3, 4),
                ("hello hello repeat world  world", 4, 5),
                ("hello repeat  repeat    \nrepeatrepeat worldworld", 6, 7),
            };

            TestRepeatRule("data/helloRepeatedSequenceWithWhitespace.txt", inputText);
        }

        [TestMethod]
        public void HelloRepeatWithModifiedWhitespaceGreenPath()
        {
            // same data as HelloRepeatGreenPath except less tokens
            var inputText = new (string text, int childCount, int tokenCount)[]
            {
                ("hello_world&worldpoint", 3, 4),
                ("hello_hello_world&&worldpoint", 4, 5),
                ("hello_repeat_world&worldpointpoint", 4, 6),
                ("hello+hello_repeat__world&&&worldpointpointpoint", 5, 8),
                ("hello_repeat__repeat__+__repeatrepeat_worldworldpoint", 7, 8),
            };

            TestRepeatRule("data/helloWsAlternative.txt", inputText);
        }

        private void TestRepeatRule(string filename, (string text, int childCount, int tokenCount)[] testData)
        { 
            var interpreter = new ParserFactory().ParseFile(filename);
            
            Debug.WriteLine(interpreter.PrintRuleTree());

            foreach (var (text, childCount, tokenCount) in testData)
            {
                var result = interpreter.Parse(text);

                Assert.IsTrue(result.IsSuccess);

                Debug.WriteLine("\n-------\n");
                result.Nodes[0].ToString((s) => Debug.Write(s), text);

                Assert.IsTrue(result.Nodes[0].Tag == "helloRepeatWorld");
                Assert.IsTrue(result.Nodes[0].Length == text.Length);
                Assert.IsTrue(result.Nodes[0].Rule is SequenceRule);

                Assert.IsTrue(result.Nodes[0].Children.Count == childCount);

                var actualTokenCount = result.Nodes[0].Children.Sum(child => child.Children == null ? 1 : child.Children.Count);
                Assert.IsTrue(tokenCount == actualTokenCount);
            }
        }
    }
}

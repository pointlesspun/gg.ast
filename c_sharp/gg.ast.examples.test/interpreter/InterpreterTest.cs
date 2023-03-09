
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.interpreter;
using gg.ast.core;
using System.Collections.Generic;
using gg.ast.util;
using gg.ast.core.rules;

using static gg.ast.util.FileCache;
using System.Diagnostics;

namespace gg.ast.examples.test.interpreter
{
    [TestClass]
    public class InterpreterTest
    {
        private static Dictionary<string, IRule> ReadInterpreterSpec()
        {
            var text = LoadTextFile("./specfiles/interpreter.spec");
            return new ParserFactory().ParseRules(text);
        }

        private static IRule CreateInterpreter()
        {
            return ReadInterpreterSpec()["interpreter"];
        }

        /// <summary>
        /// Parse all known spec files with the interpreter spec file
        /// </summary>
        [TestMethod]
        public void ParseInterpreterTest()
        {
            var testFilesDirectories = new string[]
            {
                "./introduction",
                "./specfiles"
            };
            var interpreter = CreateInterpreter();

            foreach (var directoryName in testFilesDirectories)
            {
                var specFiles = Directory.EnumerateFiles(directoryName)
                                    .Where(filename => filename.Contains(".spec"));

                foreach (var file in specFiles)
                {
                    var text = LoadTextFile(file);
                    var result = interpreter.Parse(text);

                    Assert.IsTrue(result.IsSuccess);
                }
            }
        }

        /// <summary>
        /// Parse the hello world script an test it against some input
        /// </summary>
        [TestMethod]
        public void CreateHelloWorldInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (_, _, rules) = InitializeTest("./introduction/hello_world.spec", 4);
            
            var helloWorldRule = rules["helloWorld"];

            var helloWorldText = "hello world";
            var helloWorldResult = helloWorldRule.Parse(helloWorldText);

            Assert.IsTrue(helloWorldResult.IsSuccess);
            Assert.IsTrue(helloWorldResult.CharactersRead == helloWorldText.Length);
            Assert.IsFalse(helloWorldRule.Parse("hi weld").IsSuccess);
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void CharRuleInterpreterTest()
        {
            var charRule = ReadInterpreterSpec()["charRule"];

            Assert.IsTrue(charRule.Parse("`abc`").IsSuccess);
            Assert.IsTrue(charRule.Parse("'abc'").IsSuccess);
            Assert.IsTrue(charRule.Parse("$").IsSuccess);
            Assert.IsTrue(charRule.Parse("any").IsSuccess);
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void CharSetInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (_, _, rules) = InitializeTest("./introduction/charsets.spec", 8);
            
            var anyCharacter = rules["anyCharacter"];
            var aToZSet = rules["aToZSet"];
            var wideSet = rules["wideSet"];
            var abcEnumeration = rules["abcEnumeration"];
            var notABCEnumeration = rules["notABCEnumeration"];

            for (var i = 0; i < 255; i++)
            {
                Assert.IsTrue(anyCharacter.Parse("" + (char)i).IsSuccess);
            }

            Assert.IsFalse(anyCharacter.Parse("").IsSuccess);

            Assert.IsTrue(aToZSet.Parse("a").IsSuccess);
            Assert.IsTrue(aToZSet.Parse("h").IsSuccess);
            Assert.IsTrue(aToZSet.Parse("z").IsSuccess);

            Assert.IsFalse(aToZSet.Parse("A").IsSuccess);
            Assert.IsFalse(aToZSet.Parse("Z").IsSuccess);
            Assert.IsFalse(aToZSet.Parse("0").IsSuccess);
            Assert.IsFalse(aToZSet.Parse("9").IsSuccess);

            Assert.IsTrue(wideSet.Parse("a").IsSuccess);
            Assert.IsTrue(wideSet.Parse("h").IsSuccess);
            Assert.IsTrue(wideSet.Parse("z").IsSuccess);
            Assert.IsTrue(wideSet.Parse("A").IsSuccess);
            Assert.IsTrue(wideSet.Parse("Z").IsSuccess);
            Assert.IsTrue(wideSet.Parse("0").IsSuccess);
            Assert.IsTrue(wideSet.Parse("9").IsSuccess);

            Assert.IsFalse(wideSet.Parse("_").IsSuccess);

            Assert.IsTrue(abcEnumeration.Parse("a").IsSuccess);
            Assert.IsTrue(abcEnumeration.Parse("b").IsSuccess);
            Assert.IsTrue(abcEnumeration.Parse("c").IsSuccess);

            Assert.IsFalse(abcEnumeration.Parse("d").IsSuccess);
            Assert.IsFalse(abcEnumeration.Parse("A").IsSuccess);
            Assert.IsFalse(abcEnumeration.Parse("C").IsSuccess);

            Assert.IsFalse(notABCEnumeration.Parse("a").IsSuccess);
            Assert.IsFalse(notABCEnumeration.Parse("b").IsSuccess);
            Assert.IsFalse(notABCEnumeration.Parse("c").IsSuccess);

            Assert.IsTrue(notABCEnumeration.Parse("d").IsSuccess);
            Assert.IsTrue(notABCEnumeration.Parse("A").IsSuccess);
            Assert.IsTrue(notABCEnumeration.Parse("C").IsSuccess);
        }


        [TestMethod]
        public void UnaryRepeatRuleInterpreterTest()
        {
            var charRule = ReadInterpreterSpec()["unaryValue"];
            var config = new InterpreterConfig();
            var valueMap = InterpreterValueMap.CreateValueMap(config, new List<ReferenceRule>());
            var testData = new (string text, string tag, int min, int max, bool acceptsWhitespace)[]
            {
                ("ref[ 3 ]", "repeat.ws", 3, 3, true),
                ("ref[]", "repeat.ws", -1, -1, true),
                ("ref[ .. 1]", "repeat.ws", -1, 1, true),
                ("ref [1..]", "repeat.ws", 1, -1, true),
                ("ref  [1..2]", "repeat.ws", 1, 2, true),
                ("ref<>", "repeat.noWs", -1, -1, false),
                ("ref <..1>", "repeat.noWs", -1, 1, false),
                ("ref<1..2>", "repeat.noWs", 1, 2, false),
                ("ref  <3>", "repeat.noWs", 3, 3, false),
                ("ref?", "repeat.noWs", -1, 1, false),
                ("ref +", "repeat.noWs", 1, -1, false),
                ("ref  *", "repeat.noWs",-1, -1, false)
            };

            testData.ForEachIndexed((tuple, idx) =>
            {
                var (str, tag, min, max, acceptsWhitespace) = tuple;
                var result = charRule.Parse(str);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.CharactersRead == str.Length);

                var node = result.Nodes[0];

                Assert.IsTrue(node.Children.Count == 2);
                Assert.IsTrue(node[0].Tag == "ruleReference");
                Assert.IsTrue(node[1].Tag == tag);

                var repeatRule = valueMap.Map<RepeatRule>(str, node);

                Assert.IsTrue(repeatRule.Min == min);
                Assert.IsTrue(repeatRule.Max == max);

                Assert.IsTrue((repeatRule.WhiteSpaceRule != null) == acceptsWhitespace);
            });
        }

        [TestMethod]
        public void RuleRepeatRuleInterpreterTest()
        {
            var charRule = ReadInterpreterSpec()["rule"];
            var testData = new string[]
            {
                "a=ref[];", "a =ref[ .. 1];", "a  = ref [1..];", "a= ref  [1..2];", " a  = ref[ 3 ];",
                "a = ref<>;", "a=ref <..1>;", "  a =ref<1..2>;", "a =ref  <3>;",
                "a = ref?;", "a = ref +;", "a=ref  *;"
            };

            testData.ForEachIndexed((str, _) =>
            {
                var result = charRule.Parse(str);
                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.CharactersRead == str.Length);
                Assert.IsTrue(result.Nodes[0].Children.Count == 2);
                Assert.IsTrue(result.Nodes[0][0].Tag == "identifier");
                Assert.IsTrue(result.Nodes[0][1].Tag == "ruleValue");
            });
        }


        /// <summary>
        /// </summary>
        [TestMethod]
        public void RepeatInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (_, _, rules) = InitializeTest("./introduction/repeat.spec", 17);

            // do some spot checks
            var text = "hello world";
            var invalidText = "hi weld";

            var twoNoWs = rules["explicitNoWs.two"];
            var result = twoNoWs.Parse(text + text + text);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == text.Length * 2);

            Assert.IsFalse(twoNoWs.Parse(invalidText).IsSuccess);

            var zeroOrOne = rules["zeroOrOne"];

            result = zeroOrOne.Parse(text + text + text);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == text.Length);

            result = zeroOrOne.Parse(invalidText);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == 0);
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void OrInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (_, _, rules) = InitializeTest("./introduction/or.spec", 9);

            var correctResult = rules["correctResult"];

            var testData = new string[] { "1 * 2", "3", "4 - 5" };

            foreach (var text in testData)
            {
                var result = correctResult.Parse(text);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.CharactersRead == text.Length);
                Assert.IsTrue(result.Nodes[0].Tag == "operation" || result.Nodes[0].Tag == "value");
            }
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void SequenceInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (_, _, rules) = InitializeTest("./introduction/sequences.spec", 7);
            
            var helloRule = rules["helloSpaciousWorld"];
            var result = helloRule.Parse("hello   world");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Nodes[0].Tag == "helloSpaciousWorld");            
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void NotInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (_, _, rules) = InitializeTest("./introduction/not.spec", 11);

            var altComment = rules["altComment"];
            var text = "/* this is a comment */";

            var result = altComment.Parse(text);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.CharactersRead == text.Length);
            Assert.IsTrue(result.Nodes[0].Tag == "altComment");
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void NumbersInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (_, _, rules) = InitializeTest("./specfiles/numbers.spec", 11);

            var number = rules["number"];
            var numberTexts = new string[] { "1", "-1", "0.1", "-69.42", "123e456", "12.32E-129" };

            foreach ( var text in numberTexts )
            {
                var result = number.Parse(text);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.CharactersRead == text.Length);
                Assert.IsTrue(result.Nodes[0].Tag == "number");
            }            
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void TypesInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (_, _, rules) = InitializeTest("./specfiles/types.spec", 18);

            var number = rules["typeValue"];
            var testData = new (string text, string tag)[] { 
                ("12.32E-129", "number"),
                ("true", "boolean"),
                ("null", "null"),
                ("\"string\"", "string")
            };

            foreach (var (text, tag) in testData)
            {
                var result = number.Parse(text);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.CharactersRead == text.Length);
                Assert.IsTrue(result.Nodes[0].Tag == tag);
            }
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void JsonInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (_, factory, rules) = InitializeTest("./specfiles/json.spec", 25);

            var document = rules["document"];
            var donuts = LoadTextFile("./json/donuts.json");
            var science = LoadTextFile("./json/science.json");

            var donutsResult = document.Parse(donuts);
            var scienceResult = document.Parse(science);

            // spot check the first item in the document 
            var donutNode = donutsResult.Nodes[0][0][0][1][0][1][0][1][1];
            Assert.IsTrue(donutNode.Tag == "string");

            var donutValue = factory.ValueMap.Map<string>(donuts, donutNode);
            Assert.IsTrue(donutValue == "donut");

            // donutsResult.Nodes[0].ToString((s) => Debug.Write(s), donuts);
            // scienceResult.Nodes[0].ToString((s) => Debug.Write(s), science);
        }


        /// <summary>
        /// Inception
        /// </summary>
        [TestMethod]
        public void InterpreterInterpreterTest()
        {
            // load the spec file, test if the loaded rules matches the count and return the result
            var (interpreter, factory, rules) = InitializeTest("./specfiles/interpreter.spec", 57);

            var specInterpreter = rules["interpreter"];

            var interpreterSpec = LoadTextFile("./specfiles/interpreter.spec");
            var inceptionInterpreter = new ParserFactory()
                                .ParseRules(specInterpreter, interpreterSpec)["interpreter"];

            var specRuleSet = specInterpreter.CollectRules();
            var interpreterRuleSet = interpreter.CollectRules();
            var inceptionInterpreterRuleSet = inceptionInterpreter.CollectRules();

            Assert.IsTrue(specRuleSet.Count == interpreterRuleSet.Count);
            Assert.IsTrue(interpreterRuleSet.Count == inceptionInterpreterRuleSet.Count);

            var jsonSpec = LoadTextFile("./specfiles/json.spec");
            var document = new ParserFactory()
                                .ParseRules(inceptionInterpreter, jsonSpec)["document"];

            var donuts = LoadTextFile("./json/donuts.json");
            var science = LoadTextFile("./json/science.json");

            var donutsResult = document.Parse(donuts);
            var scienceResult = document.Parse(science);

            // spot check the first item in the document 
            var donutNode = donutsResult.Nodes[0][0][0][1][0][1][0][1][1];
            Assert.IsTrue(donutNode.Tag == "string");

            var donutValue = factory.ValueMap.Map<string>(donuts, donutNode);
            Assert.IsTrue(donutValue == "donut");

            // donutsResult.Nodes[0].ToString((s) => Debug.Write(s), donuts);
            // scienceResult.Nodes[0].ToString((s) => Debug.Write(s), science);
        }

        // ... not really a test, just checking if my intuition was right and the spec file
        // is more efficient
        [TestMethod]
        public void CountNodes()
        {
            var interpreterRule = InterpreterRules.CreateInterpreterRule(new InterpreterConfig());
            var interpreterSpec = CreateInterpreter();

            var interpreterRuleSet = interpreterRule.CollectRules();
            var specRuleSet = interpreterSpec.CollectRules();
        }

        private (IRule interpreter, ParserFactory factory, Dictionary<string, IRule> rules) InitializeTest(
            string filename, 
            int expectedRuleCount)
        {
            var interpreter = CreateInterpreter();
            var specFile = LoadTextFile(filename);
            var factory = new ParserFactory();
            var rules = factory.ParseRules(interpreter, specFile);

            Assert.IsTrue(rules.Count == expectedRuleCount);

            return (interpreter, factory, rules);
        }
    }
}

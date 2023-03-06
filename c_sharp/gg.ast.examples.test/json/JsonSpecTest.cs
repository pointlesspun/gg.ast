/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Diagnostics;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core;

using gg.ast.interpreter;
using gg.ast.util;

namespace gg.ast.tests.interpreter
{
    [TestClass]
    public class JsonSpecTest
    {
        [TestMethod]
        public void TypesGreenPath()
        {
            var inputText = new (string text, string expectedTag, string childTag)[]
            {
                ("\"foo \\\"bar\\\"\"", "string", null),
                ("null", "null", null),
                ("true", "boolean", null),
                ("\"foo\"", "string", null),
                ("0xD018", "number", "hex"),
                ("1234", "number", "integer"),
                ("-0.2", "number", "decimal"),
                ("0.1e123", "number", "exponent"),
            };
            var interpreter = new ParserFactory().ParseFileRules("specfiles/json.spec")["jsonValue"];

            Debug.WriteLine(interpreter.PrintRuleTree());

            foreach (var (text, tag, childTag) in inputText)
            {
                var result = interpreter.Parse(text);

                Assert.IsTrue(result.IsSuccess);

                Assert.IsTrue(result.Nodes[0].Tag == tag);
                Assert.IsTrue(result.Nodes[0].Length == text.Length);

                if (childTag != null)
                {
                    Assert.IsTrue(result.Nodes[0][0].Tag == childTag);
                }
                else
                {
                    Assert.IsTrue(result.Nodes[0].Children == null || result.Nodes[0].Children.Count == 0);
                }
            }
        }

        [TestMethod]
        public void DonutTest()
        {
            var interpreter = new ParserFactory().ParseFile("specfiles/json.spec");
            var donutText = File.ReadAllText("json/donuts.json");
            var result = interpreter.Parse(donutText);

            Assert.IsTrue(result.IsSuccess);

            // result.Nodes[0].ToString(s => Debug.Write(s), donutText);
        }

        [TestMethod]
        public void ScienceTest()
        {
            var interpreter = new ParserFactory().ParseFile("specfiles/json.spec");
            var scienceText = File.ReadAllText("json/science.json");
            var result = interpreter.Parse(scienceText);

            Assert.IsTrue(result.IsSuccess);

            var output = new MermaidOutput()
            {
                CullNotVisibleNodes = true
            };

            // Debug.Write(output.ToString(interpreter));
            
            // output.ToMDFile(interpreter, order: MermaidOutput.Order.DepthFirst);

            // result.Nodes[0].ToString(s => Debug.Write(s), donutText);
        }
    }
}
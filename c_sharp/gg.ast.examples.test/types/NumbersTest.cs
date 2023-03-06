/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core;
using gg.ast.interpreter;
using gg.ast.util;

namespace gg.ast.tests.interpreter
{
    [TestClass]
    public class NumbersTest
    {
        [TestMethod]
        public void NumbersGreenPath()
        {
            var inputText = new (string text, string expectedTag, string childTag)[]
            {
                ("0xD018", "number", "hex"),
                ("1234", "number", "integer"),
                ("-0.2", "number", "decimal"),
                ("0.1e123", "number", "exponent"),                
            };
            var interpreter = new ParserFactory().ParseFile("specfiles/numbers.spec");

            Debug.WriteLine(interpreter.PrintRuleTree());

            foreach (var (text, tag, childTag) in inputText)
            {
                var result = interpreter.Parse(text);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.Nodes[0].Tag == tag);
                Assert.IsTrue(result.Nodes[0][0].Tag == childTag);
                Assert.IsTrue(result.Nodes[0].Length == text.Length);
            }
        }
    }
}
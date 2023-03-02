/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.interpreter;

namespace gg.ast.examples.test.calculator
{
    [TestClass]
    public class CalculatorTests
    {
        [TestMethod]
        public void CalculatorGreenPath()
        {
            var inputText = new (string text, string expectedTag, string[] childTags)[]
            {
                ("1", "number", new string[] { "integer" }),
                ("1 + 2.0", "add", new string[] { "number", "number" }),
                ("-0.1 * 20 / 5", "multiply", new string[] { "number", "divide" }),
                ("(1 * 4) - (3 / 2)", "subtract", new string[] { "group", "group" }),
                ("0.1e123 + ( 4 + (5*(3))) / (2-1)", "add", new string[] { "number", "divide" }),
            };

            var interpreter = new ParserFactory().ParseFile("calculator/calculator.spec");

            // Debug.WriteLine(interpreter.PrintRuleTree());

            foreach (var (text, tag, childTags) in inputText)
            {
                var result = interpreter.Parse(text);

                Assert.IsTrue(result.IsSuccess);
                Assert.IsTrue(result.Nodes[0].Tag == tag);
                Assert.IsTrue(result.Nodes[0].Children.Count == childTags.Length);

                // Debug.WriteLine("\n-------\n");
                // result.Nodes[0].ToString((s) => Debug.Write(s), text);

                for (var i = 0; i < childTags.Length; i++)
                {
                    Assert.IsTrue(result.Nodes[0][i].Tag == childTags[i]);
                }
            }
        }
    }
}

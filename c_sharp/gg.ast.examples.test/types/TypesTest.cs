/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core;
using gg.ast.interpreter;

using static gg.ast.util.FileCache;

namespace gg.ast.examples.test
{
    [TestClass]
    public class TypesTest
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
            var interpreter = new ParserFactory().Parse(LoadTextFile("specfiles/types.spec"));

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

    }
}
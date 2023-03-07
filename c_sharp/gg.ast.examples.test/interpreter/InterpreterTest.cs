using gg.ast.core;
using gg.ast.interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace gg.ast.examples.test.interpreter
{
    [TestClass]
    public class InterpreterTest
    {
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
            var rules = new ParserFactory().ParseFileRules("./specfiles/interpreter.spec");
            var interpreter = rules["interpreter"];

            foreach (var directoryName in testFilesDirectories) 
            {
                var specFiles = Directory.EnumerateFiles(directoryName)
                                    .Where(filename => filename.IndexOf(".spec") >= 0);
               
                foreach (var file in specFiles)
                {
                    var text = File.ReadAllText(file);
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
            var rules = new ParserFactory().ParseFileRules("./specfiles/interpreter.spec");
            var interpreter = rules["interpreter"];

            var text = File.ReadAllText("./introduction/hello_world.spec");
            
            var helloWorldRule = new ParserFactory().ParseRules(interpreter, text)["helloWorld"];

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
            var interpreterRules = new ParserFactory().ParseFileRules("./specfiles/interpreter.spec");
            var charRule = interpreterRules["charRule"];

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
            var interpreter = new ParserFactory().ParseFileRules("./specfiles/interpreter.spec")["interpreter"];

            var specFile = File.ReadAllText("./introduction/charsets.spec");
            var specFileRules = new ParserFactory().ParseRules(interpreter, specFile);
            var anyCharacter = specFileRules["anyCharacter"];
            var aToZSet = specFileRules["aToZSet"];
            var wideSet = specFileRules["wideSet"];
            var abcEnumeration = specFileRules["abcEnumeration"];
            var notABCEnumeration = specFileRules["notABCEnumeration"];

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
    }
}

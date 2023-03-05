using gg.ast.core;
using gg.ast.interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

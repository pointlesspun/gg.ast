/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.common;

using static gg.ast.examples.preprocessor.Preprocessor;
using static gg.ast.tests.TestUtil;

namespace gg.ast.examples.test.preprocessor
{
    [TestClass]
    internal class PreprocessorTests
    {

        [TestMethod]
        public void PreprocessorGreenPath()
        {
            var config = new CommentsConfig();

            var examples = new (string text, string[] tags)[]
            {
                ("//doc\n", new string[] { config.Tags.SingleLineComment }),
                ("abc", new string[] { config.Tags.DocumentText }),
                ("abc// doc", new string[] { config.Tags.DocumentText, config.Tags.SingleLineComment }),
                ("abc//doc\n more text", new string[] { config.Tags.DocumentText, config.Tags.SingleLineComment, config.Tags.DocumentText }),
                ("//doc\n more text", new string[] { config.Tags.SingleLineComment, config.Tags.DocumentText }),
                ("/*doc\n*/", new string[] { config.Tags.MultiLineComment }),
                ("foo /*doc\n*/", new string[] { config.Tags.DocumentText, config.Tags.MultiLineComment }),
                ("//doc/*doc*/\n", new string[] { config.Tags.SingleLineComment }),
                ("/*doc/*doc\n*/", new string[] { config.Tags.MultiLineComment }),
                ("/*doc*///bla\ntext", new string[] { config.Tags.MultiLineComment, config.Tags.SingleLineComment, config.Tags.DocumentText }),
                ("/*doc*///bla\ntext\n //bla\n text /**/", new string[] {
                    config.Tags.MultiLineComment, config.Tags.SingleLineComment, config.Tags.DocumentText,
                        config.Tags.SingleLineComment, config.Tags.DocumentText, config.Tags.MultiLineComment
                }),
            };

            RunGreenPathTests(CreatePreprocessorRule(config), examples.Select(x => x.text), (result, idx) =>
            {
                if (result.CharactersRead == examples[idx].text.Length
                    && result.Nodes[0].Tag == config.Tags.Document
                    && result.Nodes[0].Children.Count == examples[idx].tags.Length)
                {
                    for (var i = 0; i < result.Nodes[0].Children.Count; i++)
                    {
                        if (result.Nodes[0][i].Tag != examples[idx].tags[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            });
        }

        [TestMethod]
        public void DataExampleTest()
        {
            // no real tests here, just checking if it "works"
            //Debug.Write(Preprocessor.RemoveComments("    public void showCodeSnippetFormatting()\r\n    {\r\n        // do nothing\r\n    }\r\n"));
            // Debug.Write(Preprocessor.RemoveComments(File.ReadAllText("data/baeldung_example.java")));
            //Debug.Write(Preprocessor.RemoveComments(File.ReadAllText("data/clock.cpp")));
        }
    }
}

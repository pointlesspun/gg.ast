/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.common;

using static gg.ast.common.CommentsRules;

using static gg.ast.tests.TestUtil;

namespace gg.ast.tests.common
{
    [TestClass]
    public class CommentsTests
    {
        [TestMethod]
        public void SingleLineCommentGreenPath()
        {
            var examples = new (string text, int length)[]
            {
                ("// eoln test\n ", 13),
                ("//comment", 9),

            };

            var config = new CommentsConfig();

            RunGreenPathTests(CreateSinglelineCommentRule(config), examples.Select(x => x.text), (result, idx) =>
            {
                return examples[idx].length == result.CharactersRead;
            });
        }

        [TestMethod]
        public void MultiLineCommentGreenPath()
        {
            var examples = new (string text, int length)[]
            {
                ("/* \n eoln test*/ ", 16),
                ("/*comment*/", 11),
            };

            RunGreenPathTests(CreateMultilineCommentRule(new CommentsConfig()), examples.Select(x => x.text), (result, idx) =>
            {
                return examples[idx].length == result.CharactersRead;
            });
        }

        [TestMethod]
        public void DocumentCharactersGreenPath()
        {
            var examples = new (string text, int length)[]
            {
                ("abc def/* eoln test/n */", 7),
                ("a/c", 3),
                ("ab/", 3),
                ("/bc", 3),
                ("abc", 3),
                ("abc//comment", 3),
            };

            var config = new CommentsConfig();

            RunGreenPathTests(CreateDocumentTextRule(config), examples.Select(x => x.text), (result, idx) =>
            {
                return examples[idx].length == result.CharactersRead;
            });
        }

    }
}

/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.util;

namespace gg.ast.tests.util
{
    [TestClass]
    public class AstExtensionsTest
    {
        [TestMethod]
        public void SubstringAroundTest()
        {
            var testSamples = new (string testText, int center, int range, string expectedOutcome)[] {
                ("", 0, 3, ""),
                ("a", 0, 3, "a"),
                ("foo", 1, 3, "foo"),
                ("foo", 1, 1, "foo"),
                ("foo", 1, 0, "...o..."),
                ("prefix test", 3, 2, "...refix..." ),
                ("prefix test", 3, 3, "prefix ..."),
                ("prefix test", 2, 3, "prefix..."),
                ("prefix test", 1, 3, "prefi..."),
                ("prefix test", 0, 3, "pref..."),
                ("prefix test", -1, 3, "pre..."),
                ("prefix test", 10, 2, "...est" ),
                ("prefix test", 9, 2, "...test" ),
                ("prefix test", 8, 2, "... test" ),
                ("prefix test", 7, 2, "...x tes..." ),
                ("prefix test", 6, 2, "...ix te..." ),
            };

            foreach (var (testText, center, range, expectedOutcome) in testSamples)
            {
                var outcome = testText.SubstringAround(center, range);

                Assert.IsTrue(expectedOutcome.Equals(outcome));
            }
        }

        // xxx test get cursor position against "(ref \n\"lit\" \t (another_ref, \"lit2\") )"
    }
}

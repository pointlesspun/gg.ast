/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.interpreter;
using gg.ast.util;
using System.IO;

namespace gg.ast.tests.util
{
    [TestClass]
    public class MermaidTest
    {
        /// <summary>
        /// Integration test to see if the mermaid export works (somewhat) using 
        /// depthfirst traversal.
        /// </summary>
        [TestMethod]
        public void DepthFirstTest()
        {
            // use https://mermaid-js.github.io/mermaid-live-editor to visualize
            var mermaid = new MermaidOutput()
            {
                AllowDuplicateBranches = false,
                ShowReferenceRules = true,
            };
            var mermaidRules = new ParserFactory().ParseFileRules("./util/mermaid.spec");
            var jsonRules = new ParserFactory().ParseFileRules("./util/json.spec");

            mermaid.ToChartFile(mermaidRules["mermaidChart"]);
            mermaid.ToChartFile(jsonRules["document"]);

            Assert.IsTrue(File.Exists("./mermaidChart.mmc"));
            Assert.IsTrue(File.Exists("./document.mmc"));
            Assert.IsTrue(new FileInfo("./mermaidChart.mmc").Length > 0);
            Assert.IsTrue(new FileInfo("./document.mmc").Length > 0);
        }
    }
}

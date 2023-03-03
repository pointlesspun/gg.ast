/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.interpreter;
using gg.ast.util;

namespace gg.ast.tests.util
{
    [TestClass]
    public class MermaidTest
    {
        [TestMethod]
        public void depthFirstTest()
        {
            // use https://mermaid-js.github.io/mermaid-live-editor to visualize
            var mermaid = new MermaidOutput()
            {
                AllowDuplicates = true,
                ShowReferenceRules = true,
            };
            var mermaidRules = new ParserFactory().ParseFileRules("./util/mermaid.spec");
            var jsonRules = new ParserFactory().ParseFileRules("./util/json.spec");
            var mermaidChart = mermaid.ToMermaidChart(mermaidRules["mermaidChart"]);

            mermaid.ToChart(mermaidRules["mermaidChart"]);
            mermaid.ToChart(jsonRules["document"]);
        }
    }
}

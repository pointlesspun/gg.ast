using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.core.rules;
using gg.ast.interpreter;

using static gg.ast.util.FileCache;

namespace gg.ast.tests.interpreter
{
    [TestClass]
    public class InlineAndSubstitutionTests
    {
        [TestMethod]
        public void ReferenceAbcReferralTest()
        {
            var rules = new ParserFactory().ParseRules(LoadTextFile("./data/singleInlineReference.spec"));

            var a = rules["a"] as LiteralRule;
            var b = rules["b"] as LiteralRule;
            var c = rules["c"] as SequenceRule;

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.IsNotNull(c);
            Assert.IsTrue(a.Tag == "a");
            Assert.IsTrue(b.Tag == "b");
            Assert.IsTrue(c.Subrules.Length == 2);
            Assert.IsTrue(c.Subrules[0] == a);
            Assert.IsTrue(c.Subrules[1] == b);
        }

        [TestMethod]
        public void ReferenceDefReferralTest()
        {
            var rules = new ParserFactory().ParseRules(LoadTextFile("./data/reverseInline.spec"));

            var d = rules["d"] as LiteralRule;
            var e = rules["e"] as LiteralRule;
            var f = rules["f"] as SequenceRule;

            Assert.IsNotNull(d);
            Assert.IsNotNull(e);
            Assert.IsNotNull(f);
            Assert.IsTrue(d.Tag == "d");
            Assert.IsTrue(e.Tag == "e");
            Assert.IsTrue(f.Subrules.Length == 2);
            Assert.IsTrue(f.Subrules[0] == d);
            Assert.IsTrue(f.Subrules[1] == e);
        }

        [TestMethod]
        public void ReferenceGhijReferralTest()
        {
            var rules = new ParserFactory().ParseRules(LoadTextFile("./data/multiReference.spec"));

            var g = rules["g"] as LiteralRule;
            var h = rules["h"] as LiteralRule;
            var i = rules["i"] as LiteralRule;
            var j = rules["j"] as SequenceRule;

            Assert.IsNotNull(g);
            Assert.IsNotNull(h);
            Assert.IsNotNull(i);
            Assert.IsNotNull(j);
            Assert.IsTrue(g.Tag == "g");
            Assert.IsTrue(h.Tag == "h");
            Assert.IsTrue(i.Tag == "i");
            Assert.IsTrue(j.Subrules.Length == 3);
            Assert.IsTrue(j.Subrules[0] == g);
            Assert.IsTrue(j.Subrules[1] == h);
            Assert.IsTrue(j.Subrules[2] == i);
        }

        [TestMethod]
        public void GroupTest()
        {
            var rules = new ParserFactory().ParseRules(LoadTextFile("./data/groupReference.spec"));

            var a = rules["a"] as LiteralRule;
            var b = rules["b"] as RepeatRule;
            var c = rules["c"] as NotRule;

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.IsNotNull(c);

            Assert.IsTrue(a.Tag == "a");
            Assert.IsTrue(b.Tag == "b");           
            Assert.IsTrue(b.Subrule == a);
            Assert.IsTrue(c.Subrule == a);
        }

        [TestMethod]
        public void SubstitutionTest()
        {
            var rules = new ParserFactory().ParseRules(LoadTextFile("./data/repeatSubstitution.spec"));

            var a = rules["a"] as CharRule;
            var b = rules["b"] as CharRule;
            var c = rules["c"] as NotRule;
            var d = rules["d"] as NotRule;
            var e = rules["e"] as SequenceRule;
            var f = rules["f"] as SequenceRule;
            var g = rules["g"] as SequenceRule;

            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.IsNotNull(c);
            Assert.IsNotNull(d);
            Assert.IsNotNull(e);
            Assert.IsNotNull(f);
            Assert.IsNotNull(g);

            Assert.IsTrue(a.Tag == "a");
            Assert.IsTrue(b.Tag == "b");
            Assert.IsTrue(c.Tag == "c");
            Assert.IsTrue(d.Tag == "d");
            Assert.IsTrue(e.Tag == "e");
            Assert.IsTrue(f.Tag == "f");
            Assert.IsTrue(g.Tag == "g");

            Assert.IsTrue(c.Subrule == b);
            Assert.IsTrue(d.Subrule == a);

            Assert.IsTrue(e.Subrules.Length == 2);
            Assert.IsTrue(e.Subrules[0] == a);
            Assert.IsTrue(e.Subrules[1] == b);

            Assert.IsTrue(f.Subrules.Length == 2);
            Assert.IsTrue(f.Subrules[0] is CharRule);
            Assert.IsTrue(f.Subrules[0].Tag.IndexOf(new InterpreterConfig().Tags.Unnamed) == 0);
            Assert.IsTrue(f.Subrules[1] == b);

            Assert.IsTrue(g.Subrules.Length == 4);
            Assert.IsTrue(g.Subrules[0] == f);
            Assert.IsTrue(g.Subrules[1] == a);
            Assert.IsTrue(g.Subrules[2] == b);
            Assert.IsTrue(g.Subrules[3] == e);
        }
    }
}


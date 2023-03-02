/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.examples.introduction;

namespace gg.ast.examples.test.introduction
{
    [TestClass]
    public class CharSetsTest
    {
        [TestMethod]
        public void AnyCharacterTest()
        {
            var anyCharRule = CharSet.AnyCharacter();

            // any character should pass
            Assert.IsTrue(anyCharRule.Parse("A").IsSuccess);
            Assert.IsTrue(anyCharRule.Parse("z").IsSuccess);
            Assert.IsTrue(anyCharRule.Parse("#").IsSuccess);
            Assert.IsTrue(anyCharRule.Parse("0").IsSuccess);

            // will not pass since the EOF reached
            Assert.IsFalse(anyCharRule.Parse("").IsSuccess);
        }

        [TestMethod]
        public void AToZSetTest()
        {
            var passingInput = new string[] { "a", "z" };
            var failingInput = new string[] { "", "A", "Z", "0", "9", "-", "$", "_" };

            var aToZRule = CharSet.AToZSet();
            var aToZSpecRule = CharSet.CharSetSpecFile()["aToZSet"];

            Array.ForEach(passingInput, text => Assert.IsTrue(aToZRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(aToZRule.Parse(text).IsSuccess));

            Array.ForEach(passingInput, text => Assert.IsTrue(aToZSpecRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(aToZSpecRule.Parse(text).IsSuccess));
        }

        [TestMethod]
        public void WideSetTest()
        {
            var passingInput = new string[] { "a", "z", "A", "Z", "0", "9" };
            var failingInput = new string[] { "", "-", "$", "_" };
            
            var wideSetRule = CharSet.WideSet();
            var wideSetSpecRule = CharSet.CharSetSpecFile()["wideSet"];

            Array.ForEach(passingInput, text => Assert.IsTrue(wideSetRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(wideSetRule.Parse(text).IsSuccess));
            
            Array.ForEach(passingInput, text => Assert.IsTrue(wideSetSpecRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(wideSetSpecRule.Parse(text).IsSuccess));
        }

        [TestMethod]
        public void EnumerationTest()
        {
            var passingInput = new string[] { "a", "b", "c" };
            var failingInput = new string[] { "", "d", "A", "C", "0" };

            var abcRule = CharSet.AbcEnumeration();
            var abcSpecRule = CharSet.CharSetSpecFile()["abcEnumeration"];

            Array.ForEach(passingInput, text => Assert.IsTrue(abcRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(abcRule.Parse(text).IsSuccess));

            Array.ForEach(passingInput, text => Assert.IsTrue(abcSpecRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(abcSpecRule.Parse(text).IsSuccess));
        }

        [TestMethod]
        public void NotInEnumerationTest()
        {
            var failingInput = new string[] { "", "a", "b", "c" };
            var passingInput = new string[] { "d", "A", "C", "0", "_", "+" };

            var notAbcRule = CharSet.NotAbcEnumeration();
            var notAbcSpecRule = CharSet.CharSetSpecFile()["notABCEnumeration"];

            Array.ForEach(passingInput, text => Assert.IsTrue(notAbcRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(notAbcRule.Parse(text).IsSuccess));

            Array.ForEach(passingInput, text => Assert.IsTrue(notAbcSpecRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(notAbcSpecRule.Parse(text).IsSuccess));
        }
    }
}

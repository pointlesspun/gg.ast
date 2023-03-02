/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.examples.introduction;

namespace gg.ast.examples.test.introduction
{
    [TestClass]
    public class SequencesTest
    {       
        [TestMethod]
        public void HelloWorldSequenceTest()
        {
            var passingInput = new string[] { "helloworld" };
            var failingInput = new string[] { "", "hello world", " hello\nworld " };

            var helloWorldRule = Sequences.HelloWorldSequence();
            var helloWorldSpecRule = Sequences.SequencesSpecFile()["helloWorld"];

            Array.ForEach(passingInput, text => Assert.IsTrue(helloWorldRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(helloWorldRule.Parse(text).IsSuccess));

            Array.ForEach(passingInput, text => Assert.IsTrue(helloWorldSpecRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(helloWorldSpecRule.Parse(text).IsSuccess));
        }

        [TestMethod]
        public void HelloSpaciousWorldSequenceTest()
        {
            var passingInput = new string[] { "helloworld", "hello world", " hello\nworld " };
            var failingInput = new string[] { "", "world hello", "hello worl" };

            var helloWorldRule = Sequences.HelloSpaciousWorldSequence();
            var helloWorldSpecRule = Sequences.SequencesSpecFile()["helloSpaciousWorld"];

            Array.ForEach(passingInput, text => Assert.IsTrue(helloWorldRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(helloWorldRule.Parse(text).IsSuccess));

            Array.ForEach(passingInput, text => Assert.IsTrue(helloWorldSpecRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(helloWorldSpecRule.Parse(text).IsSuccess));
        }
    }
}

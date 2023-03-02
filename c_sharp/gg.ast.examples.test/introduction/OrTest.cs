/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.examples.introduction;

namespace gg.ast.examples.test.introduction
{
    [TestClass]
    public class OrTest
    {       
        [TestMethod]
        public void HelloWorldOrTest()
        {
            var passingInput = new string[] { "hello", "world" };
            var failingInput = new string[] { "", "hi", "weld" };

            var helloWorldRule = OrExample.HelloOrWorld();
            var helloWorldSpecRule = OrExample.LoadSpecFileRules()["helloOrWorld"];

            Array.ForEach(passingInput, text => Assert.IsTrue(helloWorldRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(helloWorldRule.Parse(text).IsSuccess));

            Array.ForEach(passingInput, text => Assert.IsTrue(helloWorldSpecRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(helloWorldSpecRule.Parse(text).IsSuccess));
        }

        [TestMethod]
        public void HelloOrderTest()
        {
            var input = new string[] { "3", "3 * 3"};
            var valueTag = "value";
            var operationTag = "operation";
            
            var orRules = OrExample.LoadSpecFileRules();
            var wrongResultRule = orRules["wrongResult"];
            var correctResultRule = orRules["correctResult"];

            // both rules get this right
            Assert.IsTrue(wrongResultRule.Parse(input[0]).Nodes[0].Tag == valueTag);
            Assert.IsTrue(correctResultRule.Parse(input[0]).Nodes[0].Tag == valueTag);

            // wrong result thinks this is a value
            Assert.IsTrue(wrongResultRule.Parse(input[1]).Nodes[0].Tag == valueTag);
            
            // only the correct result gets the next one right
            Assert.IsTrue(correctResultRule.Parse(input[1]).Nodes[0].Tag == operationTag);
        }
    }
}

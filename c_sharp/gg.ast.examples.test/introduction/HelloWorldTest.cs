/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.examples.introduction;

namespace gg.ast.examples.test.introduction
{
    [TestClass]
    public class HelloWorldTest
    {
        [TestMethod]
        public void HelloWorldRuleTest()
        {
            var helloWorldRule = HelloWorld.HelloWorldRule();

            var helloWorldText = "hello world";
            var helloWorldResult = helloWorldRule.Parse(helloWorldText);
            
            Assert.IsTrue(helloWorldResult.IsSuccess);
            Assert.IsTrue(helloWorldResult.CharactersRead == helloWorldText.Length);
            Assert.IsTrue(helloWorldResult.Nodes[0].Tag == "helloWorld");

            var hiWorldText = "hi world";
            var hiWorldResult = helloWorldRule.Parse(hiWorldText);

            Assert.IsFalse(hiWorldResult.IsSuccess);
        }

        [TestMethod]
        public void HelloWorldSpecTest()
        {
            var helloWorldRule = HelloWorld.HelloWorldSpecFile();

            var helloWorldText = "hello world";
            var helloWorldResult = helloWorldRule.Parse(helloWorldText);

            Assert.IsTrue(helloWorldResult.IsSuccess);
            Assert.IsTrue(helloWorldResult.CharactersRead == helloWorldText.Length);
            Assert.IsTrue(helloWorldResult.Nodes[0].Tag == "helloWorld");

            var hiWorldText = "hi world";
            var hiWorldResult = helloWorldRule.Parse(hiWorldText);

            Assert.IsFalse(hiWorldResult.IsSuccess);
        }
    }
}

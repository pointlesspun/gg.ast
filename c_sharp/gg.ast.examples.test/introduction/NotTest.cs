/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using gg.ast.examples.introduction;
using gg.ast.core;

namespace gg.ast.examples.test.introduction
{
    [TestClass]
    public class NotTest
    {       
        [TestMethod]
        [ExpectedException(typeof(ParseException))] 
        public void InfiniteLoopTest()
        {
            NotExample.NotFoo().Parse("bar");
        }

        [TestMethod]
        public void AvoidInfiniteLoopTest()
        {
            NotExample.NotFoo(1).Parse("bar");
        }


        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void InfiniteSpecLoopTest()
        {
            NotExample.LoadSpecFileRules()["infiniteLoop"].Parse("bar");
        }

        [TestMethod]
        public void AvoidInfiniteSpecLoopTest()
        {
            NotExample.LoadSpecFileRules()["avoidInfiniteLoop"].Parse("bar");
        }

        [TestMethod]
        public void CommentTest()
        {
            var passingInput = new string[] { "/*f*/", "/**/", "/** this is a longer comment **/", "/* This is a \n multiline \n comment */" };
            var failingInput = new string[] { "", "/*", " /* white space in front of a comment is not working */", "/*/", "/* almost *" };

            var specRules = NotExample.LoadSpecFileRules();
            var commentRule = NotExample.CreateCommentRule();
            var commentSpecRule = specRules["comment"];
            var altCommentSpecRule = specRules["altComment"];

            Array.ForEach(passingInput, text => Assert.IsTrue(commentRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(commentRule.Parse(text).IsSuccess));

            Array.ForEach(passingInput, text => Assert.IsTrue(commentSpecRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(commentSpecRule.Parse(text).IsSuccess));

            Array.ForEach(passingInput, text => Assert.IsTrue(altCommentSpecRule.Parse(text).IsSuccess));
            Array.ForEach(failingInput, text => Assert.IsFalse(altCommentSpecRule.Parse(text).IsSuccess));
        }
    }
}

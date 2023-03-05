/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.core;
using gg.ast.core.rules;
using gg.ast.interpreter;

namespace gg.ast.examples.introduction
{
    public class HelloWorld
    {
        public static IRule HelloWorldRule()
        {
            return new LiteralRule()
            {
                Tag = "helloWorld",
                Characters = "hello world"
            };
        }

        public static IRule LoadSpecFile(string specFile = "introduction/hello_world.spec")
        {
            return new ParserFactory().ParseFile(specFile);
        }
    }
}

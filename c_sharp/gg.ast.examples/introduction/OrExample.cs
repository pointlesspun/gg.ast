/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

using gg.ast.core;
using gg.ast.core.rules;
using gg.ast.interpreter;

namespace gg.ast.examples.introduction
{
    public class OrExample
    {
        public static IRule HelloOrWorld()
        {
            return new OrRule()
            {
                Tag = "helloOrWorld",        
                Subrules = new IRule[]
                {
                    new LiteralRule()
                    {
                        Tag = "hello",
                        Characters = "hello"
                    },
                    new LiteralRule()
                    {
                        Tag = "world",
                        Characters = "world"
                    },
                }
            };
        }       

        public static Dictionary<string, IRule> LoadSpecFileRules(string specFile = "introduction/or.spec")
        {
            return new ParserFactory().ParseFileRules(specFile);
        }
    }
}


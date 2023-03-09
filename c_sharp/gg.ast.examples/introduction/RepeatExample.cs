/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

using gg.ast.core;
using gg.ast.core.rules;
using gg.ast.interpreter;

using static gg.ast.util.FileCache;

namespace gg.ast.examples.introduction
{
    public class RepeatExample
    {
        public static IRule RepeatHelloOrWorld(int min, int max, IRule whitespaceRule)
        {
            return new RepeatRule()
            {
                Tag = "repeatHelloOrWorld",        
                Subrule = new LiteralRule()
                {
                    Tag = "helloWorld",
                    Characters = "hello world"
                },
                Min = min, 
                Max = max, 
                WhiteSpaceRule = whitespaceRule
            };
        }       

        public static Dictionary<string, IRule> LoadSpecFileRules(string specFile = "introduction/repeat.spec")
        {
            var rules = LoadTextFile(specFile);
            return new ParserFactory().ParseRules(rules);
        }
    }
}


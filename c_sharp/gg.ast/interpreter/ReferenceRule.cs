/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.core;

namespace gg.ast.interpreter
{
    public class ReferenceRule : MetaRuleBase
    {
        public string Reference { get; set; }

        protected override ParseResult ParseRule(string text, int index)
        {
            return Subrule.Parse(text, index);
        }

        // do not deep clone...
    }
}

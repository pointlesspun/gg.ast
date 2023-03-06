/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

namespace gg.ast.core.rules
{
    /// <summary>
    /// Throws an exception if the subrule doesn't succeed
    /// </summary>
    public class CriticalRule : MetaRuleBase
    {
        public CriticalRule()
        {
            Visibility = NodeVisiblity.Transitive;
        }

        protected override ParseResult ParseRule(string text, int index)
        {
            var result = Subrule.Parse(text, index);

            if (!result.IsSuccess)
            {
                throw new ParseException(null/*parent,*/, this, text, index);
            }

            return result;
        }       
    }
}
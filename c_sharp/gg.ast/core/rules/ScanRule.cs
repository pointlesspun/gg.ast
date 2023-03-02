/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

namespace gg.ast.core.rules
{
    /// <summary>
    /// Scan text or test if a subrule applies, return the result but change the
    /// result's charactersread to 0.
    /// </summary>
    public class ScanRule : MetaRuleBase
    {
        protected override ParseResult ParseRule(string text, int index)
        {
            var result = Subrule.Parse(text, index);

            result.CharactersRead = 0;

            return result;
        }
    }
}

/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

namespace gg.ast.core.rules
{
    /// <summary>
    /// Succeed if the associated subrule fails, fails if the associated 
    /// subrule succeeds. In either case the current index will not be
    /// moved.
    /// </summary>
    public class NotRule : MetaRuleBase
    {
        /// <summary>
        /// Number of characters to skip if the NotRule succeeds.
        /// </summary>
        public int Skip { get; set; } = 0;

        /// <summary>
        /// Will fail if the End Of File/Beginning of file is reached before 
        /// the subrule is tested
        /// </summary>
        public bool FailOnEOF { get; set; } = true;

        protected override ParseResult ParseRule(string text, int index)
        {
            if (FailOnEOF && (index < 0 || index >= text.Length))
            {
                return ParseResult.Fail; 

            }
            if (Subrule.Parse(text, index).IsSuccess)
            {
                return ParseResult.Fail;
            }

            return BuildResult(true, index, Skip);
        }
    }
}

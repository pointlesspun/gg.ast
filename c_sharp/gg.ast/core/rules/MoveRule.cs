/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

namespace gg.ast.core.rules
{
    /// <summary>
    /// Move the current index forward or backward.
    /// </summary>
    public class MoveRule : RuleBase
    {
        /// <summary>
        /// Number of positions to move the current index forward or backward
        /// </summary>
        public int Count { get; set; } = 1;

        // xxx to do Move until rule is matched

        protected override ParseResult ParseRule(string text, int index)
        {
            var newIndex = index + Count;

            // xxx todo: this behaviour needs to be specified, ie fail_if_bounds_exceed
            return (newIndex >= 0 && newIndex <= text.Length)
                ? new ParseResult(true, Count)
                : ParseResult.Fail;
        }

        public override string ToString() => $"{GetType().Name}: {Tag}({Count})";
    }
}

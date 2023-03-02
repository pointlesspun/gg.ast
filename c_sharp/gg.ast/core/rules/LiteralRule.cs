/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

namespace gg.ast.core.rules
{
    /// <summary>
    /// Parse a literal
    /// </summary>
    public class LiteralRule : RuleBase
    {
        /// <summary>
        /// Characters which have to matched in the same order.
        /// </summary>
        public string Characters { get; set; }

        /// <summary>
        /// If set to true the characters will be parsed against the same case,
        /// false means character casing will not affect the outcome.
        /// </summary>
        public bool IsCaseSensitive { get; set; } = true;

        protected override ParseResult ParseRule(string text, int index)
        {
            var length = Characters.Length;

            for (var i = 0; i < length; i++)
            {
                if (i + index >= text.Length || !MatchesCharacter(Characters[i], text[index + i], IsCaseSensitive))
                {
                    return ParseResult.Fail;
                }
            }

            return BuildResult(true, index, length);
        }

        public override string ToString() => $"{GetType().Name}: {Tag}= \"{Characters}\";";

        private bool MatchesCharacter(char a, char b, bool isCaseSensitive) => isCaseSensitive ? a == b : char.ToUpper(a) == char.ToUpper(b);
    }
}

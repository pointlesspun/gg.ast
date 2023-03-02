/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;

using gg.ast.util;

namespace gg.ast.core.rules
{
    /// <summary>
    /// Parse single or multiple characters according to a MatchType
    /// </summary>
    public class CharRule : RuleBase, IRange
    {
        public enum MatchType
        {
            /// <summary>
            /// Match any character
            /// </summary>
            Any,
            
            /// <summary>
            /// Match a character in a range eg a to z
            /// </summary>
            InRange,

            /// <summary>
            /// Match a character in a several ranges eg a to z, A to Z and 0 to 9
            /// </summary>
            InMultiRange,

            /// <summary>
            /// Match a character against a set of characters eg 'abc123#'
            /// </summary>
            InEnumeration,

            /// <summary>
            /// Match a character which is not included a set of characters eg 'abc123#'
            /// </summary>
            NotInEnumeration
        };

        private string _characters;

        /// <summary>
        /// Characters to match against, the interpretation of which depends on
        /// the value of the MatchCharacters property.
        /// </summary>
        public string Characters 
        { 
            get => _characters; 
            set 
            {
                Contract.RequiresNotNullOrEmpty(value);
                _characters = value;
            } 
        }

        /// <summary>
        /// Setting which defines how the characters are being matched see MatchType
        /// </summary>
        public MatchType MatchCharacters { get; set; } = MatchType.Any;

        /// <summary>
        /// Minimum number of matches, if there are less matches than this, the rule will fail.
        /// If 0 or negative there will be no minimum number.
        /// </summary>
        public int Min { get; set; } = 1;

        /// <summary>
        /// Max number of matches. If the max is reached during parsing, the rule will
        /// succeed. If this is 0 or less there will be no maximum.
        /// </summary>
        public int Max { get; set; } = -1;

        protected override ParseResult ParseRule(string text, int index)
        {
            if (index >= 0 && index < text.Length)
            {
                return MatchCharacters switch
                {
                    MatchType.Any => Match(text, index),
                    MatchType.InRange => Match(text, index, c => c >= Characters[0] && c <= Characters[1]),
                    MatchType.InMultiRange => Match(text, index, c => IsInMultiRange(c, Characters)),
                    MatchType.InEnumeration => Match(text, index, c => Characters.IndexOf(c) >= 0),
                    MatchType.NotInEnumeration => Match(text, index, c => Characters.IndexOf(c) < 0),
                    _ => ParseResult.Fail,
                };
            }

            return ParseResult.Fail;
        }

        public override string ToString() =>
            MatchCharacters == MatchType.Any 
            ? Tag
            : $"{GetType().Name}: {Tag} = {MatchCharacters} in \"{Characters}\";";

        private ParseResult Match(string text, int index, Func<char, bool> test = null)
        {
            int currentIndex = index;

            while (currentIndex >= 0 
                && currentIndex < text.Length 
                && !HasExceededMax(currentIndex - index)
                && (test == null || test(text[currentIndex])))
            {
                currentIndex++;
            }

            return (HasSucceeded(currentIndex - index))
                    ? BuildResult(true, index, currentIndex - index)
                    : ParseResult.Fail;
        }
        
        private bool IsInMultiRange(char c, string multiRange)
        {
            for (var i = 0; i < multiRange.Length; i += 2)
            {
                if (c >= multiRange[i] && c <= multiRange[i+1])
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasSucceeded(int length) => Min <= 0 || length >= Min;

        private bool HasExceededMax(int charactersRead) => Max > 0 && charactersRead >= Max;
    }
}

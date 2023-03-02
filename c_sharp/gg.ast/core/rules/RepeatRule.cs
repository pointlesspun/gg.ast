/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

namespace gg.ast.core.rules
{
    /// <summary>
    /// Repeat a subrule Min to Max times. Will fail if the subrule
    /// fails at any time. 
    /// </summary>
    public class RepeatRule : MetaRuleBase, IRange
    {

        /// <summary>
        /// Minimum number of matches, if there are less matches than this, the rule will fail.
        /// If 0 or negative there will be no minimum number.
        /// </summary>
        public int Min { get; set; } = 0;

        /// <summary>
        /// Max number of matches. If the max is reached during parsing, the rule will
        /// succeed. If this is 0 or less there will be no maximum.
        /// </summary>
        public int Max { get; set; } = -1;

        /// <summary>
        /// Rule used to match skippable characters (whitespace), if null (the default)
        /// no characters will be skipped.
        /// </summary>
        public IRule WhiteSpaceRule { get; set; }

        protected override ParseResult ParseRule(string text, int index)
        {
            var currentIndex = index;
            var repeatCount = 0;
            var children = new List<AstNode>();

            for (; Max < 0 || repeatCount < Max; repeatCount++)
            {
                var whitespacesSkipped = WhiteSpaceRule == null ? 0 : WhiteSpaceRule.Parse(text, currentIndex).CharactersRead;
                currentIndex += whitespacesSkipped;

                var subRuleResult = Subrule.Parse(text, currentIndex);

                if (!subRuleResult.IsSuccess)
                {
                    // if the subrule failed to parse, but whitespaces have been read, we end
                    // up with in inaccurate result (ie more characters have been read than
                    // was actually the case). So adjust the current index with the 
                    // number of whitespacesSkipped. 
                    currentIndex -= whitespacesSkipped;
                    break;
                }

                if (subRuleResult.Nodes != null)
                {
                    children.AddRange(subRuleResult.Nodes);
                }

                if (subRuleResult.CharactersRead == 0 && Max < 0)
                {
                    throw new ParseException("Potential infinite loop encountered while parsing " + this, null, this, text, index);
                } 

                currentIndex += subRuleResult.CharactersRead;
            }

            return (repeatCount >= Min)
                ? BuildResult(true, index, currentIndex - index, children)
                : ParseResult.Fail;
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {Tag}: {Subrule.Tag};";
        }
    }
}

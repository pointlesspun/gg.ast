/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.core;

namespace gg.ast.core.rules
{
    /// <summary>
    /// Rule which takes two rules a "consume rule" and a "termination rule" and continues matching
    /// until either the consume rule fails or the termination rule succeeds. The consume rule
    /// OR the termination can be null but not both. 
    /// </summary>
    public class UntilRule : RuleGroupBase
    {
        /// <summary>
        /// If a termination rule is defined, the untilrule will stop parsing when
        /// the termination rule succeeds.
        /// </summary>
        public IRule TerminationRule 
        { 
            get => _subrules[0]; 
            set
            {
                _subrules[0] = value;
                value.Parent = this;
            }            
        }

        /// <summary>
        /// If a consume rule is defined, the untilrule will continue parsing until
        /// the consume rule fails.
        /// </summary>
        public IRule ConsumeRule { 
            get => _subrules[1]; 
            set
            {
                _subrules[1] = value;
                value.Parent = this;
            } 
        }

        /// <summary>
        /// When the termination rule succeeds or consume rule fails, an additional
        /// requirement can be provided which states how many characters
        /// must be read to consider this rule a success.
        /// 
        /// xxx todo: consider moving to Min where min is the number of times this must
        /// succeed, just like repeat and char rule.
        /// 
        /// </summary>
        public int MinLength { get; set; } = 0;

        /// <summary>
        /// If set to true the termination rule's characters read will be added
        /// to the final result, if false the until rule will end at the point
        /// the termination rule succeeded.
        /// </summary>
        public bool ConsumeTerminationTokens { get; set; } = false;

        public UntilRule()
        {
            _subrules = new IRule[2];
        }
        
        protected override ParseResult ParseRule(string text, int index)
        {
            var currentIndex = index;
            var lastIndex = -1;

            while (currentIndex < text.Length)
            {
                currentIndex += WhiteSpaceRule == null ? 0 : WhiteSpaceRule.Parse(text, index).CharactersRead;

                if (TerminationRule != null)
                {
                    var terminationResult = TerminationRule.Parse(text, currentIndex);

                    if (terminationResult.IsSuccess)
                    {
                        currentIndex += ConsumeTerminationTokens ? terminationResult.CharactersRead : 0;
                        break;
                    }
                }

                var skippedCharacters = ConsumeRule == null ? 1 : ConsumeRule.Parse(text, currentIndex).CharactersRead;

                if (skippedCharacters < 0)
                {
                    break;
                }

                if (skippedCharacters == 0)
                {
                    if (lastIndex == currentIndex)
                    {
                        throw new ParseException(
                            "Infinite loop detected in Until rule, neither the termination clause or the consumable clause applies and the rule is stuck.",
                            /*parent,*/null, this, text, currentIndex
                        );
                    }
                    else
                    {
                        lastIndex = currentIndex;
                    }
                }

                currentIndex += skippedCharacters;
            }

            var length = currentIndex - index;
            return BuildResult(length >= MinLength, index, length); 
        }

        public override string ToString() => Tag;
    }
}

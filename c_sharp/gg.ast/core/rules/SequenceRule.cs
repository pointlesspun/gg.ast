/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;
using System.Linq;

namespace gg.ast.core.rules
{
    /// <summary>
    /// Parsers all added subrules and is succesfull if all of them succeed
    /// </summary>
    public class SequenceRule : RuleGroupBase
    {
        protected override ParseResult ParseRule(string text, int index)
        {
            var currentIndex = index;
            var children = new List<AstNode>();

            for (var i = 0; i < _subrules.Length; i++)
            {
                var subRule = _subrules[i];

                var whitespacesSkipped = WhiteSpaceRule == null ? 0 : WhiteSpaceRule.Parse(text, currentIndex).CharactersRead;
                currentIndex += whitespacesSkipped;

                var result = subRule.Parse(text, currentIndex);

                if (result.IsSuccess)
                {
                    // if no characters have been read we may have failed to parse an 
                    // optional subrule. If whitespace has been skipped and this is the last rule
                    // the actual CharactersRead would be subrules + trailing whitespace. This
                    // can lead to inaccurate results. So in this case, adjust the current index
                    // by subtracting the numbers of whitespaces skipped.
                    if (result.CharactersRead == 0)
                    {
                        currentIndex -= whitespacesSkipped;
                    }
                    else
                    {
                        currentIndex += result.CharactersRead;
                    }

                    if (result.Nodes != null)
                    {
                        children.AddRange(result.Nodes);
                    }
                    continue;
                }

                return ParseResult.Fail;
            }

            return BuildResult(true, index, currentIndex - index, children);
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {Tag}({string.Join(", ", Subrules.Select(rule => rule.Tag))});";
        } 
    }
}

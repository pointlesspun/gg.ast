/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Linq;

namespace gg.ast.core.rules
{
    /// <summary>
    /// Matches one of the added Subrules
    /// </summary>
    public class OrRule : RuleGroupBase
    {
        protected override ParseResult ParseRule(string text, int index)
        {
            foreach (var subRule in Subrules)
            {
                var subRuleResult = subRule.Parse(text, index);

                if (subRuleResult.IsSuccess)
                {
                    if (Visibility == RuleVisiblity.Visible || subRuleResult.Nodes == null)
                    {
                        return BuildResult(true, index, subRuleResult.CharactersRead, subRuleResult.Nodes);
                    }

                    return subRuleResult;
                }
            }

            return ParseResult.Fail;
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {Tag}({string.Join(" | ", Subrules.Select(rule => rule.Tag))});";
        }
    }
}

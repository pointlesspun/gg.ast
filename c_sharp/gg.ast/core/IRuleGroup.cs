/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

namespace gg.ast.core
{
    public interface IRuleGroup : IRule, IEnumerable<IRule>
    {
        IRule WhiteSpaceRule { get; set; }

        IRule[] Subrules { get; set; }
    }
}

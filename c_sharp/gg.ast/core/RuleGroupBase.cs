/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using gg.ast.util;

namespace gg.ast.core
{
    public abstract class RuleGroupBase : RuleBase, IRuleGroup
    {
        protected IRule[] _subrules;

        public IRule WhiteSpaceRule { get; set; }

        public IRule[] Subrules
        {
            get { return _subrules; }
            set 
            {
                Contract.RequiresNoNullValues(value, "Cannot add null rules.");

                _subrules = value;

                foreach (var rule in _subrules)
                {
                    rule.Parent = this;
                }
            }
        }

        public IEnumerator<IRule> GetEnumerator()
        {
            return ((IEnumerable<IRule>)Subrules).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Subrules.GetEnumerator();
        }

        /// <summary>
        /// Semi-shallow clone. Makes clones of the rules.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = (RuleGroupBase) base.Clone();

            clone._subrules = new IRule[Subrules.Length];

            for (var i = 0; i < Subrules.Length; i++)
            {
                clone._subrules[i] = (IRule) Subrules[i].Clone();
            }

            return clone;
        }
    }
}

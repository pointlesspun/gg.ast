/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.util;

namespace gg.ast.core
{
    public abstract class MetaRuleBase : RuleBase, IMetaRule
    {
        protected IRule _subrule;

        public IRule Subrule 
        { 
            get => _subrule; 
            set
            {
                Contract.Requires(value != null);   
                _subrule = value;
                _subrule.Parent = this;
            }
        }

        /// <summary>
        /// Sets the subrule to the given rule and copies its tag. Furthermore if the given rules' visibility is
        /// not hidden, it will sets this rule's visibility to the the provided visibility (transitive by default). 
        /// If the given rule's visibility is hidden, this rule's visibility will be hidden as well.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public MetaRuleBase Bind(IRule rule, RuleVisiblity visibility = RuleVisiblity.Transitive)
        {
            Contract.RequiresNotNull(rule);

            Tag ??= rule.Tag;
            Visibility = rule.Visibility == RuleVisiblity.Hidden 
                            ? RuleVisiblity.Hidden  
                            : visibility;
            Subrule = rule;
            return this;
        }

        /// <summary>
        /// Semi-shallow clone. Makes clones of the subrule.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            var clone = (MetaRuleBase)base.Clone();

            clone.Subrule = (IRule) _subrule.Clone();

            return clone;
        }
    }
}

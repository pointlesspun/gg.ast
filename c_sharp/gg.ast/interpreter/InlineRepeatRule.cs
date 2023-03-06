using gg.ast.core;
using gg.ast.core.rules;
using gg.ast.util;

namespace gg.ast.interpreter
{
    public class InlineRepeatRule : RepeatRule
    {
        private IRule _parent;

        public IRule Parent 
        { 
            get => _parent; 
            set
            {
                // this should prevent assigning the rule to two parents. If a transfer
                // from one parent to another is needed, the parent needs to be cleared first
                // by setting it to null.
                Contract.Requires(value == null || _parent == null);

                _parent = value;
            } 
        }
    }
}

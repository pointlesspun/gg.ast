﻿/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using gg.ast.core;
using gg.ast.util;

namespace gg.ast.interpreter
{
    public class ReferenceRule : MetaRuleBase
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

        public string Reference { get; set; }

        protected override ParseResult ParseRule(string text, int index)
        {
            return Subrule.Parse(text, index);
        }

        // do not deep clone...
    }
}

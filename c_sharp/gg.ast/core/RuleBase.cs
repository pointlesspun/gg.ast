/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;

using gg.ast.util;

namespace gg.ast.core
{
    public abstract class RuleBase : IRule
    {
        public IRule Parent { get; set; }

        public RuleVisiblity Visibility { get; set; } = RuleVisiblity.Visible;

        public string Tag { get; set; }

        public RuleCommitHandler OnCommit { get; set; }

        public ParseResult Parse(string text, int index = 0)
        {
            Contract.RequiresNotNull(text);

            var result = ParseRule(text, index);

            if (result.IsSuccess)
            {
                OnCommit?.Invoke(this, result);
            }

            return result;
        }

        public override string ToString()
        {
            var tag = string.IsNullOrEmpty(Tag) ? "" : " (" + Tag + ")";
            return $"{GetType().Name}{tag}";
        }

        /// <summary>
        /// Shallow clone (Memberwise)
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        protected abstract ParseResult ParseRule(string text, int index);

        protected ParseResult BuildResult(bool isSuccess, int index, int length, List<AstNode> children = null)
        {
            if (isSuccess)
            {
                // if no characters are read, do not create a node (eg for rules expressing 0 or N with
                // 0 success)
                var nodes = length <= 0 ? null : MapVisibilityToNodes(index, length, children);

                return new ParseResult(isSuccess, length, nodes);
            }
            else
            {
                return ParseResult.Fail;
            }
        }

        private List<AstNode> MapVisibilityToNodes(int index, int length, List<AstNode> children) =>
            Visibility switch
            {
                RuleVisiblity.Visible => new List<AstNode>()
                {
                    new AstNode(index, length, children: children, rule: this)
                },
                RuleVisiblity.Hidden => null,
                RuleVisiblity.Transitive => children,

                _ => throw new ArgumentException("unhandled case for RuleVisibility"),
            };
    }
}

/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

namespace gg.ast.core
{
    /// <summary>
    /// Node making up an abstract syntax tree
    /// </summary>
    public class AstNode
    {
        public AstNode Parent { get; set; }

        public List<AstNode> Children { get; set; }

        public AstNode this[int index] => Children[index];

        /// <summary>
        /// Start index in the text 
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Length in characters 
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Rule that produced this node
        /// </summary>
        public IRule Rule { get; set; }

        public string Tag => Rule.Tag;

        public AstNode(int startIndex = 0, int length = 0, AstNode parent = null, List<AstNode> children = null, IRule rule = null)
        {
            Children = children;
            Parent = parent;
            StartIndex = startIndex;
            Length = length;
            Rule = rule;

            if (Children != null)
            {
                Children.ForEach(c => c.Parent = this);
            }
        }

        public AstNode AddChild(AstNode child)
        {
            child.Parent = this;
            Children.Add(child);
            return child;
        }

        public override string ToString()
        {
            var ruleText = Rule == null ? "null" : Rule.Tag;
            return $"node({ruleText})";
        }

        public string Substring(string text)
        {
            return text.Substring(StartIndex, Length);
        }
    }
}

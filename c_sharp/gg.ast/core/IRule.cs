/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;

namespace gg.ast.core
{
    public delegate void RuleCommitHandler(IRule rule, in ParseResult result);

    /// <summary>
    /// Visiblity of the produced node. Visible means the node and its children will
    /// be visible in the AST. Transitive means only the node's children will be
    /// visible in the AST. Hidden implies neither the node nor its children are visible.
    /// </summary>
    public enum NodeVisiblity
    {
        Visible,
        Transitive,
        Hidden
    }

    public interface IRule : ICloneable
    {
        /// <summary>
        /// Unique identifier of the rule. 
        /// </summary>
        int Id { get; set; }

        // leaving this here as documentation / reminder: rules can have multiple parents
        // as they may be shared. So having a single parent doesn't make sense.
        // The exception are the inline rules (reference rule, inline repeat)
        // but they have explicit measures to prevent sharing. 

        // IRule Parent { get; set; }

        NodeVisiblity Visibility { get; set; }

        /// <summary>
        /// Human readable text describing the token's tag this rule produces.
        /// </summary>
        string Tag { get; set; }

        RuleCommitHandler OnCommit { get; set; }

        /// <summary>
        /// Parse the given text against this rule starting at the given index.
        /// </summary>
        /// <param name="text">Non null text to parse.</param>
        /// <param name="index">Start index in the text</param>
        /// <returns>The result of applying this rule.</returns>
        ParseResult Parse(string text, int index = 0);
    }
}
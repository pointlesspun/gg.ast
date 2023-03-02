/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;

namespace gg.ast.core
{
    public delegate void RuleCommitHandler(IRule rule, in ParseResult result);

    public enum RuleVisiblity
    {
        Visible,
        Transitive,
        Hidden
    }

    public interface IRule : ICloneable
    {

        IRule Parent { get; set; }

        RuleVisiblity Visibility { get; set; }

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
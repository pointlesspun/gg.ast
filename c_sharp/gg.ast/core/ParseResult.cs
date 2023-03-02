/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Collections.Generic;
using System.Text;

namespace gg.ast.core
{
    /// <summary>
    /// Result of applying a rule to a piec 
    /// </summary>
    public struct ParseResult
    {
        public static readonly ParseResult Fail = new ParseResult()
        {
            IsSuccess = false,
            Nodes = null,
            CharactersRead = 0
        };

        /// <summary>
        /// Was the applied rule a success/
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// How many characters did the rule read.
        /// </summary>
        public int CharactersRead { get; set; }

        /// <summary>
        /// Ast nodes produced 
        /// </summary>
        public List<AstNode> Nodes { get; set; }

        public ParseResult(bool wasSuccess, int charactersRead)
        {
            IsSuccess = wasSuccess;
            CharactersRead = charactersRead;
            Nodes = null;
        }

        public ParseResult(bool wasSuccess, int charactersRead, AstNode node)
        {
            IsSuccess = wasSuccess;
            CharactersRead = charactersRead;
            Nodes = new List<AstNode>() { node };
        }

        public ParseResult(bool wasSuccess, int charactersRead, List<AstNode> nodes)
        {
            IsSuccess = wasSuccess;
            CharactersRead = charactersRead;
            Nodes = nodes;
        }

     
    }
}

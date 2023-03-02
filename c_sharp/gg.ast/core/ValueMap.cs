/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System.Collections.Generic;

using gg.ast.util;

namespace gg.ast.core
{
    public delegate object MapFunction(string text, AstNode node);

    public class ValueMap
    {
        private readonly Dictionary<string, MapFunction> _functionLookup = new();

        public MapFunction this[string index]
        {
            get => _functionLookup[index];
            set => _functionLookup[index] = value;
        }

        public ValueMap Register(string tag, MapFunction function)
        {
            Contract.RequiresNotNullOrEmpty(tag);
            Contract.RequiresNotNull(function);

            _functionLookup[tag] = function;
            return this;
        }

        public object Map(string tag, string text, AstNode node)
        {
            Contract.RequiresNotNullOrEmpty(tag);
            Contract.RequiresNotNullOrEmpty(text);
            Contract.RequiresNotNull(node);

            return _functionLookup[tag](text, node);
        }

        public T Map<T>(string tag, string text, AstNode node)
        {
            return (T)Map(tag, text, node);
        }

        public T Map<T>(string text, AstNode node)
        {
            return (T)Map(node.Rule.Tag, text, node);
        }
    }
}

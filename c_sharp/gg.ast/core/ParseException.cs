/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Text;

using gg.ast.util;

namespace gg.ast.core
{
    public class ParseException : Exception
    {
        private AstNode _node;
        private IRule _rule;
        private string _text;
        private int _index;

        public AstNode Node { get => _node; set => _node = value; }

        public IRule Rule { get => _rule; set => _rule = value; }

        public string Text { get => _text; set => _text = value; }

        public int Index { get => _index; set => _index = value; }

        public ParseException(string message, AstNode node, IRule rule, string text, int index)
            : base(message + "\n" + ComposeMessage(node, rule, text, index))
        {
            // capture information for debugging
            Node = node;
            Rule = rule;
            Text = text;
            Index = index;
        }


        public ParseException(AstNode node, IRule rule, string text, int index)
            : base(ComposeMessage(node, rule, text, index))
        {
            // capture information for debugging
            Node = node;
            Rule = rule;
            Text = text;
            Index = index;
        }

        public static string ComposeMessage(AstNode node, IRule rule, string text, int index)
        {
            var subString = text.SubstringAround(index, 20);
            return text.GetCursorPosition(index)
                  + " " + ComposeError(rule, text, index)
                  + "\n" + subString
                  + "\n" + "".AddPrefix(Math.Min(23, index), " ") + "^"
                  + "\n\nStack trace:\n" + ComposeAstStackTrace(node, rule);
        }

        public static string ComposeError(IRule rule, string text, int index) =>
            index >= text.Length
            ? $"Aborted parsing during the rule: `{rule}`.\nThe end of the text was reached before the rule completed."
            : $"Aborted parsing during the rule: `{rule}`.\nEncountered unexpected input `{text[index]}`.";

        public static string ComposeAstStackTrace(AstNode node, IRule lastRule)
        {
            var indent = 0;
            var path = node.GetPathToRoot();
            var builder = new StringBuilder();

            foreach (var pathNode in path)
            {
                builder.AppendLine($"{pathNode.Rule}".AddPrefix(indent));
                indent += 2;
            }

            builder.AppendLine($"-> {lastRule}".AddPrefix(indent));


            return builder.ToString();
        }
    }
}

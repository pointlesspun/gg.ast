/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace gg.ast.util
{
    /// <summary>
    /// Extension methods on strings
    /// </summary>
    public static partial class StringExtensions
    {
        /// <summary>
        /// Add 'count' prefixes to the given string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string AddPrefix(this string source, int count, string prefix = " ")
        {
            var builder = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                builder.Append(prefix);
            }

            builder.Append(source);

            return builder.ToString();
        }

        /// <summary>
        /// Returns a 'cursor position' defined as the line numer and the character index on that line
        /// </summary>
        /// <param name="text"></param>
        /// <param name="characterIndex">Current position in the string</param>
        /// <param name="tabCount">Number of spaces a tab takes</param>
        /// <param name="endOfLine">End of line character</param>
        /// <param name="ignore">Characters which can be ignored</param>
        /// <returns></returns>
        public static (int line, int character) GetCursorPosition(
            this string text, 
            int characterIndex, 
            int tabCount = 4, string endOfLine = "\n", string ignore = "\r")
        {
            int line = 1;
            var character = 0;
            int index = characterIndex;

            while (index >= text.Length)
            {
                index--;
            }

            while (index >= 0 && endOfLine.IndexOf(text[index]) < 0)
            {
                if (text[index] == '\t')
                {
                    character += tabCount;
                }
                else if (ignore.IndexOf(text[index]) < 0)
                {
                    character++;
                }

                index--;
            }

            while (index >= 0)
            {
                if (endOfLine.Contains(text[index]))
                {
                    line++;
                }

                index--;
            }

            return (line, character);
        }

        [GeneratedRegex("\\r\\n?|\\n")]
        private static partial Regex SubstringRegex();

        public static string SubStringNoLineBreaks(this string s, int start, int length, string replacement = "")
        {
            return SubstringRegex().Replace(s.Substring(start, length), replacement);
        }

        public static int SkipCharacters(this string text, int index, string characters = " \t\r\n")
        {
            Contract.RequiresNotNull(text);

            var currentIndex = index;

            if (!string.IsNullOrEmpty(characters) && index >= 0)
            {
                while (currentIndex < text.Length && characters != null && characters.Contains(text[currentIndex]))
                {
                    currentIndex++;
                }
            }

            // xxx should return characters read not the index
            return currentIndex;
        }

        public static string SubstringAround(this string text, int center, int range, string prefixText = "...", string postfixText = "...")
        {
            int endIndex = center + range;
            int startIndex = Math.Max(0, center - range);
            int length = Math.Min(text.Length - startIndex, endIndex - startIndex + 1);

            var prefix = startIndex > 0 ? prefixText : "";
            var postFix = endIndex < text.Length - 1 ? postfixText : "";

            return string.Concat(prefix, text.AsSpan(startIndex, length), postFix);
        }


        //
        // credit Mike Danes
        // https://social.msdn.microsoft.com/Forums/en-US/6b16867a-f21c-4f5f-bcb1-c4858de8a08c/processing-escaped-characters-eg-t-n-that-are-passed-to-an-application?forum=csharpgeneral
        //       
        public static string ReplaceEscapeCharacters(this string span)
        {
            char[] chars = new char[span.Length];
            int input = 0;
            int output = 0;
            bool escape = false;

            while (input < span.Length)
            {
                char c = span[input++];

                if (escape)
                {
                    c = c switch
                    {
                        '\\' => '\\',
                        't' => '\t',
                        'r' => '\r',
                        'n' => '\n',
                        '\"' => '\"',
                        '\'' => '\'',
                        _ => throw new FormatException($"unknown escape character {c}")
                    };
                    chars[output++] = c;
                    escape = false;
                }
                else if (c == '\\')
                {
                    escape = true;
                }
                else
                {
                    chars[output++] = c;
                }
            }

            if (escape)
            {
                // we have a trailing \
                throw new FormatException($"trailing escape found without following character");
            }

            return new string(chars, 0, output);
        }

        // https://stackoverflow.com/questions/3754582/is-there-an-easy-way-to-return-a-string-repeated-x-number-of-times
        public static string Repeat(this string s, int n, string separator = "")
            => new StringBuilder((s.Length + separator.Length)* n).Insert(0, s + separator, n).ToString();
        
    }
}

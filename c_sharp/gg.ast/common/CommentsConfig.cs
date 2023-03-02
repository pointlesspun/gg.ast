/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

using System;

namespace gg.ast.common
{
    /// <summary>
    /// Configuration used to by the CommentsRules, ie rules to parse code comments.
    /// </summary>
    public class CommentsConfig
    {
        public class ConfigTags
        {
            public string CommentStart { get; set; } = "comment start";

            public string NotComment { get; set; } = "not a comment";

            public string SingleLineComment { get; set; } = "single line comment";           
            public string SingleLineCommentBegin { get; set; } = "single line comment begin";
            public string SingleLineCommentCharacters { get; set; } = "single line comment characters";

            public string MultiLineComment { get; set; } = "multi line comment";
            public string MultiLineCommentBegin { get; set; } = "multi line comment begin";
            public string MultiLineCommentEnd { get; set; } = "multi line comment end";

            
            public string DocumentText { get; set; } = "document text";
            public string DocumentCharacters { get; set; } = "document characters";

            public string DocumentParts { get; set; } = "document parts";

            public string Document { get; set; } = "document";
        }

        public class ConfigTokens
        {
            public string SingleLineCommentBegin { get; set; } = "//";

            public string SingleLineCommentEnd { get; set; } = Environment.NewLine;

            public string MultiLineCommentBegin { get; set; } = "/*";

            public string MultiLineCommentEnd { get; set; } = "*/";

            public string CommentStart { get; set; } = "/";
        }

        public ConfigTags Tags { get; private set; } = new ConfigTags();

        public ConfigTokens Tokens { get; private set; } = new ConfigTokens();
    }
}

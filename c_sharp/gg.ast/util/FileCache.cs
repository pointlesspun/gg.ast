using System.Collections.Generic;
using System.IO;

namespace gg.ast.util
{
    public class FileCache
    {
        private static readonly Dictionary<string, string> _fileCache = new();

        public static string LoadTextFile(string filename)
        {
            string text;
            if (_fileCache.TryGetValue(filename, out text))
            {
                return text;
            }

            text = File.ReadAllText(filename);
            _fileCache[filename] = text;
            return text;
        }
    }
}

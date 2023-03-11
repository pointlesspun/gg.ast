/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

namespace gg.ast.core
{
    /// <summary>
    /// Defines a range between min and max.
    /// </summary>
    public interface IRange
    {
        public int Min { get; set; }

        public int Max { get; set; }
    }
}

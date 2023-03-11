/* (c) pointless pun, license: cc attribution 3.0 unported https://creativecommons.org/licenses/by/3.0/ */

namespace gg.ast.core
{
    /// <summary>
    /// A rule containing a single other rule
    /// </summary>
    public interface IMetaRule : IRule
    {
        IRule Subrule { get; set; }
    }
}

// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class Toolset : ClangCommon.Toolset
    {
        protected override string SpecificVersion (Opus.Core.BaseTarget baseTarget)
        {
            return "Apple425";
        }

        protected override string SpecificInstallPath (Opus.Core.BaseTarget baseTarget)
        {
            return @"/usr/bin";
        }
    }
}

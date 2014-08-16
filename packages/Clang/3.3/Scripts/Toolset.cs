// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class Toolset : ClangCommon.Toolset
    {
        protected override string SpecificVersion (Bam.Core.BaseTarget baseTarget)
        {
            return "3.3";
        }

        protected override string SpecificInstallPath (Bam.Core.BaseTarget baseTarget)
        {
            if (Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return @"D:\dev\Thirdparty\Clang\3.3\build\bin\Release";
            }
            else
            {
                return @"/usr/bin";
            }
        }
    }
}

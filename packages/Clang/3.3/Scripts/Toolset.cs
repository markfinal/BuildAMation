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
            return "3.3";
        }

        protected override string SpecificInstallPath (Opus.Core.BaseTarget baseTarget)
        {
            if (Opus.Core.OSUtilities.IsWindowsHosting)
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

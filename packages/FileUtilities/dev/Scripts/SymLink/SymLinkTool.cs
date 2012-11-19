// <copyright file="SymLinkTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    public sealed class SymLinkTool : ISymLinkTool
    {
        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return @"c:\Windows\System32\cmd.exe";
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting || Opus.Core.OSUtilities.IsOSXHosting)
            {
                return "ln";
            }

            throw new Opus.Core.Exception("Unsupported platform for sym links", false);
        }

        #endregion
    }
}

// <copyright file="SymLinkTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("symlinktool", "FileUtilities.SymLinkTool.Version")]

namespace FileUtilities
{
    public sealed class SymLinkTool : Opus.Core.ITool
    {
        public static string Version
        {
            get
            {
                return "dev";
            }
        }

        Opus.Core.StringArray Opus.Core.ITool.EnvironmentPaths(Opus.Core.Target target)
        {
            return null;
        }

        public string Executable(Opus.Core.Target target)
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
    }
}

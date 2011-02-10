// <copyright file="CopyFilesTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("copyfilestool", "FileUtilities.CopyFilesTool.Version")]

namespace FileUtilities
{
    public sealed class CopyFilesTool : Opus.Core.ITool
    {
        public static string Version
        {
            get
            {
                return "dev";
            }
        }

        public Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target)
        {
            return new Opus.Core.StringArray();
        }

        public string Executable(Opus.Core.Target target)
        {
            string executable = null;
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                executable = @"c:\Windows\System32\cmd.exe";
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting)
            {
                executable = "cp";
            }
            return executable;
        }

        public Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get
            {
                return new Opus.Core.StringArray();
            }
        }
    }
}
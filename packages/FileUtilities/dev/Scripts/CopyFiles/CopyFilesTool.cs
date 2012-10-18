// <copyright file="CopyFilesTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
[assembly: Opus.Core.RegisterTargetToolChain("copyfilestool", "FileUtilities.CopyFilesTool.Version")]
[assembly: FileUtilities.CopyFilesRegisterToolchain(typeof(FileUtilities.CopyFilesToolsetInfo))]

namespace FileUtilities
{
    public sealed class CopyFilesToolsetInfo : Opus.Core.IToolsetInfo
    {
        #region IToolsetInfo Members

        string Opus.Core.IToolsetInfo.BinPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        Opus.Core.StringArray Opus.Core.IToolsetInfo.Environment
        {
            get { throw new System.NotImplementedException(); }
        }

        string Opus.Core.IToolsetInfo.InstallPath(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        string Opus.Core.IToolsetInfo.Version(Opus.Core.Target target)
        {
            return "dev";
        }

        #endregion
    }

    [Opus.Core.LocalAndExportTypes(typeof(LocalOptionsDelegateAttribute),
                                   typeof(ExportOptionsDelegateAttribute))]
    public sealed class CopyFilesTool : Opus.Core.ITool
    {
        public static string Version
        {
            get
            {
                return "dev";
            }
        }

        public string Executable(Opus.Core.Target target)
        {
            string executable = null;
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                executable = @"c:\Windows\System32\cmd.exe";
            }
            else if (Opus.Core.OSUtilities.IsUnixHosting || Opus.Core.OSUtilities.IsOSXHosting)
            {
                executable = "cp";
            }
            else
            {
                throw new Opus.Core.Exception("Unsupported platform for CopyFiles", false);
            }
            return executable;
        }
    }
}
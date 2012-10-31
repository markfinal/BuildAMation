// <copyright file="CopyFilesTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
#if false
[assembly: Opus.Core.RegisterTargetToolChain("copyfilestool", "FileUtilities.CopyFilesTool.Version")]
#endif

namespace FileUtilities
{
#if false
    [Opus.Core.LocalAndExportTypes(typeof(LocalOptionsDelegateAttribute),
                                   typeof(ExportOptionsDelegateAttribute))]
#endif
    public sealed class CopyFilesTool : ICopyFilesTool
    {
#if false
        public static string Version
        {
            get
            {
                return "dev";
            }
        }
#endif

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
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

        #endregion
    }
}
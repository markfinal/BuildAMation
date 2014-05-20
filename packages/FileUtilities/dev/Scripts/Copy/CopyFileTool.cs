// <copyright file="CopyFileTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace FileUtilities
{
    class CopyFileTool : ICopyFileTool
    {
        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
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
                throw new Opus.Core.Exception("Unsupported platform for CopyFiles");
            }
            return executable;
        }

        Opus.Core.Array<Opus.Core.LocationKey>
        Opus.Core.ITool.OutputLocationKeys(
            Opus.Core.BaseModule module)
        {
            var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                CopyFile.OutputFile
                );
            return array;
        }

        #endregion
    }
}

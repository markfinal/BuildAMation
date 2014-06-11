// <copyright file="Win32ManifestTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    sealed class Win32ManifestTool :
        C.IWinManifestTool,
        Opus.Core.IToolForwardedEnvironmentVariables
    {
        //private Opus.Core.IToolset toolset;

        public Win32ManifestTool(Opus.Core.IToolset toolset)
        {
            //this.toolset = toolset;
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            // TODO: would like a better way of doing this
            var platformBinFolder = WindowsSDK.WindowsSDK.BinPath(baseTarget);
            return System.IO.Path.Combine(platformBinFolder, "mt.exe");
        }

        Opus.Core.Array<Opus.Core.LocationKey>
        Opus.Core.ITool.OutputLocationKeys(
            Opus.Core.BaseModule module)
        {
            var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                C.Win32Manifest.OutputFile,
                C.Win32Manifest.OutputDir);
            return array;
        }

        #endregion

        #region IToolForwardedEnvironmentVariables Members

        Opus.Core.StringArray Opus.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                var forwardedEnvVars = new Opus.Core.StringArray(
                    "SystemRoot");
                return forwardedEnvVars;
            }
        }

        #endregion
    }
}

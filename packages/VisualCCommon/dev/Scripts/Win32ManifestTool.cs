// <copyright file="Win32ManifestTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    sealed class Win32ManifestTool :
        C.IWinManifestTool,
        Bam.Core.IToolForwardedEnvironmentVariables
    {
        public
        Win32ManifestTool(
            Bam.Core.IToolset toolset)
        {}

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            // TODO: would like a better way of doing this
            var platformBinFolder = WindowsSDK.WindowsSDK.BinPath(baseTarget);
            return System.IO.Path.Combine(platformBinFolder, "mt.exe");
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.Win32Manifest.OutputFile,
                C.Win32Manifest.OutputDir);
            return array;
        }

        #endregion

        #region IToolForwardedEnvironmentVariables Members

        Bam.Core.StringArray Bam.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                var forwardedEnvVars = new Bam.Core.StringArray(
                    "SystemRoot");
                return forwardedEnvVars;
            }
        }

        #endregion
    }
}

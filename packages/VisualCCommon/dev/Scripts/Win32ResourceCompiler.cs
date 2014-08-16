// <copyright file="Win32ResoureCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    sealed class Win32ResourceCompiler :
        C.IWinResourceCompilerTool
    {
        public
        Win32ResourceCompiler(
            Bam.Core.IToolset toolset)
        {}

        #region IWinResourceCompilerTool Members

        string C.IWinResourceCompilerTool.CompiledResourceSuffix
        {
            get
            {
                return ".res";
            }
        }

        string C.IWinResourceCompilerTool.InputFileSwitch
        {
            get
            {
                return string.Empty;
            }
        }

        string C.IWinResourceCompilerTool.OutputFileSwitch
        {
            get
            {
                return "-fo ";
            }
        }

        #endregion

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            // TODO: would like a better way of doing this
            var platformBinFolder = WindowsSDK.WindowsSDK.BinPath(baseTarget);
            return System.IO.Path.Combine(platformBinFolder, "rc.exe");
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.Win32Resource.OutputFile,
                C.Win32Resource.OutputDir
                );
            return array;
        }

        #endregion
    }
}

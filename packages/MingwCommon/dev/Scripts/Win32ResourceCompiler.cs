// <copyright file="Win32ResoureCompiler.cs" company="Mark Final">
//  MingwCommon package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public sealed class Win32ResourceCompiler :
        C.IWinResourceCompilerTool,
        Bam.Core.IToolEnvironmentVariables
    {
        private Bam.Core.IToolset toolset;
        private Bam.Core.StringArray pathEnvironment = new Bam.Core.StringArray();

        public
        Win32ResourceCompiler(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.pathEnvironment.Add(@"c:\windows\system32");
        }

        #region IWinResourceCompilerTool Members

        string C.IWinResourceCompilerTool.CompiledResourceSuffix
        {
            get
            {
                return ".obj";
            }
        }

        string C.IWinResourceCompilerTool.InputFileSwitch
        {
            get
            {
                return "--input=";
            }
        }

        string C.IWinResourceCompilerTool.OutputFileSwitch
        {
            get
            {
                return "--output=";
            }
        }

        #endregion

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var platformBinFolder = this.toolset.BinPath(baseTarget);
            return System.IO.Path.Combine(platformBinFolder, "windres.exe");
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

        #region IToolEnvironmentVariables Members

        System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>
        Bam.Core.IToolEnvironmentVariables.Variables(
            Bam.Core.BaseTarget baseTarget)
        {
            var dictionary = new System.Collections.Generic.Dictionary<string, Bam.Core.StringArray>();
            var paths = new Bam.Core.StringArray();
            paths.AddRange(this.pathEnvironment);
            paths.AddRange(this.toolset.Environment);
            dictionary["PATH"] = paths;
            return dictionary;
        }

        #endregion
    }
}

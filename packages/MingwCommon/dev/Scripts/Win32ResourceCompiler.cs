// <copyright file="Win32ResoureCompiler.cs" company="Mark Final">
//  MingwCommon package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public sealed class Win32ResourceCompiler : C.IWinResourceCompilerTool, Opus.Core.IToolEnvironmentVariables
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray pathEnvironment = new Opus.Core.StringArray();

        public Win32ResourceCompiler(Opus.Core.IToolset toolset)
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

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            string platformBinFolder = this.toolset.BinPath(baseTarget);
            return System.IO.Path.Combine(platformBinFolder, "windres.exe");
        }

        #endregion

        #region IToolEnvironmentVariables Members

        System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> Opus.Core.IToolEnvironmentVariables.Variables(Opus.Core.BaseTarget baseTarget)
        {
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> dictionary = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();
            Opus.Core.StringArray paths = new Opus.Core.StringArray();
            paths.AddRange(this.pathEnvironment);
            paths.AddRange(this.toolset.Environment);
            dictionary["PATH"] = paths;
            return dictionary;
        }

        #endregion
    }
}

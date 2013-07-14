// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public sealed class CxxCompiler : C.ICxxCompilerTool, Opus.Core.IToolSupportsResponseFile, Opus.Core.IToolForwardedEnvironmentVariables, Opus.Core.IToolEnvironmentVariables
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        public CxxCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.requiredEnvironmentVariables.Add("SystemRoot");
        }

        #region ICompilerTool Members

        string C.ICompilerTool.PreprocessedOutputSuffix
        {
            get
            {
                return ".ii";
            }
        }

        string C.ICompilerTool.ObjectFileSuffix
        {
            get
            {
                return ".obj";
            }
        }

        string C.ICompilerTool.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePaths(Opus.Core.BaseTarget baseTarget)
        {
            string installPath = this.toolset.InstallPath(baseTarget);
            Opus.Core.StringArray includePaths = new Opus.Core.StringArray();
            includePaths.Add(System.IO.Path.Combine(installPath, "include"));
            return includePaths;
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-I");
            }
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            string platformBinFolder = this.toolset.BinPath(baseTarget);
            return System.IO.Path.Combine(platformBinFolder, "cl.exe");
        }

        #endregion

        #region IToolSupportsResponseFile Members

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }

        #endregion

        #region IToolForwardedEnvironmentVariables Members

        Opus.Core.StringArray Opus.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        #endregion

        #region IToolEnvironmentVariables Members

        System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> Opus.Core.IToolEnvironmentVariables.Variables(Opus.Core.BaseTarget baseTarget)
        {
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> dictionary = new System.Collections.Generic.Dictionary<string, Opus.Core.StringArray>();
            dictionary["PATH"] = this.toolset.Environment;

            var compilerTool = this as C.ICompilerTool;
            dictionary["INCLUDE"] = compilerTool.IncludePaths (baseTarget);

            return dictionary;
        }

        #endregion
    }
}

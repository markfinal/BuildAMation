// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    // NEW STYLE
#if true
    public abstract class CCompiler : C.ICompilerTool, Opus.Core.IToolRequiredEnvironmentVariables, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        protected CCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.requiredEnvironmentVariables.Add("TEMP");
        }

        protected abstract string Filename
        {
            get;
        }

        #region ICompilerTool Members

        string C.ICompilerTool.PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        string C.ICompilerTool.ObjectFileSuffix
        {
            get
            {
                return ".o";
            }
        }

        string C.ICompilerTool.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePaths(Opus.Core.Target target)
        {
            // TODO: sort this out... it required a call to the InstallPath to get the right paths
            this.toolset.InstallPath((Opus.Core.BaseTarget)target);
            return (this.toolset as MingwCommon.Toolset).includePaths;
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-isystem", "-I");
            }
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            string installPath = target.Toolset.BinPath((Opus.Core.BaseTarget)target);
            string executablePath = System.IO.Path.Combine(installPath, this.Filename);
            return executablePath;
        }

        #endregion

        #region IToolRequiredEnvironmentVariables Members

        Opus.Core.StringArray Opus.Core.IToolRequiredEnvironmentVariables.VariableNames
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        #endregion

        #region IToolEnvironmentPaths Members

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            return this.toolset.Environment;
        }

        #endregion
    }
#else
    public abstract class CCompiler : C.Compiler, Opus.Core.ITool
    {
        public abstract string Executable(Opus.Core.Target target);

        protected Opus.Core.StringArray CommonIncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-isystem", "-I");
            }
        }
    }
#endif
}
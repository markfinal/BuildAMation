// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public abstract class CxxCompiler : C.ICxxCompilerTool
    {
        private Opus.Core.IToolset toolset;

        protected CxxCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        protected abstract string Filename
        {
            get;
        }

        #region ICompilerTool implementation
        Opus.Core.StringArray C.ICompilerTool.IncludePaths (Opus.Core.Target target)
        {
            // TODO: sort this out... it required a call to the InstallPath to get the right paths
            this.toolset.InstallPath((Opus.Core.BaseTarget)target);
            return (this.toolset as Toolset).GccDetail.IncludePaths;
        }

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

        Opus.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-isystem", "-I");
            }
        }
        #endregion

        #region ITool implementation
        string Opus.Core.ITool.Executable (Opus.Core.Target target)
        {
            string installPath = target.Toolset.BinPath((Opus.Core.BaseTarget)target);
            string executablePath = System.IO.Path.Combine(installPath, this.Filename);
            return executablePath;
        }
        #endregion
    }
}


// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public class CCompiler : C.ICompilerTool
    {
        private Opus.Core.IToolset toolset;

        public CCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ICompilerTool implementation
        Opus.Core.StringArray C.ICompilerTool.IncludePaths(Opus.Core.BaseTarget baseTarget)
        {
            // TODO: sort this out... it required a call to the InstallPath to get the right paths
            this.toolset.InstallPath(baseTarget);
            return (this.toolset as Toolset).GccDetail.IncludePaths;
        }

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

        Opus.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-isystem", "-I");
            }
        }
        #endregion

        #region ITool implementation
        string Opus.Core.ITool.Executable (Opus.Core.BaseTarget baseTarget)
        {
            string installPath = this.toolset.BinPath(baseTarget);
            string executablePath = System.IO.Path.Combine(installPath, "icc");
            return executablePath;
        }
        #endregion
    }
}


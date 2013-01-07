// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class CxxCompiler : C.ICxxCompilerTool
    {
        private Opus.Core.IToolset toolset;

        public CxxCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
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
            return new Opus.Core.StringArray();
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
            string executablePath = System.IO.Path.Combine(this.toolset.InstallPath(baseTarget), "clang++");
            if (baseTarget.HasPlatform(Opus.Core.EPlatform.Windows))
            {
                // TODO: can we have this file extension somewhere central?
                executablePath += ".exe";
            }
            return executablePath;
        }

        #endregion
    }
}

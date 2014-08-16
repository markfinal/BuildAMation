// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ClangCommon package</summary>
// <author>Mark Final</author>
namespace ClangCommon
{
    public sealed class CxxCompiler :
        C.ICxxCompilerTool
    {
        private Bam.Core.IToolset toolset;

        public
        CxxCompiler(
            Bam.Core.IToolset toolset)
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

        Bam.Core.StringArray
        C.ICompilerTool.IncludePaths(
            Bam.Core.BaseTarget baseTarget)
        {
            return new Bam.Core.StringArray();
        }

        Bam.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Bam.Core.StringArray("-I");
            }
        }

        #endregion

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var executablePath = System.IO.Path.Combine(this.toolset.InstallPath(baseTarget), "clang++");
            if (baseTarget.HasPlatform(Bam.Core.EPlatform.Windows))
            {
                // TODO: can we have this file extension somewhere central?
                executablePath += ".exe";
            }
            return executablePath;
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.ObjectFile.OutputFile,
                C.ObjectFile.OutputDir
                );
            return array;
        }

        #endregion
    }
}

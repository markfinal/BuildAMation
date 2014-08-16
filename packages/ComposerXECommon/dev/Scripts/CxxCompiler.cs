// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXECommon package</summary>
// <author>Mark Final</author>
namespace ComposerXECommon
{
    public class CxxCompiler :
        C.ICxxCompilerTool
    {
        private Bam.Core.IToolset toolset;

        public
        CxxCompiler(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region ICompilerTool implementation
        Bam.Core.StringArray
        C.ICompilerTool.IncludePaths(
            Bam.Core.BaseTarget baseTarget)
        {
            // TODO: sort this out... it required a call to the InstallPath to get the right paths
            this.toolset.InstallPath(baseTarget);
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

        Bam.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Bam.Core.StringArray("-isystem", "-I");
            }
        }
        #endregion

        #region ITool implementation
        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var installPath = this.toolset.BinPath(baseTarget);
            var executablePath = System.IO.Path.Combine(installPath, "icpc");
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

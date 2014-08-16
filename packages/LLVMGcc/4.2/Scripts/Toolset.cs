// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>LLVMGcc package</summary>
// <author>Mark Final</author>
namespace LLVMGcc
{
    public sealed class Toolset :
        GccCommon.Toolset,
        XcodeBuilder.IXcodeDetails
    {
        public
        Toolset()
        {
            this.toolConfig[typeof(C.ICompilerTool)]       = new Bam.Core.ToolAndOptionType(new CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)]    = new Bam.Core.ToolAndOptionType(new CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.IObjCCompilerTool)]   = new Bam.Core.ToolAndOptionType(new CCompiler(this), typeof(ObjCCompilerOptionCollection));
            this.toolConfig[typeof(C.IObjCxxCompilerTool)] = new Bam.Core.ToolAndOptionType(new CxxCompiler(this), typeof(ObjCxxCompilerOptionCollection));
            this.toolConfig[typeof(C.ILinkerTool)]         = new Bam.Core.ToolAndOptionType(new Linker(this), typeof(LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)]       = new Bam.Core.ToolAndOptionType(new GccCommon.Archiver(this), typeof(ArchiverOptionCollection));
        }

        #region IXcodeDetails implementation

        XcodeBuilder.EXcodeVersion XcodeBuilder.IXcodeDetails.SupportedVersion
        {
            get
            {
                return XcodeBuilder.EXcodeVersion.V4dot6;
            }
        }

        #endregion
    }
}

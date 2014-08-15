// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class Toolset :
        ClangCommon.Toolset,
        XcodeBuilder.IXcodeDetails
    {
        public
        Toolset() : base()
        {
            this.toolConfig[typeof(C.ICompilerTool)]       = new Opus.Core.ToolAndOptionType(new ClangCommon.CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)]    = new Opus.Core.ToolAndOptionType(new ClangCommon.CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.IObjCCompilerTool)]   = new Opus.Core.ToolAndOptionType(new ClangCommon.CCompiler(this), typeof(ClangCommon.ObjCCompilerOptionCollection));
            this.toolConfig[typeof(C.IObjCxxCompilerTool)] = new Opus.Core.ToolAndOptionType(new ClangCommon.CxxCompiler(this), typeof(ClangCommon.ObjCxxCompilerOptionCollection));
            this.toolConfig[typeof(C.ILinkerTool)]         = new Opus.Core.ToolAndOptionType(new Linker(this), typeof(GccCommon.LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)]       = new Opus.Core.ToolAndOptionType(new GccCommon.Archiver(this), typeof(GccCommon.ArchiverOptionCollection));
            this.toolConfig[typeof(C.IPosixSharedLibrarySymlinksTool)] =
                new Opus.Core.ToolAndOptionType(new GccCommon.PosixSharedLibrarySymlinksTool(this), typeof(GccCommon.PosixSharedLibrarySymlinksOptionCollection));
        }

        protected override string
        SpecificVersion(
            Opus.Core.BaseTarget baseTarget)
        {
            return "Apple425";
        }

        protected override string
        SpecificInstallPath(
            Opus.Core.BaseTarget baseTarget)
        {
            return @"/usr/bin";
        }

        #region IXcodeDetails implementation

        XcodeBuilder.EXcodeVersion XcodeBuilder.IXcodeDetails.SupportedVersion
        {
            get
            {
                return XcodeBuilder.EXcodeVersion.V4;
            }
        }

        #endregion
    }
}

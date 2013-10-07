// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Clang package</summary>
// <author>Mark Final</author>
namespace Clang
{
    public sealed class Toolset : ClangCommon.Toolset
    {
        public Toolset()
            : base()
        {
            this.toolConfig[typeof(C.ILinkerTool)]   = new Opus.Core.ToolAndOptionType(new Gcc.Linker(this), typeof(Gcc.LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)] = new Opus.Core.ToolAndOptionType(new GccCommon.Archiver(this), typeof(Gcc.ArchiverOptionCollection));
        }

        protected override string SpecificVersion (Opus.Core.BaseTarget baseTarget)
        {
            return "Apple425";
        }

        protected override string SpecificInstallPath (Opus.Core.BaseTarget baseTarget)
        {
            return @"/usr/bin";
        }
    }
}

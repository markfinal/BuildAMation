// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public sealed class Toolset :
        GccCommon.Toolset
    {
        public
        Toolset()
        {
            this.toolConfig[typeof(C.ICompilerTool)] = new Bam.Core.ToolAndOptionType(new CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)] = new Bam.Core.ToolAndOptionType(new CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.ILinkerTool)] = new Bam.Core.ToolAndOptionType(new Linker(this), typeof(LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)] = new Bam.Core.ToolAndOptionType(new GccCommon.Archiver(this), typeof(ArchiverOptionCollection));
        }
    }
}

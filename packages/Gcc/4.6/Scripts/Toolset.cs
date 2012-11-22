// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public sealed class Toolset : GccCommon.Toolset
    {
        public Toolset()
        {
            this.toolMap[typeof(C.ICompilerTool)] = new CCompiler(this);
            this.toolMap[typeof(C.ICxxCompilerTool)] = new CxxCompiler(this);
            this.toolMap[typeof(C.ILinkerTool)] = new Linker(this);
            this.toolMap[typeof(C.IArchiverTool)] = new GccCommon.Archiver(this);

            this.toolOptionsMap[typeof(C.ICompilerTool)] = typeof(Gcc.CCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ICxxCompilerTool)] = typeof(Gcc.CxxCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ILinkerTool)] = typeof(Gcc.LinkerOptionCollection);
            this.toolOptionsMap[typeof(C.IArchiverTool)] = typeof(Gcc.ArchiverOptionCollection);
        }
    }
}

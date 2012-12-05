// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
namespace ComposerXE
{
    public sealed class Toolset : ComposerXECommon.Toolset
    {
        public Toolset()
        {
            this.toolMap[typeof(C.ICompilerTool)] = new ComposerXECommon.CCompiler(this);
            this.toolMap[typeof(C.ICxxCompilerTool)] = new ComposerXECommon.CxxCompiler(this);

            this.toolOptionsMap[typeof(C.ICompilerTool)] = typeof(CCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ICxxCompilerTool)] = typeof(CxxCompilerOptionCollection);
        }

        protected override string Version
        {
            get
            {
                return "12";
            }
        }
    }
}

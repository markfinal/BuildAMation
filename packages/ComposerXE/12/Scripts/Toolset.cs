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
            this.toolConfig[typeof(C.ICompilerTool)] = new Opus.Core.ToolAndOptionType(new ComposerXECommon.CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)] = new Opus.Core.ToolAndOptionType(new ComposerXECommon.CxxCompiler(this), typeof(CxxCompilerOptionCollection));
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

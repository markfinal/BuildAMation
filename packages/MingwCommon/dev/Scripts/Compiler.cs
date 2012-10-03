// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public abstract class CCompiler : C.Compiler, Opus.Core.ITool
    {
        public abstract string Executable(Opus.Core.Target target);

        protected Opus.Core.StringArray CommonIncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-isystem", "-I");
            }
        }
    }
}
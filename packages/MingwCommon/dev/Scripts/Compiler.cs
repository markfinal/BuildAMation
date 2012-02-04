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

        public override Opus.Core.StringArray IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-isystem", "-I");
            }
        }
    }
}
// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    // Not sealed since the C++ compiler inherits from it
    public abstract class CCompiler : C.Compiler, Opus.Core.ITool
    {
        public abstract string Executable(Opus.Core.Target target);

        public abstract Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get;
        }

        public abstract Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target);
    }
}
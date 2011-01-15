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

        public abstract Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get;
        }

        public abstract Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target);
    }
}
// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ExportCompilerOptionsDelegateAttribute : System.Attribute
    {
    }

    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class LocalCompilerOptionsDelegateAttribute : System.Attribute
    {
    }

    public abstract class Compiler
    {
        public abstract string ExecutableCPlusPlus(Opus.Core.Target target);

        public abstract Opus.Core.StringArray IncludeDirectoryPaths(Opus.Core.Target target);
    }
}
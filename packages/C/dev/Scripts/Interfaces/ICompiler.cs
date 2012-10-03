// <copyright file="ICompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface ICompiler
    {
        Opus.Core.StringArray IncludeDirectoryPaths(Opus.Core.Target target);

        Opus.Core.StringArray IncludePathCompilerSwitches
        {
            get;
        }
    }
}
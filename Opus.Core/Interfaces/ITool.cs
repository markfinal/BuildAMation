// <copyright file="ITool.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface ITool
    {
        string Executable(Target target);

        StringArray RequiredEnvironmentVariables
        {
            get;
        }

        StringArray EnvironmentPaths(Target target);
    }
}
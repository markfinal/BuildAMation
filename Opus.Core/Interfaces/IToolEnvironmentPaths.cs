// <copyright file="IToolEnvironmentPaths.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IToolEnvironmentPaths
    {
        StringArray Paths(Target target);
    }
}
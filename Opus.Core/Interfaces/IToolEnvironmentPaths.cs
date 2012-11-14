// <copyright file="IToolEnvironmentPaths.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    // TODO: deprecate this interface in favour of IToolEnvironmentVariables
    public interface IToolEnvironmentPaths
    {
        StringArray Paths(Target target);
    }
}
// <copyright file="IOutputPaths.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IOutputPaths
    {
        // TOOD: does this require a Target
        System.Collections.Generic.Dictionary<string, string> GetOutputPaths();
    }
}
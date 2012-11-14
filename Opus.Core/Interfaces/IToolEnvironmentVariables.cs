// <copyright file="IToolEnvironmentVariables.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IToolEnvironmentVariables
    {
        // TODO: change to BaseTarget
        System.Collections.Generic.Dictionary<string, StringArray> Variables(Opus.Core.Target Target);
    }
}
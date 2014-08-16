// <copyright file="IToolEnvironmentVariables.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface IToolEnvironmentVariables
    {
        System.Collections.Generic.Dictionary<string, StringArray>
        Variables(
            Opus.Core.BaseTarget baseTarget);
    }
}

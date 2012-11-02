// <copyright file="IToolRequiredEnvironmentVariables.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    // TODO: rename to IToolForwardedEnvironmentVariables
    public interface IToolRequiredEnvironmentVariables
    {
        StringArray VariableNames
        {
            get;
        }
    }
}
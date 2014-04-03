// <copyright file="IToolForwardedEnvironmentVariables.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IToolForwardedEnvironmentVariables
    {
        StringArray VariableNames
        {
            get;
        }
    }
}
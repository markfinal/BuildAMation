// <copyright file="IAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IAction
    {
        string CommandLineSwitch
        {
            get;
        }

        string Description
        {
            get;
        }

        bool Execute();
    }
}
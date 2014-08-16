// <copyright file="IAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface IAction :
        System.ICloneable
    {
        string CommandLineSwitch
        {
            get;
        }

        string Description
        {
            get;
        }

        bool
        Execute();
    }
}

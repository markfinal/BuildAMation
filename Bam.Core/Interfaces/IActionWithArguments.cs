// <copyright file="IActionWithArguments.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface IActionWithArguments :
        IAction
    {
        void
        AssignArguments(
            string arguments);
    }
}

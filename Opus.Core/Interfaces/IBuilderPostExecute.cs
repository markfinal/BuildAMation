// <copyright file="IBuilderPostExecute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface IBuilderPostExecute
    {
        void PostExecute(Opus.Core.DependencyNodeCollection executedNodes);
    }
}

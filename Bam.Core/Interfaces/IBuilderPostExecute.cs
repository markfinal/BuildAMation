// <copyright file="IBuilderPostExecute.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public interface IBuilderPostExecute
    {
        void
        PostExecute(
            DependencyNodeCollection executedNodes);
    }
}

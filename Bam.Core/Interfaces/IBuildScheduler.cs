// <copyright file="IBuildScheduler.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public delegate void
    BuildSchedulerProgressUpdatedDelegate(
        int percentageComplete);

    public interface IBuildScheduler
    {
        bool AreNodesAvailable
        {
            get;
        }

        DependencyNode
        GetNextNodeToBuild();

        event BuildSchedulerProgressUpdatedDelegate ProgressUpdated;
    }
}

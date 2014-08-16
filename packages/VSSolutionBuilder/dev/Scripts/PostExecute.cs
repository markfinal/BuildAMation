// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder :
        Bam.Core.IBuilderPostExecute
    {
        #region IBuilderPostExecute Members

        void
        Bam.Core.IBuilderPostExecute.PostExecute(
            Bam.Core.DependencyNodeCollection executedNodes)
        {
            Bam.Core.Log.DebugMessage("PostExecute for VSSolutionBuilder");

            this.solutionFile.ResolveSourceFileConfigurationExclusions();
            this.solutionFile.Serialize();
        }

        #endregion
    }
}

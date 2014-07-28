// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder :
        Opus.Core.IBuilderPostExecute
    {
        #region IBuilderPostExecute Members

        void
        Opus.Core.IBuilderPostExecute.PostExecute(
            Opus.Core.DependencyNodeCollection executedNodes)
        {
            Opus.Core.Log.DebugMessage("PostExecute for VSSolutionBuilder");

            this.solutionFile.ResolveSourceFileConfigurationExclusions();
            this.solutionFile.Serialize();
        }

        #endregion
    }
}

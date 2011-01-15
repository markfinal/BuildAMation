// <copyright file="PostExecute.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public void PostExecute(System.Collections.Generic.List<object> dataList)
        {
            Opus.Core.Log.DebugMessage("PostExecute for VSSolutionBuilder");

            this.solutionFile.ResolveSourceFileConfigurationExclusions();
            this.solutionFile.Serialize();
        }
    }
}
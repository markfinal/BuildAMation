// <copyright file="CopyFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(FileUtilities.CopyFileCollection moduleToBuild, out bool success)
        {
            Opus.Core.Log.MessageAll("TODO: Stub function for VSSolution support for {0}", moduleToBuild);
            success = false;
            return null;
        }
    }
}

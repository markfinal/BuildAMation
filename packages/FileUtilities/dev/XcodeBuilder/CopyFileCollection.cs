// <copyright file="CopyFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(FileUtilities.CopyFileCollection moduleToBuild, out bool success)
        {
            success = true;
            return null;
        }
    }
}

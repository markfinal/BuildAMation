// <copyright file="SymlinkFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
        public object Build(FileUtilities.SymlinkFile moduleToBuild, out bool success)
        {
            success = true;
            return null;
        }
    }
}

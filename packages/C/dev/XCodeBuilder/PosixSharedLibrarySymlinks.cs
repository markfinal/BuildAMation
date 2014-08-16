// <copyright file="PosixSharedLibrarySymlinks.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        [Bam.Core.EmptyBuildFunction]
        public object
        Build(
            C.PosixSharedLibrarySymlinks moduleToBuild,
            out bool success)
        {
            success = true;
            return null;
        }
    }
}

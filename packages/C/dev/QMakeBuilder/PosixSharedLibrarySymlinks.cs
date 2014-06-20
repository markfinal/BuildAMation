// <copyright file="PosixSharedLibrarySymlinks.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        [Opus.Core.EmptyBuildFunction]
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

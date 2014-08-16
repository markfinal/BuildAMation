// <copyright file="PosixSharedLibrarySymlinksTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    sealed class PosixSharedLibrarySymlinksTool :
        C.IPosixSharedLibrarySymlinksTool
    {
        public
        PosixSharedLibrarySymlinksTool(
            Bam.Core.IToolset toolset)
        {}

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            return "ln";
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.PosixSharedLibrarySymlinks.OutputDir,
                C.PosixSharedLibrarySymlinks.LinkerSymlink,
                C.PosixSharedLibrarySymlinks.MajorVersionSymlink,
                C.PosixSharedLibrarySymlinks.MinorVersionSymlink);
            return array;
        }

        #endregion
    }
}

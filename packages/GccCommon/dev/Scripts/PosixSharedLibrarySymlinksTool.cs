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
        //private Opus.Core.IToolset toolset;

        public PosixSharedLibrarySymlinksTool(Opus.Core.IToolset toolset)
        {
            //this.toolset = toolset;
        }

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.BaseTarget baseTarget)
        {
            return "ln";
        }

        Opus.Core.Array<Opus.Core.LocationKey>
        Opus.Core.ITool.OutputLocationKeys(
            Opus.Core.BaseModule module)
        {
            var array = new Opus.Core.Array<Opus.Core.LocationKey>(
                C.PosixSharedLibrarySymlinks.OutputDir,
                C.PosixSharedLibrarySymlinks.LinkerSymlink,
                C.PosixSharedLibrarySymlinks.MajorVersionSymlink,
                C.PosixSharedLibrarySymlinks.MinorVersionSymlink);
            return array;
        }

        #endregion
    }
}

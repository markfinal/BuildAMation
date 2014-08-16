// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public sealed class Archiver :
        C.IArchiverTool
    {
        private Bam.Core.IToolset toolset;

        public
        Archiver(
            Bam.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        #region IArchiverTool Members

        string C.IArchiverTool.StaticLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        string C.IArchiverTool.StaticLibrarySuffix
        {
            get
            {
                return ".a";
            }
        }

        string C.IArchiverTool.StaticLibraryOutputSubDirectory
        {
            get
            {
                return "lib";
            }
        }

        #endregion

        #region ITool Members

        string
        Bam.Core.ITool.Executable(
            Bam.Core.BaseTarget baseTarget)
        {
            var installPath = this.toolset.BinPath(baseTarget);
            var executablePath = System.IO.Path.Combine(installPath, "ar");
            return executablePath;
        }

        Bam.Core.Array<Bam.Core.LocationKey>
        Bam.Core.ITool.OutputLocationKeys(
            Bam.Core.BaseModule module)
        {
            var array = new Bam.Core.Array<Bam.Core.LocationKey>(
                C.StaticLibrary.OutputFileLocKey,
                C.StaticLibrary.OutputDirLocKey
                );
            return array;
        }

        #endregion
    }
}

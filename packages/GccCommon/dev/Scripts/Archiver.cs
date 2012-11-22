// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public sealed class Archiver : C.IArchiverTool
    {
        //private Opus.Core.IToolset toolset;

        public Archiver(Opus.Core.IToolset toolset)
        {
            //this.toolset = toolset;
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

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            string installPath = target.Toolset.BinPath((Opus.Core.BaseTarget)target);
            string executablePath = System.IO.Path.Combine(installPath, "ar");
            return executablePath;
        }

        #endregion
    }
}


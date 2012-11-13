// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    // NEW STYLE
#if true
    public abstract class Linker : C.ILinkerTool
    {
        protected Opus.Core.IToolset toolset;

        protected Linker(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
        }

        protected abstract string Filename
        {
            get;
        }

        #region ILinkerTool Members

        string C.ILinkerTool.ExecutableSuffix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.MapFileSuffix
        {
            get
            {
                return ".map";
            }
        }

        string C.ILinkerTool.StartLibraryList
        {
            get
            {
                return "-Wl,--start-group";
            }
        }

        string C.ILinkerTool.EndLibraryList
        {
            get
            {
                return "-Wl,--end-group";
            }
        }

        string C.ILinkerTool.ImportLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        string C.ILinkerTool.ImportLibrarySuffix
        {
            get
            {
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return ".so";
                }
                else if (Opus.Core.OSUtilities.IsOSXHosting)
                {
                    return ".dylib";
                }
                else
                {
                    return null;
                }
            }
        }

        string C.ILinkerTool.DynamicLibraryPrefix
        {
            get
            {
                return "lib";
            }
        }

        string C.ILinkerTool.DynamicLibrarySuffix
        {
            get
            {
                if (Opus.Core.OSUtilities.IsUnixHosting)
                {
                    return ".so";
                }
                else if (Opus.Core.OSUtilities.IsOSXHosting)
                {
                    return ".dylib";
                }
                else
                {
                    return null;
                }
            }
        }

        string C.ILinkerTool.ImportLibrarySubDirectory
        {
            get
            {
                return "lib";
            }
        }

        string C.ILinkerTool.BinaryOutputSubDirectory
        {
            get
            {
                return "bin";
            }
        }

        Opus.Core.StringArray C.ILinkerTool.LibPaths(Opus.Core.Target target)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            string installPath = target.Toolset.BinPath((Opus.Core.BaseTarget)target);
            string executablePath = System.IO.Path.Combine(installPath, this.Filename);
            return executablePath;
        }

        #endregion
    }
#else
    public abstract class Linker : C.Linker, Opus.Core.ITool
    {
        public abstract string Executable(Opus.Core.Target target);

        protected override string StartLibraryList
        {
            get
            {
                return "-Wl,--start-group";
            }
        }

        protected override string EndLibraryList
        {
            get
            {
                return "-Wl,--end-group";
            }
        }
    }
#endif
}


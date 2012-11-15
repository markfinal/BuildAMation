// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    // NEW STYLE
#if true
    public abstract class Linker : C.ILinkerTool, Opus.Core.IToolForwardedEnvironmentVariables, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        protected Linker(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.requiredEnvironmentVariables.Add("TEMP");
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
                return ".a";
            }
        }

        string C.ILinkerTool.DynamicLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.DynamicLibrarySuffix
        {
            get
            {
                return ".dll";
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

        #region IToolForwardedEnvironmentVariables Members

        Opus.Core.StringArray Opus.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        #endregion

        #region IToolEnvironmentPaths Members

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            return this.toolset.Environment;
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
// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    // NEW STYLE
#if true
    public sealed class Archiver : C.IArchiverTool, Opus.Core.IToolSupportsResponseFile, Opus.Core.IToolForwardedEnvironmentVariables, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        public Archiver(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.requiredEnvironmentVariables.Add("SystemRoot");
        }

        #region IArchiverTool Members

        string C.IArchiverTool.StaticLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.IArchiverTool.StaticLibrarySuffix
        {
            get
            {
                return ".lib";
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
            string binPath = this.toolset.BinPath((Opus.Core.BaseTarget)target);
            return System.IO.Path.Combine(binPath, "lib.exe");
        }

        #endregion

        #region IToolSupportsResponseFile Members

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
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
    public sealed class Archiver : C.Archiver, Opus.Core.ITool, Opus.Core.IToolSupportsResponseFile, Opus.Core.IToolForwardedEnvironmentVariables, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private string platformBinFolder;

        public Archiver(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindows(target))
            {
                throw new Opus.Core.Exception("VisualC compiler supports only win32 and win64");
            }

            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(VisualC.Toolset));
            this.platformBinFolder = info.BinPath((Opus.Core.BaseTarget)target);
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.platformBinFolder = toolChainInstance.BinPath(target);
#endif

            this.requiredEnvironmentVariables.Add("SystemRoot");
        }

        public string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.platformBinFolder, "lib.exe");
        }

        Opus.Core.StringArray Opus.Core.IToolForwardedEnvironmentVariables.VariableNames
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(VisualC.Toolset));
            return info.Environment;
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
#endif
        }

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }
    }
#endif
}
// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
#if true
    public sealed class Linker : C.ILinkerTool, Opus.Core.IToolSupportsResponseFile, Opus.Core.IToolRequiredEnvironmentVariables, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        public Linker(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.requiredEnvironmentVariables.Add("TEMP");
            this.requiredEnvironmentVariables.Add("TMP");
        }

        #region ILinkerTool Members

        string C.ILinkerTool.ExecutableSuffix
        {
            get
            {
                return ".exe";
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
                return string.Empty;
            }
        }

        string C.ILinkerTool.EndLibraryList
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.ImportLibraryPrefix
        {
            get
            {
                return string.Empty;
            }
        }

        string C.ILinkerTool.ImportLibrarySuffix
        {
            get
            {
                return ".lib";
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
            if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                return (this.toolset as VisualCCommon.Toolset).lib64Folder;
            }
            else
            {
                return (this.toolset as VisualCCommon.Toolset).lib32Folder;
            }
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            VisualCCommon.Toolset toolset = this.toolset as VisualCCommon.Toolset;
            if (target.HasPlatform(Opus.Core.EPlatform.Win64))
            {
                if (Opus.Core.OSUtilities.Is64BitHosting)
                {
                    return System.IO.Path.Combine(toolset.bin64Folder, "link.exe");
                }
                else
                {
                    return System.IO.Path.Combine(toolset.bin6432Folder, "link.exe");
                }
            }
            else
            {
                return System.IO.Path.Combine(toolset.bin32Folder, "link.exe");
            }
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

        #region IToolRequiredEnvironmentVariables Members

        Opus.Core.StringArray Opus.Core.IToolRequiredEnvironmentVariables.VariableNames
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
            return target.Toolset.Environment;
        }

        #endregion
    }
#else
    [Opus.Core.AssignOptionCollection(typeof(LinkerOptionCollection))]
    public sealed class Linker : C.Linker, Opus.Core.ITool, Opus.Core.IToolSupportsResponseFile, Opus.Core.IToolRequiredEnvironmentVariables, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private string platformBinFolder;

        public Linker(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindows(target))
            {
                throw new Opus.Core.Exception("VisualC linker supports only win32 and win64");
            }

            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(VisualC.Toolset));
            this.platformBinFolder = info.BinPath((Opus.Core.BaseTarget)target);
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.platformBinFolder = toolChainInstance.BinPath(target);
#endif

            this.requiredEnvironmentVariables.Add("TEMP");
            this.requiredEnvironmentVariables.Add("TMP");
        }

        public string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.platformBinFolder, "link.exe");
        }

        // OLD STYLE
#if false
        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return this.Executable(target);
        }
#endif

        Opus.Core.StringArray Opus.Core.IToolRequiredEnvironmentVariables.VariableNames
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

        protected override string StartLibraryList
        {
            get
            {
                return "";
            }
        }

        protected override string EndLibraryList
        {
            get
            {
                return "";
            }
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
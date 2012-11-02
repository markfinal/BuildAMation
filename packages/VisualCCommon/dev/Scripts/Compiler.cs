// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    // NEW STYLE
#if true
    public sealed class CCompiler : C.ICompilerTool, Opus.Core.IToolSupportsResponseFile, Opus.Core.IToolRequiredEnvironmentVariables, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        public CCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.requiredEnvironmentVariables.Add("SystemRoot");
        }

        #region ICompilerTool Members

        string C.ICompilerTool.PreprocessedOutputSuffix
        {
            get
            {
                return ".i";
            }
        }

        string C.ICompilerTool.ObjectFileSuffix
        {
            get
            {
                return ".obj";
            }
        }

        string C.ICompilerTool.ObjectFileOutputSubDirectory
        {
            get
            {
                return "obj";
            }
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePaths(Opus.Core.Target target)
        {
            string installPath = this.toolset.InstallPath((Opus.Core.BaseTarget)target);
            Opus.Core.StringArray includePaths = new Opus.Core.StringArray();
            includePaths.Add(System.IO.Path.Combine(installPath, "include"));
            return includePaths;
        }

        Opus.Core.StringArray C.ICompilerTool.IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-I");
            }
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            string platformBinFolder = this.toolset.BinPath((Opus.Core.BaseTarget)target);
            return System.IO.Path.Combine(platformBinFolder, "cl.exe");
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
            return this.toolset.Environment;
        }

        #endregion
    }
#else
    [Opus.Core.AssignOptionCollection(typeof(CCompilerOptionCollection))]
    public sealed class CCompiler : C.Compiler, Opus.Core.ITool, Opus.Core.IToolSupportsResponseFile, Opus.Core.IToolRequiredEnvironmentVariables, Opus.Core.IToolEnvironmentPaths, C.ICompiler
    {
        private Opus.Core.StringArray includeFolder = new Opus.Core.StringArray();
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        private string platformBinFolder;

        public CCompiler(Opus.Core.BaseTarget baseTarget)
        {
            if (!Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            if (!Opus.Core.OSUtilities.IsWindows(baseTarget))
            {
                throw new Opus.Core.Exception("VisualC compiler supports only win32 and win64");
            }

            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(VisualC.Toolset));
            this.platformBinFolder = info.BinPath(baseTarget);

            string installPath = info.InstallPath(baseTarget);
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.platformBinFolder = toolChainInstance.BinPath(target);

            string installPath = toolChainInstance.InstallPath(target);
#endif
            this.includeFolder.Add(System.IO.Path.Combine(installPath, "include"));

            this.requiredEnvironmentVariables.Add("SystemRoot");
        }

        public string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(platformBinFolder, "cl.exe");
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

        // NEW STYLE
#if true
        Opus.Core.StringArray C.ICompiler.IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return this.includeFolder;
        }

        Opus.Core.StringArray C.ICompiler.IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-I");
            }
        }
#else
        public override Opus.Core.StringArray IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return this.includeFolder;
        }

        public override Opus.Core.StringArray IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-I");
            }
        }
#endif

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
// <copyright file="Win32ResoureCompiler.cs" company="Mark Final">
//  MingwCommon package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    // NEW STYLE
#if true
    public sealed class Win32ResourceCompiler : C.IWinResourceCompilerTool, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.IToolset toolset;
        private Opus.Core.StringArray pathEnvironment = new Opus.Core.StringArray();

        public Win32ResourceCompiler(Opus.Core.IToolset toolset)
        {
            this.toolset = toolset;
            this.pathEnvironment.Add(@"c:\windows\system32");
            this.pathEnvironment.AddRange(toolset.Environment);
        }

        #region IWinResourceCompilerTool Members

        string C.IWinResourceCompilerTool.CompiledResourceSuffix
        {
            get
            {
                return ".obj";
            }
        }

        string C.IWinResourceCompilerTool.InputFileSwitch
        {
            get
            {
                return "--input=";
            }
        }

        string C.IWinResourceCompilerTool.OutputFileSwitch
        {
            get
            {
                return "--output=";
            }
        }

        #endregion

        #region ITool Members

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            string platformBinFolder = this.toolset.BinPath((Opus.Core.BaseTarget)target);
            return System.IO.Path.Combine(platformBinFolder, "windres.exe");
        }

        #endregion

        #region IToolEnvironmentPaths Members

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            return this.pathEnvironment;
        }

        #endregion
    }
#else
    sealed class Win32ResourceCompiler : C.Win32ResourceCompilerBase, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.StringArray pathEnvironment = new Opus.Core.StringArray();
        private string platformBinFolder;

        public Win32ResourceCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Mingw.Toolset));
            this.platformBinFolder = info.BinPath((Opus.Core.BaseTarget)target);
            this.pathEnvironment.AddRange(info.Environment);
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.platformBinFolder = toolChainInstance.BinPath(target);
            this.pathEnvironment.AddRange(toolChainInstance.Environment);
#endif
            this.pathEnvironment.Add(@"c:\windows\system32");
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.platformBinFolder, "windres.exe");
        }

        public override string InputFileSwitch
        {
            get
            {
                return "--input=";
            }
        }

        public override string OutputFileSwitch
        {
            get
            {
                return "--output=";
            }
        }

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            return this.pathEnvironment;
        }
    }
#endif
}

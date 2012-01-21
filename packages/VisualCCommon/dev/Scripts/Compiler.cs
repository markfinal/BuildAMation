// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public class CCompiler : C.Compiler, Opus.Core.ITool, Opus.Core.IToolSupportsResponseFile
    {
        private Opus.Core.StringArray includeFolder = new Opus.Core.StringArray();
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();

        private string platformBinFolder;

        public CCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            if (!Opus.Core.OSUtilities.IsWindows(target.Platform))
            {
                throw new Opus.Core.Exception("VisualC compiler supports only win32 and win64");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.platformBinFolder = toolChainInstance.BinPath(target);

            string installPath = toolChainInstance.InstallPath(target);
            this.includeFolder.Add(System.IO.Path.Combine(installPath, "include"));

            this.requiredEnvironmentVariables.Add("SystemRoot");
        }

        public string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(platformBinFolder, "cl.exe");
        }

        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return this.Executable(target);
        }

        public Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        public Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target)
        {
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
        }

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

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }
    }
}
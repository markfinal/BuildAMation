// <copyright file="Win32ResoureCompiler.cs" company="Mark Final">
//  MingwCommon package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
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

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.platformBinFolder = toolChainInstance.BinPath(target);
            this.pathEnvironment.AddRange(toolChainInstance.Environment);
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
}

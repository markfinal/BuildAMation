// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualCCommon package</summary>
// <author>Mark Final</author>
namespace VisualCCommon
{
    public sealed class Linker : C.Linker, Opus.Core.ITool
    {
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private string platformBinFolder;

        public Linker(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindows(target.Platform))
            {
                throw new Opus.Core.Exception("VisualC linker supports only win32 and win64");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.platformBinFolder = toolChainInstance.BinPath(target);

            this.requiredEnvironmentVariables.Add("TEMP");
            this.requiredEnvironmentVariables.Add("TMP");
        }

        public string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.platformBinFolder, "link.exe");
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
    }
}
// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed class Linker : MingwCommon.Linker
    {
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private string binPath;

        public Linker(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindows(target.Platform))
            {
                throw new Opus.Core.Exception("Mingw linker is only supported under win32 and win64 platforms");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);

            this.requiredEnvironmentVariables.Add("TEMP");
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "mingw32-gcc-3.4.5");
        }

        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "mingw32-g++.exe");
        }

        public override Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        public override Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target)
        {
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
        }
    }
}
// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public sealed class Archiver : C.Archiver, Opus.Core.ITool, Opus.Core.IToolEnvironmentPaths
    {
        private string binPath;

        static Archiver()
        {
            requiredEnvironmentVariables.Add("TEMP");
        }

        public Archiver(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindows(target.Platform))
            {
                throw new Opus.Core.Exception("Mingw compiler is only supported under win32 and win64 platforms");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);
        }

        public string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(binPath, "ar.exe");
        }

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
        }
    }
}
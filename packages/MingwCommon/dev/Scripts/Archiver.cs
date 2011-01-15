// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public sealed class Archiver : C.Archiver, Opus.Core.ITool
    {
        private static Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
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

        public Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get
            {
                return requiredEnvironmentVariables;
            }
        }

        public Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target)
        {
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
        }
    }
}
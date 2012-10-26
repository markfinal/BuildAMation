// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>MingwCommon package</summary>
// <author>Mark Final</author>
namespace MingwCommon
{
    public sealed class Archiver : C.Archiver, Opus.Core.ITool, Opus.Core.IToolEnvironmentPaths, Opus.Core.IToolRequiredEnvironmentVariables
    {
        private static Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private string binPath;

        static Archiver()
        {
            requiredEnvironmentVariables.Add("TEMP");
        }

        public Archiver(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindows(target))
            {
                throw new Opus.Core.Exception("Mingw compiler is only supported under win32 and win64 platforms");
            }

            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Mingw.Toolset));
            this.binPath = info.BinPath(target);
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);
#endif
        }

        public string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(binPath, "ar.exe");
        }

        Opus.Core.StringArray Opus.Core.IToolRequiredEnvironmentVariables.VariableNames
        {
            get
            {
                return requiredEnvironmentVariables;
            }
        }

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Mingw.Toolset));
            return info.Environment;
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
#endif
        }
    }
}
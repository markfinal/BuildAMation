// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    // Not sealed since the C++ compiler inherits from it
    public class CCompiler : MingwCommon.CCompiler
    {
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private Opus.Core.StringArray includeFolders = new Opus.Core.StringArray();
        private string binPath;

        public CCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindows(target.Platform))
            {
                throw new Opus.Core.Exception("Mingw compiler is only supported under win32 and win64 platforms");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);

            string installPath = toolChainInstance.InstallPath(target);

            string gccIncludeFolder = System.IO.Path.Combine(installPath, "lib");
            gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "gcc");
            gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "mingw32");
            gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "4.5.0"); // TODO: this is the package version; look up in the package collection, or some other way
            gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "include");

            includeFolders.Add(System.IO.Path.Combine(installPath, "include"));
            includeFolders.Add(gccIncludeFolder);

            requiredEnvironmentVariables.Add("TEMP");
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(binPath, "mingw32-gcc-4.5.0");
        }

        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(binPath, "mingw32-g++");
        }

        public override Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get
            {
                return requiredEnvironmentVariables;
            }
        }

        public override Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target)
        {
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
        }

        public override Opus.Core.StringArray IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return includeFolders;
        }
    }
}
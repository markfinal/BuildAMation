// <copyright file="CPlusPlusCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed class CPlusPlusCompiler : C.CxxCompiler, Opus.Core.ITool, Opus.Core.IToolRequiredEnvironmentVariables, Opus.Core.IToolEnvironmentPaths, C.ICompiler
    {
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private Opus.Core.StringArray includeFolders = new Opus.Core.StringArray();
        private string binPath;

        public CPlusPlusCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsWindows(target))
            {
                throw new Opus.Core.Exception("Mingw compiler is only supported under win32 and win64 platforms");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);

            string installPath = toolChainInstance.InstallPath(target);

            string gccIncludeFolder = System.IO.Path.Combine(installPath, "lib");
            gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "gcc");
            gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "mingw32");
            gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "3.4.5"); // TODO: this is the package version; look up in the package collection, or some other way
            gccIncludeFolder = System.IO.Path.Combine(gccIncludeFolder, "include");

            this.includeFolders.Add(System.IO.Path.Combine(installPath, "include"));
            this.includeFolders.Add(gccIncludeFolder);

            this.requiredEnvironmentVariables.Add("TEMP");
        }

        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "mingw32-g++");
        }

        Opus.Core.StringArray Opus.Core.IToolRequiredEnvironmentVariables.VariableNames
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
        }

        Opus.Core.StringArray C.ICompiler.IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return this.includeFolders;
        }

        Opus.Core.StringArray C.ICompiler.IncludePathCompilerSwitches
        {
            get
            {
                return new Opus.Core.StringArray("-isystem", "-I");
            }
        }
    }
}

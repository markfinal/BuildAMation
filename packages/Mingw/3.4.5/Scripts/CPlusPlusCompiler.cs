// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    // NEW STYLE
#if true
    public sealed class CxxCompiler : MingwCommon.CxxCompiler
    {
        public CxxCompiler(Opus.Core.IToolset toolset)
            : base(toolset)
        {
        }

        protected override string Filename
        {
            get
            {
                return "mingw32-g++";
            }
        }
    }
#else
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
            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Mingw.Toolset));
            this.binPath = info.BinPath((Opus.Core.BaseTarget)target);

            string installPath = info.InstallPath((Opus.Core.BaseTarget)target);
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);

            string installPath = toolChainInstance.InstallPath(target);
#endif

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
            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Mingw.Toolset));
            return info.Environment;
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
#endif
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
#endif
}

// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    // NEW STYLE
#if true
    public sealed class Linker : GccCommon.Linker
    {
        public Linker(Opus.Core.IToolset toolset)
            : base(toolset)
        {
        }

        protected override string Filename
        {
            get
            {
                return "gcc-4.6";
            }
        }
    }
#else
    public sealed class Linker : GccCommon.Linker, Opus.Core.IToolEnvironmentPaths, Opus.Core.IToolSupportsResponseFile
    {
        private Opus.Core.StringArray environmentPaths = new Opus.Core.StringArray();
        private string binPath;

        public Linker(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsUnix(target.Platform))
            {
                throw new Opus.Core.Exception("Gcc linker is only supported under unix32 and unix64 platforms");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);

            this.environmentPaths.Add("/usr/bin");
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "gcc-4.6");
        }

        // OLD STYLE
#if false
        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "g++-4.6");
        }
#endif

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            return this.environmentPaths;
        }

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }
    }
#endif
}
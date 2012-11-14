// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
namespace ComposerXE
{
    // NEW STYLE
#if true
#else
    public sealed class Linker : ComposerXECommon.Linker, Opus.Core.IToolEnvironmentPaths, Opus.Core.IToolSupportsResponseFile
    {
        private Opus.Core.StringArray environmentPaths = new Opus.Core.StringArray();
        private string binPath;

        public Linker(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsUnix(target))
            {
                throw new Opus.Core.Exception("ComposerXE linker is only supported under unix32 and unix64 platforms");
            }

            // NEW STYLE
#if true
            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(ComposerXE.Toolset));
            this.binPath = info.BinPath((Opus.Core.BaseTarget)target);
#else
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);
#endif

            this.environmentPaths.Add("/opt/intel/bin");
            this.environmentPaths.Add("/usr/bin");
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "icc");
        }

        // OLD STYLE
#if false
        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "icpc");
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

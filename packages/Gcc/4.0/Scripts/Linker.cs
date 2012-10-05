// <copyright file="Linker.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public sealed class Linker : GccCommon.Linker, Opus.Core.IToolEnvironmentPaths
    {
        private Opus.Core.StringArray environmentPaths = new Opus.Core.StringArray();
        private string binPath;

        public Linker(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsOSX(target))
            {
                throw new Opus.Core.Exception("Gcc linker is only supported under osx32 and osx64 platforms");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);

            this.environmentPaths.Add("/usr/bin");
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "gcc-4.0");
        }

        // OLD STYLE
#if false
        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "g++-4.0");
        }
#endif

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            return this.environmentPaths;
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
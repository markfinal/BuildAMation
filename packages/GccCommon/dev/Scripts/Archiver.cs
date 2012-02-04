// <copyright file="Archiver.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>GccCommon package</summary>
// <author>Mark Final</author>
namespace GccCommon
{
    public sealed class Archiver : C.Archiver, Opus.Core.ITool, Opus.Core.IToolSupportsResponseFile, Opus.Core.IToolEnvironmentPaths
    {
        private string binPath;

        public Archiver(Opus.Core.Target target)
        {
            if (!(Opus.Core.OSUtilities.IsUnix(target.Platform) || Opus.Core.OSUtilities.IsOSX(target.Platform)))
            {
                throw new Opus.Core.Exception("Gcc archiver is only supported under unix32, unix64, osx32 and osx64 platforms");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);
        }

        public string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "ar");
        }

        Opus.Core.StringArray Opus.Core.IToolEnvironmentPaths.Paths(Opus.Core.Target target)
        {
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
        }

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }
    }
}


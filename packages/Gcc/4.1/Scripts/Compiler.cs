// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    // Not sealed since the C++ compiler inherits from it
    public class CCompiler : GccCommon.CCompiler
    {
        private Opus.Core.StringArray requiredEnvironmentVariables = new Opus.Core.StringArray();
        private Opus.Core.StringArray includeFolders = new Opus.Core.StringArray();
        private string binPath;

        public CCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsUnix(target.Platform))
            {
                throw new Opus.Core.Exception("Gcc compiler is only supported under unix32 and unix64 platforms");
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);

            this.includeFolders.Add("/usr/include");
            this.includeFolders.Add("/usr/lib/gcc/i486-linux-gnu/4.1/include");
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "gcc-4.1");
        }

        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "g++-4.1");
        }

        public override Opus.Core.StringArray RequiredEnvironmentVariables
        {
            get
            {
                return this.requiredEnvironmentVariables;
            }
        }

        public override Opus.Core.StringArray EnvironmentPaths(Opus.Core.Target target)
        {
            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            return toolChainInstance.Environment;
        }

        public override Opus.Core.StringArray IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return this.includeFolders;
        }
    }
}
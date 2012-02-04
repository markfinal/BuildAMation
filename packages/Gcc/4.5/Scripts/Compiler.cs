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
        private Opus.Core.StringArray includeFolders = new Opus.Core.StringArray();
        private string binPath;

        public CCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsUnix(target.Platform))
            {
                throw new Opus.Core.Exception("Gcc compiler is only supported under unix32 and unix64 platforms", false);
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);

            this.includeFolders.Add("/usr/include");
            {
                // this is for some Linux distributions
                string path = System.String.Format("/usr/include/{0}", this.MachineType(target));
                if (System.IO.Directory.Exists(path))
                {
                    this.includeFolders.Add(path);
                }
            }
            string gccLibFolder = System.String.Format("/usr/lib/gcc/{0}/{1}", this.MachineType(target), this.GccVersion(target));
            string gccIncludeFolder = System.String.Format("{0}/include", gccLibFolder);
            string gccIncludeFixedFolder = System.String.Format("{0}/include-fixed", gccLibFolder);

            if (!System.IO.Directory.Exists(gccIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc include folder '{0}' does not exist", gccIncludeFolder), false);
            }
            this.includeFolders.Add(gccIncludeFolder);
            
            if (!System.IO.Directory.Exists(gccIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc include folder '{0}' does not exist", gccIncludeFixedFolder), false);
            }
            this.includeFolders.Add(gccIncludeFixedFolder);
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "gcc-4.5");
        }

        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "g++-4.5");
        }

        public override Opus.Core.StringArray IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return this.includeFolders;
        }
    }
}

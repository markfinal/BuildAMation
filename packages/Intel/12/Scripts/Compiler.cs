// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Intel package</summary>
// <author>Mark Final</author>
namespace Intel
{
    // Not sealed since the C++ compiler inherits from it
    public class CCompiler : IntelCommon.CCompiler, Opus.Core.IToolSupportsResponseFile
    {
        private Opus.Core.StringArray includeFolders = new Opus.Core.StringArray();
        private string binPath;

        public CCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsUnix(target.Platform))
            {
                throw new Opus.Core.Exception("Intel compiler is only supported under unix32 and unix64 platforms", false);
            }

            Toolchain toolChainInstance = C.ToolchainFactory.GetTargetInstance(target) as Toolchain;
            this.binPath = toolChainInstance.BinPath(target);

            this.includeFolders.Add("/usr/include");
            this.includeFolders.Add("/usr/include/linux");
#if false
            {
                // this is for some Linux distributions
                string path = System.String.Format("/usr/include/{0}", this.MachineType(target));
                if (System.IO.Directory.Exists(path))
                {
                    this.includeFolders.Add(path);
                }
            }
            string IntelLibFolder = System.String.Format("/usr/lib/Intel/{0}/{1}", this.MachineType(target), this.IntelVersion(target));
            string IntelIncludeFolder = System.String.Format("{0}/include", IntelLibFolder);
            string IntelIncludeFixedFolder = System.String.Format("{0}/include-fixed", IntelLibFolder);

            if (!System.IO.Directory.Exists(IntelIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("Intel include folder '{0}' does not exist", IntelIncludeFolder), false);
            }
            this.includeFolders.Add(IntelIncludeFolder);
            
            if (!System.IO.Directory.Exists(IntelIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("Intel include folder '{0}' does not exist", IntelIncludeFixedFolder), false);
            }
            this.includeFolders.Add(IntelIncludeFixedFolder);
#endif
        }

        public override string Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "icc");
        }

        public override string ExecutableCPlusPlus(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "icpc");
        }

        public override Opus.Core.StringArray IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return this.includeFolders;
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

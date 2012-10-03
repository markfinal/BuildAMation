// <copyright file="Compiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>ComposerXE package</summary>
// <author>Mark Final</author>
namespace ComposerXE
{
    // Not sealed since the C++ compiler inherits from it
    public class CCompiler : ComposerXECommon.CCompiler, Opus.Core.IToolSupportsResponseFile, C.ICompiler
    {
        private Opus.Core.StringArray includeFolders = new Opus.Core.StringArray();
        private string binPath;

        public CCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsUnix(target))
            {
                throw new Opus.Core.Exception("ComposerXE compiler is only supported under unix32 and unix64 platforms", false);
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
            string ComposerXELibFolder = System.String.Format("/usr/lib/ComposerXE/{0}/{1}", this.MachineType(target), this.ComposerXEVersion(target));
            string ComposerXEIncludeFolder = System.String.Format("{0}/include", ComposerXELibFolder);
            string ComposerXEIncludeFixedFolder = System.String.Format("{0}/include-fixed", ComposerXELibFolder);

            if (!System.IO.Directory.Exists(ComposerXEIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("ComposerXE include folder '{0}' does not exist", ComposerXEIncludeFolder), false);
            }
            this.includeFolders.Add(ComposerXEIncludeFolder);
            
            if (!System.IO.Directory.Exists(ComposerXEIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("ComposerXE include folder '{0}' does not exist", ComposerXEIncludeFixedFolder), false);
            }
            this.includeFolders.Add(ComposerXEIncludeFixedFolder);
#endif
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

        // NEW STYLE
#if true
        Opus.Core.StringArray C.ICompiler.IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return this.includeFolders;
        }
#else
        public override Opus.Core.StringArray IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return this.includeFolders;
        }
#endif

        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }
    }
}

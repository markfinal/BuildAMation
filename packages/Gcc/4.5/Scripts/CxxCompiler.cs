// <copyright file="CxxCompiler.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    // NEW STYLE
#if true
    public sealed class CxxCompiler : GccCommon.CxxCompiler
    {
        public CxxCompiler(Opus.Core.IToolset toolset)
            : base(toolset)
        {
        }

        #region implemented abstract members of GccCommon.CxxCompiler
        protected override string Filename
        {
            get
            {
                return "g++-4.5";
            }
        }
        #endregion
    }
#else
    public sealed class CxxCompiler : GccCommon.CxxCompiler, Opus.Core.IToolSupportsResponseFile, C.ICompiler, Opus.Core.ITool
    {
        private Opus.Core.StringArray includeFolders = new Opus.Core.StringArray();
        private string binPath;

        public CxxCompiler(Opus.Core.Target target)
        {
            if (!Opus.Core.OSUtilities.IsUnix(target))
            {
                throw new Opus.Core.Exception("Gcc compiler is only supported under unix32 and unix64 platforms", false);
            }

            Opus.Core.IToolset info = Opus.Core.ToolsetFactory.CreateToolset(typeof(Gcc.Toolset));
            this.binPath = info.BinPath((Opus.Core.BaseTarget)target);
            
            GccCommon.IGCCInfo gccInfo = info as GccCommon.IGCCInfo;
            this.includeFolders.Add("/usr/include");
            {
                // this is for some Linux distributions
                string path = System.String.Format("/usr/include/{0}", gccInfo.MachineType(target));
                if (System.IO.Directory.Exists(path))
                {
                    this.includeFolders.Add(path);
                }
            }
            string gccLibFolder = System.String.Format("/usr/lib/gcc/{0}/{1}", gccInfo.MachineType(target), gccInfo.GccVersion(target));
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

#region Opus.Core.ITool
        string Opus.Core.ITool.Executable(Opus.Core.Target target)
        {
            return System.IO.Path.Combine(this.binPath, "g++-4.4");
        }
#endregion

#region C.ICompiler
        Opus.Core.StringArray C.ICompiler.IncludeDirectoryPaths(Opus.Core.Target target)
        {
            return this.includeFolders;
        }

        Opus.Core.StringArray C.ICompiler.IncludePathCompilerSwitches
        {
            get
            {
                return base.CommonIncludePathCompilerSwitches;
            }
        }
#endregion

#region Opus.Core.IToolSupportsResponseFile
        string Opus.Core.IToolSupportsResponseFile.Option
        {
            get
            {
                return "@";
            }
        }
#endregion
    }
#endif
}

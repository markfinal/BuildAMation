// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Gcc package</summary>
// <author>Mark Final</author>
namespace Gcc
{
    public sealed class Toolset : GccCommon.Toolset
    {
        public Toolset()
        {
            this.toolMap[typeof(C.ICompilerTool)] = new CCompiler(this);
            this.toolMap[typeof(C.ICxxCompilerTool)] = new CxxCompiler(this);
            this.toolMap[typeof(C.ILinkerTool)] = new Linker(this);
            this.toolMap[typeof(C.IArchiverTool)] = new GccCommon.Archiver(this);

            this.toolOptionsMap[typeof(C.ICompilerTool)] = typeof(Gcc.CCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ICxxCompilerTool)] = typeof(Gcc.CPlusPlusCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ILinkerTool)] = typeof(Gcc.LinkerOptionCollection);
            this.toolOptionsMap[typeof(C.IArchiverTool)] = typeof(Gcc.ArchiverOptionCollection);
        }

        protected override void GetInstallPath(Opus.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return;
            }

            string installPath = null;
            if (Opus.Core.State.HasCategory("Gcc") && Opus.Core.State.Has("Gcc", "InstallPath"))
            {
                installPath = Opus.Core.State.Get("Gcc", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Gcc install path set from command line to '{0}'", installPath);
            }

            if (null == installPath)
            {
                installPath = "/usr/bin";
            }

            this.installPath = installPath;
            this.gccDetail = GccCommon.GccDetailGatherer.GetGccDetails(Opus.Core.Target.GetInstance(baseTarget, this));

            // C include paths
            this.includePaths.Add("/usr/include");
            {
                // this is for some Linux distributions
                string path = System.String.Format("/usr/include/{0}", this.gccDetail.Target);
                if (System.IO.Directory.Exists(path))
                {
                    this.includePaths.Add(path);
                }
            }
            string gccLibFolder = System.String.Format("/usr/lib/gcc/{0}/{1}", this.gccDetail.Target, this.gccDetail.Version);
            string gccIncludeFolder = System.String.Format("{0}/include", gccLibFolder);
            string gccIncludeFixedFolder = System.String.Format("{0}/include-fixed", gccLibFolder);

            if (!System.IO.Directory.Exists(gccIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc include folder '{0}' does not exist", gccIncludeFolder), false);
            }
            this.includePaths.Add(gccIncludeFolder);
            
            if (!System.IO.Directory.Exists(gccIncludeFolder))
            {
                throw new Opus.Core.Exception(System.String.Format("Gcc include folder '{0}' does not exist", gccIncludeFixedFolder), false);
            }
            this.includePaths.Add(gccIncludeFixedFolder);

            // C++ include paths
            this.cxxIncludePath = this.gccDetail.GxxIncludePath;
        }

        protected override string GetVersion (Opus.Core.BaseTarget baseTarget)
        {
            return "4.4";
        }

        public override string GetMachineType(Opus.Core.BaseTarget baseTarget)
        {
            this.GetInstallPath(baseTarget);
            return this.gccDetail.Target;
        }
    }
}

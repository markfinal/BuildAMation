// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>Mingw package</summary>
// <author>Mark Final</author>
namespace Mingw
{
    public sealed class Toolset : MingwCommon.Toolset
    {
        public Toolset()
        {
            this.toolMap[typeof(C.ICompilerTool)] = new CCompiler(this);
            this.toolMap[typeof(C.ICxxCompilerTool)] = new CxxCompiler(this);
            this.toolMap[typeof(C.ILinkerTool)] = new Linker(this);
            this.toolMap[typeof(C.IArchiverTool)] = new MingwCommon.Archiver(this);
            this.toolMap[typeof(C.IWinResourceCompilerTool)] = new MingwCommon.Win32ResourceCompiler(this);

            this.toolOptionsMap[typeof(C.ICompilerTool)] = typeof(Mingw.CCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ICxxCompilerTool)] = typeof(Mingw.CxxCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ILinkerTool)] = typeof(Mingw.LinkerOptionCollection);
            this.toolOptionsMap[typeof(C.IArchiverTool)] = typeof(Mingw.ArchiverOptionCollection);
            this.toolOptionsMap[typeof(C.IWinResourceCompilerTool)] = typeof(C.Win32ResourceCompilerOptionCollection);
        }

        protected override void GetInstallPath(Opus.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return;
            }

            if (Opus.Core.State.HasCategory("Mingw") && Opus.Core.State.Has("Mingw", "InstallPath"))
            {
                this.installPath = Opus.Core.State.Get("Mingw", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("Mingw install path set from command line to '{0}'", this.installPath);
            }

            if (null == this.installPath)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.OpenLMSoftwareKey(@"Microsoft\Windows\CurrentVersion\Uninstall\{AC2C1BDB-1E91-4F94-B99C-E716FE2E9C75}_is1"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("Mingw 4.5.0 was not installed");
                    }

                    this.installPath = key.GetValue("InstallLocation") as string;
                    Opus.Core.Log.DebugMessage("Mingw: Install path from registry '{0}'", this.installPath);
                }
            }

            this.binPath = System.IO.Path.Combine(this.installPath, "bin");
            this.environment.Add(this.binPath);

            this.details = MingwCommon.MingwDetailGatherer.DetermineSpecs(baseTarget, this);
        }
    }
}

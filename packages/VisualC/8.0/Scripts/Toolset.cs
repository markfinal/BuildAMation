// <copyright file="Toolset.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VisualC package</summary>
// <author>Mark Final</author>
namespace VisualC
{
    public sealed class Toolset : VisualCCommon.Toolset, VisualStudioProcessor.IVisualStudioTargetInfo
    {
        static Toolset()
        {
            if (!Opus.Core.State.HasCategory("VSSolutionBuilder"))
            {
                Opus.Core.State.AddCategory("VSSolutionBuilder");
            }

            if (!Opus.Core.State.Has("VSSolutionBuilder", "SolutionType"))
            {
                Opus.Core.State.Add<System.Type>("VSSolutionBuilder", "SolutionType", typeof(Solution));
            }
        }

        public Toolset()
        {
            this.toolMap[typeof(C.ICompilerTool)] = new VisualCCommon.CCompiler(this);
            this.toolMap[typeof(C.ICxxCompilerTool)] = new VisualCCommon.CxxCompiler(this);
            this.toolMap[typeof(C.ILinkerTool)] = new VisualCCommon.Linker(this);
            this.toolMap[typeof(C.IArchiverTool)] = new VisualCCommon.Archiver(this);
            this.toolMap[typeof(C.IWinResourceCompilerTool)] = new VisualCCommon.Win32ResourceCompiler(this);

            this.toolOptionsMap[typeof(C.ICompilerTool)] = typeof(VisualC.CCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ICxxCompilerTool)] = typeof(VisualC.CPlusPlusCompilerOptionCollection);
            this.toolOptionsMap[typeof(C.ILinkerTool)] = typeof(VisualC.LinkerOptionCollection);
            this.toolOptionsMap[typeof(C.IArchiverTool)] = typeof(VisualC.ArchiverOptionCollection);
            this.toolOptionsMap[typeof(C.IWinResourceCompilerTool)] = typeof(C.Win32ResourceCompilerOptionCollection);
        }
        
        protected override void GetInstallPath()
        {
            if (null != this.installPath)
            {
                return;
            }

            if (Opus.Core.State.HasCategory("VisualC") && Opus.Core.State.Has("VisualC", "InstallPath"))
            {
                this.installPath = Opus.Core.State.Get("VisualC", "InstallPath") as string;
                Opus.Core.Log.DebugMessage("VisualC 2005 install path set from command line to '{0}'", this.installPath);
            }
            
            if (null == this.installPath)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\VisualStudio\SxS\VC7"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("VisualStudio was not installed");
                    }
                    
                    this.installPath = key.GetValue("8.0") as string;
                    if (null == this.installPath)
                    {
                        throw new Opus.Core.Exception("VisualStudio 2005 was not installed");
                    }
                    
                    this.installPath = this.installPath.TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar });
                    Opus.Core.Log.DebugMessage("VisualStudio 2005: Installation path from registry '{0}'", this.installPath);
                }
            }
            
            this.bin32Folder = System.IO.Path.Combine(this.installPath, "bin");
            this.bin64Folder = System.IO.Path.Combine(this.bin32Folder, "amd64");
            this.bin6432Folder = System.IO.Path.Combine(this.bin32Folder, "x86_amd64");
            
            this.lib32Folder.Add(System.IO.Path.Combine(this.installPath, "lib"));
            this.lib64Folder.Add(System.IO.Path.Combine(this.lib32Folder[0], "amd64"));

            string parent = System.IO.Directory.GetParent(this.installPath).FullName;
            string common7 = System.IO.Path.Combine(parent, "Common7");
            string ide = System.IO.Path.Combine(common7, "IDE");

            this.environment = new Opus.Core.StringArray();
            this.environment.Add(ide);
        }

        protected override string GetVersion (Opus.Core.BaseTarget baseTarget)
        {
            return "8.0"; // TODO: return CRT version
        }

        #region IVisualStudioTargetInfo Members

        VisualStudioProcessor.EVisualStudioTarget VisualStudioProcessor.IVisualStudioTargetInfo.VisualStudioTarget
        {
            get
            {
                return VisualStudioProcessor.EVisualStudioTarget.VCPROJ;
            }
        }

        #endregion
    }
}

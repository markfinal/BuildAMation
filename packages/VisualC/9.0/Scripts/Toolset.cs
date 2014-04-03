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
            this.toolConfig[typeof(C.ICompilerTool)] = new Opus.Core.ToolAndOptionType(new VisualCCommon.CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)] = new Opus.Core.ToolAndOptionType(new VisualCCommon.CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.ILinkerTool)] = new Opus.Core.ToolAndOptionType(new VisualCCommon.Linker(this), typeof(LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)] = new Opus.Core.ToolAndOptionType(new VisualCCommon.Archiver(this), typeof(ArchiverOptionCollection));
            this.toolConfig[typeof(C.IWinResourceCompilerTool)] = new Opus.Core.ToolAndOptionType(new VisualCCommon.Win32ResourceCompiler(this), typeof(VisualCCommon.Win32ResourceCompilerOptionCollection));
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
                Opus.Core.Log.DebugMessage("VisualC 2008 install path set from command line to '{0}'", this.installPath);
            }

            if (null == this.installPath)
            {
                using (Microsoft.Win32.RegistryKey key = Opus.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\VisualStudio\SxS\VC7"))
                {
                    if (null == key)
                    {
                        throw new Opus.Core.Exception("VisualStudio was not installed");
                    }

                    this.installPath = key.GetValue("9.0") as string;
                    if (null == this.installPath)
                    {
                        throw new Opus.Core.Exception("VisualStudio 2008 was not installed");
                    }

                    this.installPath = this.installPath.TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar });
                    Opus.Core.Log.DebugMessage("VisualStudio 2008: Installation path from registry '{0}'", this.installPath);
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

            this.environment.Add(ide);
        }

        protected override string GetVersion(Opus.Core.BaseTarget baseTarget)
        {
            return this.GetVersionString("9.0");
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
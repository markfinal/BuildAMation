#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace VisualC
{
    public sealed class Toolset :
        VisualCCommon.Toolset,
        VisualStudioProcessor.IVisualStudioTargetInfo
    {
        static
        Toolset()
        {
            if (!Bam.Core.State.HasCategory("VSSolutionBuilder"))
            {
                Bam.Core.State.AddCategory("VSSolutionBuilder");
            }

            if (!Bam.Core.State.Has("VSSolutionBuilder", "SolutionType"))
            {
                Bam.Core.State.Add<System.Type>("VSSolutionBuilder", "SolutionType", typeof(Solution));
            }
        }

        public
        Toolset()
        {
            this.toolConfig[typeof(C.ICompilerTool)] = new Bam.Core.ToolAndOptionType(new VisualCCommon.CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)] = new Bam.Core.ToolAndOptionType(new VisualCCommon.CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.ILinkerTool)] = new Bam.Core.ToolAndOptionType(new VisualCCommon.Linker(this), typeof(LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)] = new Bam.Core.ToolAndOptionType(new VisualCCommon.Archiver(this), typeof(ArchiverOptionCollection));
            this.toolConfig[typeof(C.IWinResourceCompilerTool)] = new Bam.Core.ToolAndOptionType(new VisualCCommon.Win32ResourceCompiler(this), typeof(VisualCCommon.Win32ResourceCompilerOptionCollection));
            this.toolConfig[typeof(C.IWinManifestTool)] = new Bam.Core.ToolAndOptionType(new VisualCCommon.Win32ManifestTool(this), typeof(VisualCCommon.Win32ManifestOptionCollection));
        }

        protected override void
        GetInstallPath()
        {
            if (null != this.installPath)
            {
                return;
            }

            if (Bam.Core.State.HasCategory("VisualC") && Bam.Core.State.Has("VisualC", "InstallPath"))
            {
                this.installPath = Bam.Core.State.Get("VisualC", "InstallPath") as string;
                Bam.Core.Log.DebugMessage("VisualC 2010 install path set from command line to '{0}'", this.installPath);
            }

            if (null == this.installPath)
            {
                using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\VisualStudio\SxS\VC7"))
                {
                    if (null == key)
                    {
                        throw new Bam.Core.Exception("VisualStudio was not installed");
                    }

                    this.installPath = key.GetValue("10.0") as string;
                    if (null == this.installPath)
                    {
                        throw new Bam.Core.Exception("VisualStudio 2010 was not installed");
                    }

                    this.installPath = this.installPath.TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar });
                    Bam.Core.Log.DebugMessage("VisualStudio 2010: Installation path from registry '{0}'", this.installPath);
                }
            }

            this.bin32Folder = System.IO.Path.Combine(this.installPath, "bin");
            this.bin64Folder = System.IO.Path.Combine(this.bin32Folder, "amd64");
            this.bin6432Folder = System.IO.Path.Combine(this.bin32Folder, "x86_amd64");

            this.lib32Folder.Add(System.IO.Path.Combine(this.installPath, "lib"));
            this.lib64Folder.Add(System.IO.Path.Combine(this.lib32Folder[0], "amd64"));

            var parent = System.IO.Directory.GetParent(this.installPath).FullName;
            var common7 = System.IO.Path.Combine(parent, "Common7");
            var ide = System.IO.Path.Combine(common7, "IDE");

            this.environment = new Bam.Core.StringArray();
            this.environment.Add(ide);
        }

        protected override string
        GetVersion(
            Bam.Core.BaseTarget baseTarget)
        {
            return this.GetVersionString("10.0");
        }

        #region IVisualStudioTargetInfo Members

        VisualStudioProcessor.EVisualStudioTarget VisualStudioProcessor.IVisualStudioTargetInfo.VisualStudioTarget
        {
            get
            {
                return VisualStudioProcessor.EVisualStudioTarget.MSBUILD;
            }
        }

        #endregion
    }
}

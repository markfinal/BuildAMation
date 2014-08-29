#region License
// Copyright 2010-2014 Mark Final
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
namespace Mingw
{
    public sealed class Toolset :
        MingwCommon.Toolset
    {
        public
        Toolset()
        {
            this.toolConfig[typeof(C.ICompilerTool)] = new Bam.Core.ToolAndOptionType(new CCompiler(this), typeof(CCompilerOptionCollection));
            this.toolConfig[typeof(C.ICxxCompilerTool)] = new Bam.Core.ToolAndOptionType(new CxxCompiler(this), typeof(CxxCompilerOptionCollection));
            this.toolConfig[typeof(C.ILinkerTool)] = new Bam.Core.ToolAndOptionType(new Linker(this), typeof(LinkerOptionCollection));
            this.toolConfig[typeof(C.IArchiverTool)] = new Bam.Core.ToolAndOptionType(new MingwCommon.Archiver(this), typeof(ArchiverOptionCollection));
            this.toolConfig[typeof(C.IWinResourceCompilerTool)] = new Bam.Core.ToolAndOptionType(new MingwCommon.Win32ResourceCompiler(this), typeof(C.Win32ResourceCompilerOptionCollection));
        }

        protected override void
        GetInstallPath(
            Bam.Core.BaseTarget baseTarget)
        {
            if (null != this.installPath)
            {
                return;
            }

            if (Bam.Core.State.HasCategory("Mingw") && Bam.Core.State.Has("Mingw", "InstallPath"))
            {
                this.installPath = Bam.Core.State.Get("Mingw", "InstallPath") as string;
                Bam.Core.Log.DebugMessage("Mingw install path set from command line to '{0}'", this.installPath);
            }

            if (null == this.installPath)
            {
                using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\Windows\CurrentVersion\Uninstall\MinGW"))
                {
                    if (null == key)
                    {
                        throw new Bam.Core.Exception("Mingw 3.4.5 was not installed");
                    }

                    this.installPath = key.GetValue("InstallLocation") as string;
                    Bam.Core.Log.DebugMessage("Mingw: Install path from registry '{0}'", this.installPath);
                }
            }

            this.binPath = System.IO.Path.Combine(this.installPath, "bin");
            this.environment.Add(this.binPath);

            this.details = MingwCommon.MingwDetailGatherer.DetermineSpecs(baseTarget, this);
        }
    }
}

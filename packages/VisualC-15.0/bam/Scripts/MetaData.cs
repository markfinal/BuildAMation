#region License
// Copyright (c) 2010-2017, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace VisualC
{
    public class MetaData :
        Bam.Core.PackageMetaData,
        VisualCCommon.IRuntimeLibraryPathMeta
    {
        private System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string,object>();

        public MetaData()
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            // TODO: get this from the registry
            this.InstallDir = Bam.Core.TokenizedString.Create("$(0)/Microsoft Visual Studio/2017/Community", null, new Bam.Core.TokenizedStringArray(Bam.Core.OSUtilities.WindowsProgramFilesx86Path));
            this.InstallDir.Parse();
            if (!System.IO.Directory.Exists(this.InstallDir.ToString()))
            {
                throw new Bam.Core.Exception("'{0}' was not found. Was VisualStudio 2017 installed?", this.InstallDir.ToString());
            }

            this.VCToolsVersion = Bam.Core.TokenizedString.CreateVerbatim("14.12.25827");
            this.CRuntimeVersion = Bam.Core.TokenizedString.CreateVerbatim("14.12.25810");

            if (Bam.Core.OSUtilities.Is64BitHosting)
            {
                this.Bin32Dir = Bam.Core.TokenizedString.Create("$(0)/VC/Tools/MSVC/$(1)/bin/HostX64/x86", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.VCToolsVersion));
                this.Bin64Dir = Bam.Core.TokenizedString.Create("$(0)/VC/Tools/MSVC/$(1)/bin/HostX64/x64", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.VCToolsVersion));
                this.MSPDBDir = Bam.Core.TokenizedString.Create("$(0)/VC/Tools/MSVC/$(1)/bin/HostX64/x64", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.VCToolsVersion));
            }
            else
            {
                this.Bin32Dir = Bam.Core.TokenizedString.Create("$(0)/VC/Tools/MSVC/$(1)/bin/HostX86/x86", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.VCToolsVersion));
                this.Bin64Dir = Bam.Core.TokenizedString.Create("$(0)/VC/Tools/MSVC/$(1)/bin/HostX86/x64", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.VCToolsVersion));
                this.MSPDBDir = Bam.Core.TokenizedString.Create("$(0)/VC/Tools/MSVC/$(1)/bin/HostX86/x86", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.VCToolsVersion));
            }
            this.IncludeDir = Bam.Core.TokenizedString.Create("$(0)/VC/Tools/MSVC/$(1)/include", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.VCToolsVersion));
            this.Lib32Dir = Bam.Core.TokenizedString.Create("$(0)/VC/Tools/MSVC/$(1)/lib/x86", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.VCToolsVersion));
            this.Lib64Dir = Bam.Core.TokenizedString.Create("$(0)/VC/Tools/MSVC/$(1)/lib/x64", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.VCToolsVersion));

            this.SolutionFormatVersion = "12.00"; // same as VS2015
            this.PlatformToolset = "v141";
            this.VCXProjToolsVersion = "15.0";
            this.VCXProjFiltersToolsVersion = "4.0"; // same as VS2015
            this.UseWindowsSDKPublicPatches = true; // headers like stdio.h are in WindowsSDK 10
            this.RequiredExecutablePaths = new Bam.Core.TokenizedStringArray(Bam.Core.TokenizedString.Create("$(0)/Common7/IDE", null, new Bam.Core.TokenizedStringArray(this.InstallDir)));
        }

        public override object this[string index]
        {
            get
            {
                return this.Meta[index];
            }
        }

        public override bool
        Contains(
            string index)
        {
            return this.Meta.ContainsKey(index);
        }

        public Bam.Core.TokenizedString
        InstallDir
        {
            get
            {
                return this.Meta["InstallDir"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["InstallDir"] = value;
            }
        }

        public Bam.Core.TokenizedString
        Bin32Dir
        {
            get
            {
                return this.Meta["Bin32Dir"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["Bin32Dir"] = value;
            }
        }

        public Bam.Core.TokenizedString
        Bin64Dir
        {
            get
            {
                return this.Meta["Bin64Dir"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["Bin64Dir"] = value;
            }
        }

        public Bam.Core.TokenizedString
        IncludeDir
        {
            get
            {
                return this.Meta["IncludeDir"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["IncludeDir"] = value;
            }
        }

        public Bam.Core.TokenizedString
        Lib32Dir
        {
            get
            {
                return this.Meta["Lib32Dir"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["Lib32Dir"] = value;
            }
        }

        public Bam.Core.TokenizedString
        Lib64Dir
        {
            get
            {
                return this.Meta["Lib64Dir"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["Lib64Dir"] = value;
            }
        }

        public Bam.Core.TokenizedString
        MSPDBDir
        {
            get
            {
                return this.Meta["MSPDBDIR"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["MSPDBDIR"] = value;
            }
        }

        public string
        SolutionFormatVersion
        {
            get
            {
                return this.Meta["SolutionFormatVersion"] as string;
            }

            private set
            {
                this.Meta["SolutionFormatVersion"] = value;
            }
        }

        public string
        PlatformToolset
        {
            get
            {
                return this.Meta["PlatformToolset"] as string;
            }

            private set
            {
                this.Meta["PlatformToolset"] = value;
            }
        }

        public string
        VCXProjToolsVersion
        {
            get
            {
                return this.Meta["VCXProjToolsVersion"] as string;
            }

            private set
            {
                this.Meta["VCXProjToolsVersion"] = value;
            }
        }

        public string
        VCXProjFiltersToolsVersion
        {
            get
            {
                return this.Meta["VCXProjFiltersToolsVersion"] as string;
            }

            private set
            {
                this.Meta["VCXProjFiltersToolsVersion"] = value;
            }
        }

        public bool
        UseWindowsSDKPublicPatches
        {
            get
            {
                return (bool)this.Meta["RequiresWindowsSDK"];
            }

            private set
            {
                this.Meta["RequiresWindowsSDK"] = value;
            }
        }

        public Bam.Core.TokenizedStringArray
        RequiredExecutablePaths
        {
            get
            {
                return this.Meta["AdditionalPATHs"] as Bam.Core.TokenizedStringArray;
            }

            private set
            {
                this.Meta["AdditionalPATHs"] = value;
            }
        }

        public int
        CompilerMajorVersion
        {
            get
            {
                return 19;
            }
        }

        public int
        CompilerMinorVersion
        {
            get
            {
                return 10;
            }
        }

        public Bam.Core.TokenizedString
        VCToolsVersion
        {
            get
            {
                return this.Meta["VCToolsVersion"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["VCToolsVersion"] = value;
            }
        }

        public Bam.Core.TokenizedString
        CRuntimeVersion
        {
            get
            {
                return this.Meta["CRuntimeVersion"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["CRuntimeVersion"] = value;
            }
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CRuntimePaths(
            C.EBit depth)
        {
            // only redist the VisualC specific version runtime, and the universal CRT
            // don't redist the api-ms-win-crt-*-l1-1-0.dll files from the WindowsSDK, as I can find no reference
            // to needing to do so

            var windowsSDKMeta = Bam.Core.Graph.Instance.PackageMetaData<WindowsSDK.MetaData>("WindowsSDK");

            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    {
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/Redist/MSVC/$(1)/x86/Microsoft.VC141.CRT/vcruntime140.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.CRuntimeVersion)));
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/Redist/ucrt/DLLs/x86/ucrtbase.dll", null, new Bam.Core.TokenizedStringArray(windowsSDKMeta.InstallDirSDK10)));
                    }
                    break;

                case C.EBit.SixtyFour:
                    {
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/Redist/MSVC/$(1)/x64/Microsoft.VC141.CRT/vcruntime140.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.CRuntimeVersion)));
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/Redist/ucrt/DLLs/x64/ucrtbase.dll", null, new Bam.Core.TokenizedStringArray(windowsSDKMeta.InstallDirSDK10)));
                    }
                    break;

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
            return dynamicLibPaths;
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CxxRuntimePaths(
            C.EBit depth)
        {
            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/Redist/MSVC/$(1)/x86/Microsoft.VC141.CRT/msvcp140.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.CRuntimeVersion)));
                    break;

                case C.EBit.SixtyFour:
                    dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("$(0)/VC/Redist/MSVC/$(1)/x64/Microsoft.VC141.CRT/msvcp140.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir, this.CRuntimeVersion)));
                    break;

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
            return dynamicLibPaths;
        }
    }
}

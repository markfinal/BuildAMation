#region License
// Copyright (c) 2010-2018, Mark Final
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
        VisualCCommon.MetaData,
        VisualCCommon.IRuntimeLibraryPathMeta
    {
        public MetaData()
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            var install_dir = this.vswhere_getinstallpath(10);
            this.InstallDir = Bam.Core.TokenizedString.CreateVerbatim(install_dir);
            this.get_tool_environment_variables(
                "VC",
                has64bithost_32bitcross: false,
                hasNative64BitTools: false
            );

            this.SolutionFormatVersion = "11.00";
            this.PlatformToolset = "v100";
            this.VCXProjToolsVersion = "4.0";
            this.VCXProjFiltersToolsVersion = "4.0";
            this.UseWindowsSDKPublicPatches = false;
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

        public int
        CompilerMajorVersion
        {
            get
            {
                return 16;
            }
        }

        public int
        CompilerMinorVersion
        {
            get
            {
                return 0;
            }
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CRuntimePaths(
            C.EBit depth)
        {
            // https://social.msdn.microsoft.com/Forums/en-US/7b703997-e0d2-4b25-bb3a-c6a00141221d/visual-studio-2010-missing-mscvr100dll?forum=Vsexpressvcs
            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            if (Bam.Core.OSUtilities.Is64BitHosting)
            {
                switch (depth)
                {
                    case C.EBit.ThirtyTwo:
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("C:/Windows/SysWOW64/msvcr100.dll", null));
                        break;

                    case C.EBit.SixtyFour:
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("C:/Windows/System32/msvcr100.dll", null));
                        break;

                    default:
                        throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
                }
            }
            else
            {
                switch (depth)
                {
                    case C.EBit.ThirtyTwo:
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("C:/Windows/System32/msvcr100.dll", null));
                        break;

                    case C.EBit.SixtyFour:
                        throw new Bam.Core.Exception("64-bit CRT is not available on 32-bit operating systems.");

                    default:
                        throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
                }
            }
            return dynamicLibPaths;
        }

        Bam.Core.TokenizedStringArray
        VisualCCommon.IRuntimeLibraryPathMeta.CxxRuntimePaths(
            C.EBit depth)
        {
            // https://social.msdn.microsoft.com/Forums/en-US/7b703997-e0d2-4b25-bb3a-c6a00141221d/visual-studio-2010-missing-mscvr100dll?forum=Vsexpressvcs
            var dynamicLibPaths = new Bam.Core.TokenizedStringArray();
            if (Bam.Core.OSUtilities.Is64BitHosting)
            {
                switch (depth)
                {
                    case C.EBit.ThirtyTwo:
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("C:/Windows/SysWOW64/msvcp100.dll", null));
                        break;

                    case C.EBit.SixtyFour:
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("C:/Windows/System32/msvcp100.dll", null));
                        break;

                    default:
                        throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
                }
            }
            else
            {
                switch (depth)
                {
                    case C.EBit.ThirtyTwo:
                        dynamicLibPaths.Add(Bam.Core.TokenizedString.Create("C:/Windows/System32/msvcp100.dll", null));
                        break;

                    case C.EBit.SixtyFour:
                        throw new Bam.Core.Exception("64-bit CRT is not available on 32-bit operating systems.");

                    default:
                        throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
                }
            }
            return dynamicLibPaths;
        }
    }
}

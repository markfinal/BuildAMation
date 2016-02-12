#region License
// Copyright (c) 2010-2016, Mark Final
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
            this.InstallDir = Bam.Core.TokenizedString.Create("$(0)/Microsoft Visual Studio 14.0", null, new Bam.Core.TokenizedStringArray(Bam.Core.OSUtilities.WindowsProgramFilesx86Path));
            this.PlatformToolset = "v140";
            this.UseWindowsSDKPublicPatches = true; // headers like stdio.h are in WindowsSDK 10
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

        // TODO: note that msvcr*.dll no longer exists from VisualStudio 2015
        // it is replaced by the universal crt, which is in the Windows SDK: https://blogs.msdn.microsoft.com/vcblog/2015/03/03/introducing-the-universal-crt/
        // but also part of the Windows 10 OS
        Bam.Core.TokenizedString
        VisualCCommon.IRuntimeLibraryPathMeta.MSVCR(
            C.EBit depth)
        {
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    return Bam.Core.TokenizedString.Create("$(0)/VC/redist/x86/Microsoft.VC140.CRT/msvcr140.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir));

                case C.EBit.SixtyFour:
                    return Bam.Core.TokenizedString.Create("$(0)/VC/redist/x64/Microsoft.VC140.CRT/msvcr140.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir));

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
        }

        Bam.Core.TokenizedString VisualCCommon.IRuntimeLibraryPathMeta.MSVCP(C.EBit depth)
        {
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    return Bam.Core.TokenizedString.Create("$(0)/VC/redist/x86/Microsoft.VC140.CRT/msvcp140.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir));

                case C.EBit.SixtyFour:
                    return Bam.Core.TokenizedString.Create("$(0)/VC/redist/x64/Microsoft.VC140.CRT/msvcp140.dll", null, new Bam.Core.TokenizedStringArray(this.InstallDir));

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
        }
    }
}

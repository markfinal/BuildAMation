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
            this.Meta.Add("InstallDir", @"C:\Program Files (x86)\Microsoft Visual Studio 14.0");
            this.Meta.Add("PlatformToolset", "v140");
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
        InstallDir
        {
            get
            {
                return this.Meta["InstallDir"] as string;
            }
        }

        public string
        PlatformToolset
        {
            get
            {
                return this.Meta["PlatformToolset"] as string;
            }
        }

        Bam.Core.TokenizedString
        VisualCCommon.IRuntimeLibraryPathMeta.MSVCR(
            C.EBit depth)
        {
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    return Bam.Core.TokenizedString.CreateVerbatim(this.Meta["InstallDir"] + @"\VC\redist\x86\Microsoft.VC120.CRT\msvcr120.dll");

                case C.EBit.SixtyFour:
                    return Bam.Core.TokenizedString.CreateVerbatim(this.Meta["InstallDir"] + @"\VC\redist\x64\Microsoft.VC120.CRT\msvcr120.dll");

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
        }

        Bam.Core.TokenizedString VisualCCommon.IRuntimeLibraryPathMeta.MSVCP(C.EBit depth)
        {
            switch (depth)
            {
                case C.EBit.ThirtyTwo:
                    return Bam.Core.TokenizedString.CreateVerbatim(this.Meta["InstallDir"] + @"\VC\redist\x86\Microsoft.VC120.CRT\msvcp120.dll");

                case C.EBit.SixtyFour:
                    return Bam.Core.TokenizedString.CreateVerbatim(this.Meta["InstallDir"] + @"\VC\redist\x64\Microsoft.VC120.CRT\msvcp120.dll");

                default:
                    throw new Bam.Core.Exception("Unrecognized bit depth, {0}", depth);
            }
        }
    }
}

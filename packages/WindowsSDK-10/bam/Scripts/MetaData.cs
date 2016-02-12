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
namespace WindowsSDK
{
    public class MetaData :
        Bam.Core.PackageMetaData
    {
        private System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string, object>();

        public MetaData()
        {
            if (!Bam.Core.OSUtilities.IsWindowsHosting)
            {
                return;
            }

            using (var key = Bam.Core.Win32RegistryUtilities.Open32BitLMSoftwareKey(@"Microsoft\Windows Kits\Installed Roots"))
            {
                if (null == key)
                {
                    throw new Bam.Core.Exception("Windows SDKs were not installed");
                }

                var installPath10 = key.GetValue("KitsRoot10") as string;
                Bam.Core.Log.DebugMessage("Windows 10 SDK installation folder is {0}", installPath10);
                this.InstallDirSDK10 = Bam.Core.TokenizedString.CreateVerbatim(installPath10);

                // TODO: how can this be queried programmatically?
                // especially when not running Windows 10?
                this.SpecificVersion10 = Bam.Core.TokenizedString.CreateVerbatim("10.0.10240.0");

                var installPath81 = key.GetValue("KitsRoot81") as string;
                Bam.Core.Log.DebugMessage("Windows 8.1 SDK installation folder is {0}", installPath81);
                this.InstallDirSDK81 = Bam.Core.TokenizedString.CreateVerbatim(installPath81);
                return;
            }

            throw new Bam.Core.Exception("Unable to locate Windows SDK registry keys");
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
        InstallDirSDK10
        {
            get
            {
                return this.Meta["InstallDirWinSDK10"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["InstallDirWinSDK10"] = value;
            }
        }

        public Bam.Core.TokenizedString
        InstallDirSDK81
        {
            get
            {
                return this.Meta["InstallDirWinSDK81"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["InstallDirWinSDK81"] = value;
            }
        }

        public Bam.Core.TokenizedString
        SpecificVersion10
        {
            get
            {
                return this.Meta["SpecificVersion10"] as Bam.Core.TokenizedString;
            }

            private set
            {
                this.Meta["SpecificVersion10"] = value;
            }
        }
    }
}

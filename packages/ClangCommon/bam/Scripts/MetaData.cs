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
namespace ClangCommon
{
    public abstract class MetaData :
        Bam.Core.PackageMetaData,
        C.IToolchainDiscovery
    {
        protected System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string,object>();

        protected MetaData(
            string lastUpgradeCheck,
            Bam.Core.StringArray expectedSDKs)
        {
            if (!Bam.Core.OSUtilities.IsOSXHosting)
            {
                return;
            }

            this.Meta.Add("LastUpgradeCheck", lastUpgradeCheck);

            this.SDK = ClangCommon.ConfigureUtilities.SetSDK(expectedSDKs, this.Contains("SDK") ? this.SDK : null);
            if (!this.Contains("MinVersion"))
            {
                this.MinimumVersionSupported = "macosx10.6";
            }
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

        public string SDK
        {
            get
            {
                return this.Meta["SDK"] as string;
            }

            set
            {
                this.Meta["SDK"] = value;
            }
        }

        public string MinimumVersionSupported
        {
            get
            {
                return this.Meta["MinVersion"] as string;
            }

            set
            {
                this.Meta["MinVersion"] = value;
            }
        }

        public string LastUpgradeCheck
        {
            get
            {
                return this.Meta["LastUpgradeCheck"] as string;
            }
        }

        public string SDKPath
        {
            get
            {
                return this.Meta["SDKPath"] as string;
            }

            private set
            {
                this.Meta["SDKPath"] = value;
            }
        }

        // this is the clang version
        public abstract int CompilerMajorVersion
        {
            get;
        }

        void
        C.IToolchainDiscovery.discover(
            C.EBit? depth)
        {
            if (this.Contains("SDKPath"))
            {
                return;
            }
            this.SDKPath = ClangCommon.ConfigureUtilities.GetSDKPath(this.SDK);
            Bam.Core.Log.Info("Using {0} and {1} SDK installed at {2}",
                ClangCommon.ConfigureUtilities.GetClangVersion(this.SDK),
                this.SDK,
                this.SDKPath
            );
        }
    }
}

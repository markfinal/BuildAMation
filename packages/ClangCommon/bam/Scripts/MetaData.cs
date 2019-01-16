#region License
// Copyright (c) 2010-2019, Mark Final
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
        protected System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string, object>();
        private readonly Bam.Core.StringArray expectedSDKs;

        protected MetaData(
            string lastUpgradeCheck,
            Bam.Core.StringArray expectedSDKs,
            int pbxprojObjectVersion = 46)
        {
            this.LastUpgradeCheck = lastUpgradeCheck;
            this.PbxprojObjectVersion = pbxprojObjectVersion;
            this.expectedSDKs = expectedSDKs;
        }

        public override object this[string index] => this.Meta[index];

        public override bool
        Contains(
            string index) => this.Meta.ContainsKey(index);

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

        public string MacOSXMinimumVersionSupported
        {
            get
            {
                return this.Meta["MacOSXMinVersion"] as string;
            }

            set
            {
                this.Meta["MacOSXMinVersion"] = value;
            }
        }

        public string LastUpgradeCheck
        {
            get
            {
                return this.Meta["LastUpgradeCheck"] as string;
            }

            private set
            {
                this.Meta["LastUpgradeCheck"] = value;
            }
        }

        public int PbxprojObjectVersion
        {
            get
            {
                return (int)this.Meta["PbxprojObjectVersion"];
            }

            private set
            {
                this.Meta["PbxprojObjectVersion"] = value;
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

        public C.ToolchainVersion ToolchainVersion
        {
            get
            {
                if (!this.Meta.ContainsKey("ToolchainVersion"))
                {
                    (this as C.IToolchainDiscovery).discover(null);
                }
                return this.Meta["ToolchainVersion"] as C.ToolchainVersion;
            }

            private set
            {
                this.Meta["ToolchainVersion"] = value;
            }
        }

        private C.ToolchainVersion
        GetCompilerVersion()
        {
            var contents = new System.Text.StringBuilder();
            contents.AppendLine("__clang_major__");
            contents.AppendLine("__clang_minor__");
            contents.AppendLine("__clang_patchlevel__");
            var temp_file = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(temp_file, contents.ToString());
            var sdk = this.SDK;
            var result = Bam.Core.OSUtilities.RunExecutable(
                ConfigureUtilities.XcrunPath,
                $"--sdk {sdk} clang -E -P -x c {temp_file}"
            );
            var version = result.StandardOutput.Split(System.Environment.NewLine);
            if (version.Length != 3)
            {
                throw new Bam.Core.Exception(
                    $"Expected 3 lines: major, minor, patchlevel; instead got {version.Length} and {result.StandardOutput}"
                );
            }
            return ClangCommon.ToolchainVersion.FromComponentVersions(
                System.Convert.ToInt32(version[0]),
                System.Convert.ToInt32(version[1]),
                System.Convert.ToInt32(version[2])
            );
        }

        void
        C.IToolchainDiscovery.discover(
            C.EBit? depth)
        {
            if (this.Contains("SDKPath"))
            {
                return;
            }

            try
            {

                try
                {
                    this.SDK = ClangCommon.ConfigureUtilities.SetSDK(
                        this.expectedSDKs,
                        this.Contains("SDK") ? this.SDK : null
                    );

                    this.ToolchainVersion = this.GetCompilerVersion();

                    if (!this.Contains("MacOSXMinVersion"))
                    {
                        var isXcode10 = this.ToolchainVersion.AtLeast(ClangCommon.ToolchainVersion.Xcode_10);
                        if (isXcode10)
                        {
                            // Xcode 10 now requires 10.9+, and only libc++
                            this.MacOSXMinimumVersionSupported = "10.9";
                        }
                        else
                        {
                            // 10.7 is the minimum version required for libc++ currently
                            this.MacOSXMinimumVersionSupported = "10.7";
                        }
                    }
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    if (Bam.Core.OSUtilities.IsOSXHosting)
                    {
                        throw;
                    }
                    // arbitrary choice for non-macOS platforms
                    this.SDK = "macos10.13";
                    this.MacOSXMinimumVersionSupported = "10.13";
                }

                this.SDKPath = ClangCommon.ConfigureUtilities.GetSDKPath(this.SDK);
                Bam.Core.Log.Info(
                    $"Using {ClangCommon.ConfigureUtilities.GetClangVersion(this.SDK)} and {this.SDK} SDK installed at {this.SDKPath}"
                );
            }
            catch (System.InvalidOperationException)
            {
                if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    throw;
                }

                this.SDKPath = "";
            }
        }
    }
}

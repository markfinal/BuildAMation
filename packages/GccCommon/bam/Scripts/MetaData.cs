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
using System.Linq;
namespace GccCommon
{
    public abstract class MetaData :
        Bam.Core.PackageMetaData,
        C.IToolchainDiscovery
    {
        private System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string,object>();

        protected MetaData()
        {
            if (!Bam.Core.OSUtilities.IsLinuxHosting)
            {
                return;
            }

            this.Meta.Add("ExpectedMajorVersion", this.CompilerMajorVersion);
            this.Meta.Add("ExpectedMinorVersion", this.CompilerMinorVersion);
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

        private void
        ValidateGcc()
        {
            if (!this.Meta.ContainsKey("GccPath"))
            {
                throw new Bam.Core.Exception("Could not find gcc. Was the package installed?");
            }
            var gccLocation = this.Meta["GccPath"] as string;
            var gccVersion = this.Meta["GccVersion"] as string[];
            if (System.Convert.ToInt32(gccVersion[0]) != (int)this.Meta["ExpectedMajorVersion"])
            {
                throw new Bam.Core.Exception("{0} reports version {1}. Expected {2}.{3}", gccLocation, System.String.Join(".", gccVersion), this.Meta["ExpectedMajorVersion"], this.Meta["ExpectedMinorVersion"]);
            }
            if (System.Convert.ToInt32(gccVersion[1]) != (int)this.Meta["ExpectedMinorVersion"])
            {
                throw new Bam.Core.Exception("{0} reports version {1}. Expected {2}.{3}", gccLocation, System.String.Join(".", gccVersion), this.Meta["ExpectedMajorVersion"], this.Meta["ExpectedMinorVersion"]);
            }
        }

        private void
        ValidateGxx()
        {
            if (!this.Meta.ContainsKey("G++Path"))
            {
                throw new Bam.Core.Exception("Could not find g++. Was the package installed?");
            }
            var gxxLocation = this.Meta["G++Path"] as string;
            var gxxVersion = this.Meta["G++Version"] as string[];
            if (System.Convert.ToInt32(gxxVersion[0]) != (int)this.Meta["ExpectedMajorVersion"])
            {
                throw new Bam.Core.Exception("{0} reports version {1}. Expected {2}.{3}", gxxLocation, System.String.Join(".", gxxVersion), this.Meta["ExpectedMajorVersion"], this.Meta["ExpectedMinorVersion"]);
            }
            if (System.Convert.ToInt32(gxxVersion[1]) != (int)this.Meta["ExpectedMinorVersion"])
            {
                throw new Bam.Core.Exception("{0} reports version {1}. Expected {2}.{3}", gxxLocation, System.String.Join(".", gxxVersion), this.Meta["ExpectedMajorVersion"], this.Meta["ExpectedMinorVersion"]);
            }
        }

        private void
        ValidateAr()
        {
            if (!this.Meta.ContainsKey("ArPath"))
            {
                throw new Bam.Core.Exception("Could not find ar. Was the package installed?");
            }
        }

        private void
        ValidateLd()
        {
            if (!this.Meta.ContainsKey("LdPath"))
            {
                throw new Bam.Core.Exception("Could not find ld. Was the package installed?");
            }
        }

        public string GccPath
        {
            get
            {
                this.ValidateGcc();
                return this.Meta["GccPath"] as string;
            }
        }

        public string GxxPath
        {
            get
            {
                this.ValidateGxx();
                return this.Meta["G++Path"] as string;
            }
        }

        public string ArPath
        {
            get
            {
                this.ValidateAr();
                return this.Meta["ArPath"] as string;
            }
        }

        public string LdPath
        {
            get
            {
                this.ValidateLd();
                return this.Meta["LdPath"] as string;
            }
        }

        public abstract int
        CompilerMajorVersion
        {
            get;
        }

        public abstract int
        CompilerMinorVersion
        {
            get;
        }

        void
        C.IToolchainDiscovery.discover (
            C.EBit? depth)
        {
            if (this.Contains("GccPath"))
            {
                return;
            }
            var gccLocations = Bam.Core.OSUtilities.GetInstallLocation("gcc");
            if (null != gccLocations)
            {
                var location = gccLocations.First();
                this.Meta.Add("GccPath", location);
#if BAM_V2
                var gccVersion = Bam.Core.OSUtilities.RunExecutable(location, "-dumpversion").StandardOutput;
#else
                var gccVersion = Bam.Core.OSUtilities.RunExecutable(location, "-dumpversion");
#endif
                var gccVersionSplit = gccVersion.Split(new [] { '.' });
                this.Meta.Add("GccVersion", gccVersionSplit);
                Bam.Core.Log.Info("Using GCC version {0} installed at {1}", gccVersion, location);
            }

            var gxxLocations = Bam.Core.OSUtilities.GetInstallLocation("g++");
            if (null != gxxLocations)
            {
                var location = gxxLocations.First();
                this.Meta.Add("G++Path", location);
#if BAM_V2
                var gxxVersion = Bam.Core.OSUtilities.RunExecutable(location, "-dumpversion").StandardOutput.Split(new[] { '.' });
#else
                var gxxVersion = Bam.Core.OSUtilities.RunExecutable(location, "-dumpversion").Split(new [] { '.' });
#endif
                this.Meta.Add("G++Version", gxxVersion);
            }

            var arLocations = Bam.Core.OSUtilities.GetInstallLocation("ar");
            if (null != arLocations)
            {
                this.Meta.Add("ArPath", arLocations.First());
            }

            var ldLocations = Bam.Core.OSUtilities.GetInstallLocation("ld");
            if (null != ldLocations)
            {
                this.Meta.Add("LdPath", ldLocations.First());
            }
        }
    }
}

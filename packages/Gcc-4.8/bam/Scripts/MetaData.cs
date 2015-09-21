#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace Gcc
{
    public class MetaData :
        Bam.Core.IPackageMetaData
    {
        private System.Collections.Generic.Dictionary<string, object> Meta = new System.Collections.Generic.Dictionary<string,object>();

        public MetaData()
        {
            this.Meta.Add("ExpectedMajorVersion", 4);
            this.Meta.Add("ExpectedMinorVersion", 8);

            var gccLocation = GccCommon.ConfigureUtilities.GetInstallLocation("gcc");
            if (null == gccLocation)
            {
                return;
            }

            this.Meta.Add("InstallPath", System.IO.Path.GetDirectoryName(gccLocation));
            this.Meta.Add("GccPath", gccLocation);
            var gccVersion = GccCommon.ConfigureUtilities.RunExecutable(gccLocation, "-dumpversion").Split(new [] {'.'});
            this.Meta.Add("GccVersion", gccVersion);

            var gxxLocation = GccCommon.ConfigureUtilities.RunExecutable("which", "g++");
            if (null == gxxLocation)
            {
                return;
            }
            this.Meta.Add("G++Path", gxxLocation);
        }

        object Bam.Core.IPackageMetaData.this[string index]
        {
            get
            {
                return this.Meta[index];
            }
        }

        bool Bam.Core.IPackageMetaData.Contains(
            string index)
        {
            return this.Meta.ContainsKey(index);
        }

        public void
        ValidateInstallPath()
        {
            if (!this.Meta.ContainsKey("InstallPath"))
            {
                throw new Bam.Core.Exception("Could not find gcc. Was the package installed?");
            }
        }

        public void
        ValidateVersion()
        {
            var gccLocation = this.GccPath;
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

        public string InstallPath
        {
            get
            {
                return this.Meta["InstallPath"] as string;
            }
        }

        public string GccPath
        {
            get
            {
                return this.Meta["GccPath"] as string;
            }
        }

        public string GxxPath
        {
            get
            {
                return this.Meta["G++Path"] as string;
            }
        }
    }
}

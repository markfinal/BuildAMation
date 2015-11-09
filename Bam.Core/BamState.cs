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
using System.Linq;
namespace Bam.Core
{
    public class BamState
    {
        private static void
        GetAssemblyVersionData(
            out System.Version assemblyVersion,
            out string productVersion)
        {
            var coreAssembly = System.Reflection.Assembly.GetAssembly(typeof(BamState));
            assemblyVersion = coreAssembly.GetName().Version;
            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(coreAssembly.Location);
            var pv = versionInfo.ProductVersion.Trim();
            if (string.IsNullOrEmpty(pv))
            {
                // some Mono implementations only gather the product major/minor/build strings from the assembly
                productVersion = System.String.Format("{0}.{1}", versionInfo.ProductMajorPart, versionInfo.ProductMinorPart);
            }
            else
            {
                productVersion = pv;
            }
        }

        private static string
        GetBamDirectory()
        {
            var bamAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var rm = new System.Resources.ResourceManager(System.String.Format("{0}.PackageInfoResources", bamAssembly.GetName().Name), bamAssembly);
            // TODO: would be nice to check in advance if any exist
            try
            {
                return rm.GetString("BamInstallDir");
            }
            catch (System.Resources.MissingManifestResourceException)
            {
                // this assumes running an executable from the BAM! installation folder
                return System.IO.Path.GetDirectoryName(bamAssembly.Location);
            }
        }

        private static string
        GetWorkingDirectory()
        {
            var bamAssembly = System.Reflection.Assembly.GetEntryAssembly();
            var rm = new System.Resources.ResourceManager(System.String.Format("{0}.PackageInfoResources", bamAssembly.GetName().Name), bamAssembly);
            // TODO: would be nice to check in advance if any exist
            try
            {
                return rm.GetString("WorkingDir");
            }
            catch (System.Resources.MissingManifestResourceException)
            {
                return System.IO.Directory.GetCurrentDirectory();
            }
        }

        public BamState()
        {
            System.Version assemblyVersion;
            string productVersion;
            GetAssemblyVersionData(out assemblyVersion, out productVersion);

            this.RunningMono = (System.Type.GetType("Mono.Runtime") != null);
            this.ExecutableDirectory = GetBamDirectory();
            this.Version = assemblyVersion;
            this.VersionString = productVersion;
            this.WorkingDirectory = GetWorkingDirectory();

            // TODO: commented out as the TargetFrameworkAttribute was only introduced in CLR 4
#if false
            var targetFramework = bamAssembly.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false);
            var targetFrameworkName = (targetFramework[0] as System.Runtime.Versioning.TargetFrameworkAttribute).FrameworkName;
            Add<string>("BuildAMation", "TargetFramework", targetFrameworkName);
            var targetFrameworkNameSplit = targetFrameworkName.Split('=');
            Add<string>("BuildAMation", "CSharpCompilerVersion", targetFrameworkNameSplit[1]);
#endif
        }

        public string ExecutableDirectory
        {
            get;
            private set;
        }

        public System.Version Version
        {
            get;
            private set;
        }

        public string VersionString
        {
            get;
            private set;
        }

        public bool RunningMono
        {
            get;
            private set;
        }

        public string WorkingDirectory
        {
            get;
            private set;
        }    }
}

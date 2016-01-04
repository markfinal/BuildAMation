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
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Inspect the current configuration state of BuildAMation (Bam), such as the version number,
    /// where Bam is being executed, which version of the .NET framework is being targeted.
    /// </summary>
    public class BamState
    {
        private static void
        GetAssemblyVersionData(
            out System.Version assemblyVersion,
            out string productVersion,
            out string targetFrameworkName)
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
            var targetFrameworkAttributes = coreAssembly.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false);
            if (targetFrameworkAttributes.Length > 0)
            {
                targetFrameworkName = (targetFrameworkAttributes[0] as System.Runtime.Versioning.TargetFrameworkAttribute).FrameworkName;
            }
            else
            {
                targetFrameworkName = null;
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

        /// <summary>
        /// Create an instance of the class. Although there is only going to be one instance created,
        /// this is not a singleton, as BamState is attached to the Graph.
        /// </summary>
        public BamState()
        {
            System.Version assemblyVersion;
            string productVersion;
            string targetFrameworkName;
            GetAssemblyVersionData(out assemblyVersion, out productVersion, out targetFrameworkName);

            this.RunningMono = (System.Type.GetType("Mono.Runtime") != null);
            this.ExecutableDirectory = GetBamDirectory();
            this.WorkingDirectory = GetWorkingDirectory();

            this.Version = assemblyVersion;
            this.VersionString = productVersion;
            this.TargetFrameworkVersion = targetFrameworkName;
        }

        /// <summary>
        /// Obtains the directory containing the Bam assemblies.
        /// </summary>
        /// <value>Bam assembly directory path</value>
        public string ExecutableDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtains the version of Bam in use.
        /// </summary>
        /// <value>System.Version representation of the Bam version.</value>
        public System.Version Version
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtains a string representation of the version of Bam in use.
        /// </summary>
        /// <value>The version string.</value>
        public string VersionString
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether Bam is running through Mono.
        /// </summary>
        /// <value><c>true</c> if running mono; otherwise, <c>false</c>.</value>
        public bool RunningMono
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtains the working directory in which Bam is being executed.
        /// </summary>
        /// <value>The working directory.</value>
        public string WorkingDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Retrieves the .NET framework version being targeted for package builds.
        /// </summary>
        /// <value>The target framework version.</value>
        public string TargetFrameworkVersion
        {
            get;
            private set;
        }
    }
}

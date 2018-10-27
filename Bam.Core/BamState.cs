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
using System.Reflection;
namespace Bam.Core
{
    /// <summary>
    /// Inspect the current configuration state of BuildAMation (Bam), such as the version number,
    /// where Bam is being executed, which version of the .NET framework is being targeted.
    /// </summary>
    public class BamState :
        ISemanticVersion
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
            targetFrameworkName = coreAssembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>().FrameworkName;
        }

        private static string
        GetBamDirectory()
        {
            // must check the ENTRY assembly
            // - in normal runs, this is bam!
            // - in debug runs, this is the procedurally generated app
            // - in unittest runs, this is the unit test assembly
            var bamAssembly = System.Reflection.Assembly.GetEntryAssembly();
            try
            {
                var rm = new System.Resources.ResourceManager(System.String.Format("{0}.PackageInfoResources", bamAssembly.GetName().Name), bamAssembly);
                return rm.GetString("BamInstallDir");
            }
            catch (System.Resources.MissingManifestResourceException)
            {
                var basename = System.IO.Path.GetFileNameWithoutExtension(bamAssembly.Location);
                if (basename.Contains("Bam"))
                {
                    return System.IO.Path.GetDirectoryName(bamAssembly.Location);
                }
                else
                {
                    return null; // probably the unittests
                }
            }
        }

        private static string
        GetWorkingDirectory()
        {
            // must check the ENTRY assembly
            // - in normal runs, this is bam!
            // - in debug runs, this is the procedurally generated app
            // - in unittest runs, this is the unit test assembly
            var bamAssembly = System.Reflection.Assembly.GetEntryAssembly();
            try
            {
                var rm = new System.Resources.ResourceManager(System.String.Format("{0}.PackageInfoResources", bamAssembly.GetName().Name), bamAssembly);
                return rm.GetString("WorkingDir");
            }
            catch (System.Resources.MissingManifestResourceException)
            {
                var basename = System.IO.Path.GetFileNameWithoutExtension(bamAssembly.Location);
                if (basename.Contains("Bam"))
                {
                    return System.IO.Directory.GetCurrentDirectory();
                }
                else
                {
                    return null; // probably the unittests
                }
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

            this.ExecutableDirectory = GetBamDirectory();
            this.WorkingDirectory = GetWorkingDirectory();

            this.Version = assemblyVersion;
            this.VersionString = productVersion;
            this.TargetFrameworkVersion = targetFrameworkName;

            this.preBuildTasks = new Array<System.Threading.Tasks.Task>();

            this.BuildStartTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
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

        /// <summary>
        /// Retrieve the time that bam was launched.
        /// </summary>
        public System.DateTime BuildStartTime
        {
            get;
            private set;
        }

        int? ISemanticVersion.MajorVersion
        {
            get
            {
                return this.Version.Major;
            }
        }

        int? ISemanticVersion.MinorVersion
        {
            get
            {
                return this.Version.Minor;
            }
        }

        int? ISemanticVersion.PatchVersion
        {
            get
            {
                return this.Version.Build;
            }
        }

        private readonly Array<System.Threading.Tasks.Task> preBuildTasks;

        /// <summary>
        /// Append a task from an async method that must be completed before builds start.
        /// </summary>
        /// <param name="task"></param>
        public void
        AppendPreBuildTask(
            System.Threading.Tasks.Task task)
        {
            lock (this.preBuildTasks)
            {
                Log.MessageAll($"Adding task {task.ToString()}");
                this.preBuildTasks.Add(task);
            }
        }

        /// <summary>
        /// Wait on all prebuild tasks to complete. This is a blocking function.
        /// </summary>
        public void
        WaitOnAllPreBuildTasks()
        {
            Log.MessageAll($"*** WAITING ON {this.preBuildTasks.Count} TASKS");
            System.Threading.Tasks.Task.WaitAll(this.preBuildTasks.ToArray());
        }
    }
}

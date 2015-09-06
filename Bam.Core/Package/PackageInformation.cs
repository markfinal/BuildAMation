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
namespace Bam.Core
{
#if true
#else
    public class PackageInformation :
        System.IComparable
    {
        private
        PackageInformation(
            string name,
            string version)
        {
            this.Identifier = new PackageIdentifier(name, version);
            this.IsBuilder = false;

            throw new Exception("Who calls this constructor?");
        }

        public
        PackageInformation(
            PackageIdentifier id)
        {
            this.Identifier = id;
            this.IsBuilder = false;
        }

        public PackageIdentifier Identifier
        {
            get;
            private set;
        }

        public string Name
        {
            get
            {
                return this.Identifier.Name;
            }
        }

        public string Version
        {
            get
            {
                return this.Identifier.Version;
            }
        }

        public bool IsBuilder
        {
            get;
            set;
        }

        private string ScriptsDirectory
        {
            get
            {
                var scriptsDirectory = System.IO.Path.Combine(this.Identifier.Path, "Scripts");
                return scriptsDirectory;
            }
        }

        public string ProjectDirectory
        {
            get
            {
                var dir = System.IO.Path.Combine(this.Identifier.Path, "BamProject");
                return dir;
            }
        }

        public string DebugProjectFilename
        {
            get
            {
                var debugProjectFilename = System.IO.Path.Combine(this.ProjectDirectory, System.String.Format("{0}.csproj", this.FullName));
                return debugProjectFilename;
            }
        }

        public StringArray Scripts
        {
            get
            {
                var scripts = new StringArray();
                if (System.IO.Directory.Exists(this.ScriptsDirectory))
                {
                    var files = System.IO.Directory.GetFiles(this.ScriptsDirectory, "*.cs", System.IO.SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        scripts.AddRange(files);
                    }
                    if (0 == scripts.Count)
                    {
                        scripts = null;
                    }
                }
                return scripts;
            }
        }

        public StringArray BuilderScripts
        {
            get
            {
                var builderNames = new StringArray();

                var builderPackage = State.BuilderPackage;
                if (null != builderPackage)
                {
                    builderNames.Add(builderPackage.Name);
                }
                else
                {
                    foreach (var package in State.PackageInfo)
                    {
                        if (BuilderUtilities.IsBuilderPackage(package.Name))
                        {
                            builderNames.Add(package.Name);
                        }
                    }
                }

                var builderScripts = new StringArray();
                foreach (var builderName in builderNames)
                {
                    var builderDirectory = System.IO.Path.Combine(this.Identifier.Path, builderName);
                    if (System.IO.Directory.Exists(builderDirectory))
                    {
                        var files = System.IO.Directory.GetFiles(builderDirectory, "*.cs");
                        if (files.Length > 0)
                        {
                            builderScripts.AddRange(files);
                        }
                    }
                }

                if (0 == builderScripts.Count)
                {
                    return null;
                }

                return builderScripts;
            }
        }

        public string BuildDirectory
        {
            get
            {
                var buildRoot = Core.State.BuildRoot;
                var packageBuildDirectory = System.IO.Path.Combine(buildRoot, this.FullName);
                return packageBuildDirectory;
            }
        }

        public Location BuildDirectoryLocation
        {
            get
            {
                var buildDirectory = State.BuildRootLocation.SubDirectory(this.FullName);
                return buildDirectory;
            }
        }

        public string FullName
        {
            get
            {
                var fullName = this.Identifier.ToString("-");
                return fullName;
            }
        }

        public override string
        ToString()
        {
            return this.FullName;
        }

        int
        System.IComparable.CompareTo(
            object obj)
        {
            var objAs = obj as PackageInformation;
            int compared = this.FullName.CompareTo(objAs.FullName);
            return compared;
        }
    }
#endif
}

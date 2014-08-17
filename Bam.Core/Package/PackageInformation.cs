#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
namespace Bam.Core
{
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
}

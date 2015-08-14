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
    public class PackageIdentifier :
        System.IComparable
    {
        public
        PackageIdentifier(
            string name,
            string version) : this(name, version, true)
        {}

        public
        PackageIdentifier(
            string name,
            string version,
            bool validatePackageLocation)
        {
            if (null == name)
            {
                throw new Exception("Invalid name");
            }
            if (null == version)
            {
                throw new Exception("Invalid version");
            }

            this.Name = name;
            this.Version = version;
            this.PlatformFilter = EPlatform.All;
            if (validatePackageLocation)
            {
                this.Root = this.LocateRoot();
                this.Location = DirectoryLocation.Get(this.Path);
            }
        }

        public string Name
        {
            get;
            private set;
        }

        public string Version
        {
            get;
            private set;
        }

        public DirectoryLocation Root
        {
            get;
            private set;
        }

        public EPlatform PlatformFilter
        {
            get;
            set;
        }

        public string DefinitionPathName
        {
            get
            {
                var scriptFile = System.IO.Path.Combine(this.Path, this.Name + ".xml");
                return scriptFile;
            }
        }

        public PackageDefinitionFile Definition
        {
            get;
            set;
        }

        public string ScriptPathName
        {
            get
            {
                var scriptFile = System.IO.Path.Combine(this.Path, this.Name + ".cs");
                return scriptFile;
            }
        }

        public string CompilationDefinition
        {
            get
            {
                var compilationDefine = System.String.Format("D_PACKAGE_{0}_{1}", this.Name, this.Version.Replace('.', '_').Replace('-', '_')).ToUpper();
                return compilationDefine;
            }
        }

        public bool
        MatchName(
            PackageIdentifier identifier,
            bool ignoreCase)
        {
            return this.MatchName(identifier.Name, ignoreCase);
        }

        public bool
        MatchName(
            string name,
            bool ignoreCase)
        {
            var match = (0 == System.String.Compare(this.Name, name, ignoreCase));
            return match;
        }

        public int
        MatchVersion(
            PackageIdentifier identifier,
            bool ignoreCase)
        {
            return this.MatchVersion(identifier.Version, ignoreCase);
        }

        public int
        MatchVersion(
            string version,
            bool ignoreCase)
        {
            int compare = System.String.Compare(this.Version, version, ignoreCase);
            return compare;
        }

        public bool
        Match(
            PackageIdentifier identifier,
            bool ignoreCase)
        {
            var match = this.MatchName(identifier, ignoreCase) && (0 == this.MatchVersion(identifier, ignoreCase));
            return match;
        }

        public bool
        Match(
            string name,
            string version,
            bool ignoreCase)
        {
            var match = this.MatchName(name, ignoreCase) && (0 == this.MatchVersion(version, ignoreCase));
            return match;
        }

        public bool
        ConvertVersionToDouble(
            out double version)
        {
            return double.TryParse(this.Version, out version);
        }

        public string
        ToString(
            string separator)
        {
            var identifierString = System.String.Format("{0}{1}{2}", this.Name, separator, this.Version);
            return identifierString;
        }

        public override string
        ToString()
        {
            return this.ToString("-");
        }

        public DirectoryLocation Location
        {
            get;
            private set;
        }

        public string Path
        {
            get
            {
                var rootedPath = System.IO.Path.Combine(this.Root.AbsolutePath, this.Name);
                rootedPath = System.IO.Path.Combine(rootedPath, this.Version);
                return rootedPath;
            }
        }

        int
        System.IComparable.CompareTo(
            object obj)
        {
            var objAs = obj as PackageIdentifier;
            string left = this.ToString("-");
            string right = objAs.ToString("-");
            int compared = left.CompareTo(right);
            return compared;
        }

        private DirectoryLocation
        LocateRoot()
        {
            var rootContainingPackage = false;
            foreach (var root in State.PackageRoots)
            {
                var packageDirectory = System.IO.Path.Combine(root.AbsolutePath, this.Name);
                if (System.IO.Directory.Exists(packageDirectory))
                {
                    rootContainingPackage = true;
                }

                var versionDirectory = System.IO.Path.Combine(packageDirectory, this.Version);
                if (System.IO.Directory.Exists(versionDirectory))
                {
                    return root;
                }
            }

            var message = new System.Text.StringBuilder();
            if (!rootContainingPackage)
            {
                message.AppendFormat("Unable to locate package '{0}' in any registered package roots:\n", this.ToString("-"));
                foreach (var root in State.PackageRoots)
                {
                    message.AppendFormat("{0}\n", root);
                }
            }
            else
            {
                message.AppendFormat("Package '{0}' found in registered roots below, but not the specific version '{1}':\n",
                                     this.Name, this.Version);
                foreach (var root in State.PackageRoots)
                {
                    message.AppendFormat("{0}\n", root);
                }
            }
            throw new Exception(message.ToString());
        }

        public bool IsDefaultVersion
        {
            set;
            get;
        }
    }
}

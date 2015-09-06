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
    public sealed class PackageInformationCollection :
        System.Collections.Generic.ICollection<PackageInformation>
    {
        private System.Collections.Generic.List<PackageInformation> list = new System.Collections.Generic.List<PackageInformation>();

        public PackageInformation MainPackage
        {
            get
            {
                if (0 == this.list.Count)
                {
                    throw new Exception("No packages have been specified or identified. Please run 'bam' from a package directory");
                }

                var mainPackage = this.list[0];
                return mainPackage;
            }
        }

        public void
        Add(
            PackageInformation item,
            bool allowMultipleVersions)
        {
            if (!allowMultipleVersions && this.Contains(item))
            {
                throw new Exception("Package '{0}' already present in the collection. Could not add '{1}'", this[item.Name].FullName, item.FullName);
            }

            this.list.Add(item);
            State.PackageRoots.AddUnique(item.Identifier.Root);
        }

        public void
        Add(
            PackageInformation item)
        {
            this.Add(item, false);
        }

        public void
        Clear()
        {
            this.list.Clear();
        }

        public bool
        Contains(
            PackageInformation item)
        {
            var ignoreCase = true;

            foreach (var package in this.list)
            {
                if (package.Identifier.MatchName(item.Identifier, ignoreCase))
                {
                    // are both versions numbers?
                    double currentVersion;
                    double incomingVersion;
                    if (package.Identifier.ConvertVersionToDouble(out currentVersion) && item.Identifier.ConvertVersionToDouble(out incomingVersion))
                    {
                        if (currentVersion < incomingVersion)
                        {
                            Log.DebugMessage("Package '{0}': version '{1}' replaces '{2}'", package.Name, item.Version, package.Version);
                            this.list.Remove(package);
                            this.list.Add(item);
                        }

                        return true;
                    }
                    else
                    {
                        // not numbers, try a string comparison
                        int versionComparison = package.Identifier.MatchVersion(item.Identifier, ignoreCase);
                        if (0 == versionComparison)
                        {
                            if (0 == System.String.Compare(package.Identifier.Root.AbsolutePath, item.Identifier.Root.AbsolutePath, ignoreCase))
                            {
                                return true;
                            }
                            else
                            {
                                throw new Exception("Package '{0}-{1}' found in roots '{2}' and '{3}'", package.Name, package.Version, package.Identifier.Root, item.Identifier.Root);
                            }
                        }
                        else
                        {
                            if (0 == System.String.Compare(item.Version, "dev") || versionComparison < 0)
                            {
                                Log.DebugMessage("Package '{0}': version '{1}' replaces '{2}'", package.Name, item.Version, package.Version);
                                this.list.Remove(package);
                                this.list.Add(item);
                                return true;
                            }
                            else
                            {
                                throw new Exception("Package '{0}' with version '{1}' already specified; version '{2}' is older", package.Name, package.Version, item.Version);
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void
        CopyTo(
            PackageInformation[] array,
            int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public bool IsReadOnly
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool
        Remove(
            PackageInformation item)
        {
            if (this.Contains(item))
            {
                return this.list.Remove(item);
            }
            else
            {
                throw new Exception("Package '{0}' was not present in the collection", item.FullName);
            }
        }

        public System.Collections.Generic.IEnumerator<PackageInformation>
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public PackageInformation this[int index]
        {
            get
            {
                return this.list[index];
            }
        }

        public PackageInformation this[string name]
        {
            get
            {
                foreach (var package in this.list)
                {
                    var ignoreCase = false;
                    if (package.Identifier.MatchName(name, ignoreCase))
                    {
                        return package;
                    }
                }

                return null;
            }
        }

        public void
        Sort()
        {
            this.list.Sort();
        }

        public string
        ToString(
            string prefix,
            string suffix)
        {
            string message = null;
            foreach (var package in this.list)
            {
                message += prefix + package.ToString() + suffix;
            }
            return message;
        }

        public override string
        ToString()
        {
            return this.ToString(null, " ");
        }
    }
#endif
}

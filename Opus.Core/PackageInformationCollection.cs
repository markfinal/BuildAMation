// <copyright file="PackageInformationCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class PackageInformationCollection : System.Collections.Generic.ICollection<PackageInformation>
    {
        private System.Collections.Generic.List<PackageInformation> list = new System.Collections.Generic.List<PackageInformation>();

        public PackageInformation MainPackage
        {
            get
            {
                if (0 == this.list.Count)
                {
                    throw new Exception("No packages have been specified or identified. Please run Opus from a package directory");
                }

                PackageInformation mainPackage = this.list[0];
                return mainPackage;
            }
        }

        public void Add(PackageInformation item)
        {
            if (this.Contains(item))
            {
                throw new Exception(System.String.Format("Package '{0}' already present in the collection", item.FullName));
            }

            this.list.Add(item);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public bool Contains(PackageInformation item)
        {
            bool ignoreCase = true;

            foreach (PackageInformation package in this.list)
            {
                if (0 == System.String.Compare(package.Name, item.Name, ignoreCase))
                {
                    // are both versions numbers?
                    double currentVersion;
                    double incomingVersion;
                    if (double.TryParse(package.Version, out currentVersion) && double.TryParse(item.Version, out incomingVersion))
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
                        int versionComparison = System.String.Compare(package.Version, item.Version, ignoreCase);
                        if (0 == versionComparison)
                        {
                            if (0 == System.String.Compare(package.Root, item.Root, ignoreCase))
                            {
                                return true;
                            }
                            else
                            {
                                throw new Exception(System.String.Format("Package '{0}-{1}' found in roots '{2}' and '{3}'", package.Name, package.Version, package.Root, item.Root));
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
                                throw new Exception(System.String.Format("Package '{0}' with version '{1}' already specified; version '{2}' is older", package.Name, package.Version, item.Version));
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void CopyTo(PackageInformation[] array, int arrayIndex)
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

        public bool Remove(PackageInformation item)
        {
            if (this.Contains(item))
            {
                return this.list.Remove(item);
            }
            else
            {
                throw new Exception(System.String.Format("Package '{0}' was not present in the collection", item.FullName));
            }
        }

        public System.Collections.Generic.IEnumerator<PackageInformation> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
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
                foreach (PackageInformation package in this.list)
                {
                    if (package.Name == name)
                    {
                        return package;
                    }
                }

                return null;
            }
        }

        public void Sort()
        {
            this.list.Sort();
        }

        public string ToString(string prefix, string suffix)
        {
            string message = null;
            foreach (PackageInformation package in this.list)
            {
                message += prefix + package.ToString() + suffix;
            }
            return message;
        }

        public override string ToString()
        {
            return this.ToString(null, " ");
        }
    }
}
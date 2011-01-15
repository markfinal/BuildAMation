// <copyright file="DirectoryCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class DirectoryCollection : System.ICloneable, System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<PackageAndDirectoryPath> directoryList = new System.Collections.Generic.List<PackageAndDirectoryPath>();

        public object Clone()
        {
            DirectoryCollection clone = new DirectoryCollection();
            foreach (PackageAndDirectoryPath pap in this.directoryList)
            {
                clone.Add(pap.Package, pap.RelativePath.Clone() as string);
            }
            return clone;
        }

        private bool Contains(PackageAndDirectoryPath pap)
        {
            foreach (PackageAndDirectoryPath listPap in this.directoryList)
            {
                if (listPap.Package == pap.Package &&
                    listPap.RelativePath == pap.RelativePath)
                {
                    return true;
                }
            }

            return false;
        }

        public void Add(string absoluteDirectoryPath, bool checkForExistence)
        {
            if (checkForExistence && !System.IO.Directory.Exists(absoluteDirectoryPath))
            {
                throw new Opus.Core.Exception(System.String.Format("The directory '{0}' does not exist", absoluteDirectoryPath));
            }

            PackageAndDirectoryPath pap = new PackageAndDirectoryPath(null, absoluteDirectoryPath);
            if (this.Contains(pap))
            {
                Opus.Core.Log.DebugMessage("Absolute path '{0}' is already present in the list of directories", absoluteDirectoryPath);
            }
            else
            {
                this.directoryList.Add(pap);
            }
        }

        public void Add(Opus.Core.PackageInformation package, string relativePath)
        {
            PackageAndDirectoryPath pap = new PackageAndDirectoryPath(package, relativePath);
            if (this.Contains(pap))
            {
                Opus.Core.Log.DebugMessage("Relative path '{0}' is already present for package '{1}'", relativePath, package.FullName);
            }
            else
            {
                this.directoryList.Add(pap);
            }
        }

        public void AddRange(Opus.Core.PackageInformation package, string[] relativePaths)
        {
            foreach (string path in relativePaths)
            {
                this.Add(package, path);
            }
        }

        public void AddRange(Opus.Core.PackageInformation package, Opus.Core.StringArray relativePaths)
        {
            foreach (string path in relativePaths)
            {
                this.Add(package, path);
            }
        }

        public PackageAndDirectoryPath this[int index]
        {
            get
            {
                return this.directoryList[index];
            }
        }

        public int Count
        {
            get
            {
                return this.directoryList.Count;
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return new DirectoryCollectionEnumerator(this);
        }
    }
}
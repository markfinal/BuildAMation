// <copyright file="FileCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class FileCollection : System.ICloneable, System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<string> filePaths = new System.Collections.Generic.List<string>();

        public FileCollection()
        {
        }

        public FileCollection(params FileCollection[] collections)
        {
            foreach (FileCollection collection in collections)
            {
                foreach (string path in collection)
                {
                    this.Add(path);
                }
            }
        }

        public object Clone()
        {
            FileCollection clone = new FileCollection();
            foreach (string path in this.filePaths)
            {
                clone.Add(path);
            }
            return clone;
        }

        public void Add(string absolutePath)
        {
            this.filePaths.Add(absolutePath);
        }

        public void AddToFront(string absolutePath)
        {
            this.filePaths.Insert(0, absolutePath);
        }

        public string this[int index]
        {
            get
            {
                return this.filePaths[index];
            }
        }

        public int Count
        {
            get
            {
                return this.filePaths.Count;
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.filePaths.GetEnumerator();
        }

        public static FileCollection AddUsingWildcards(PackageInformation package, params string[] pathParts)
        {
            if (null == package)
            {
                throw new Exception("Package is null. Cannot evaluate wildcarded paths");
            }

            FileCollection collection = new FileCollection();

            Opus.Core.File path = new Opus.Core.File(pathParts);
            string[] files = System.IO.Directory.GetFiles(package.Directory, path.RelativePath, System.IO.SearchOption.AllDirectories);
            foreach (string file in files)
            {
                collection.Add(file);
            }

            return collection;
        }
    }
}
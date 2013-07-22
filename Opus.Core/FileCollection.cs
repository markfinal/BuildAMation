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
            foreach (var collection in collections)
            {
                foreach (string path in collection)
                {
                    this.Add(path);
                }
            }
        }

        public object Clone()
        {
            var clone = new FileCollection();
            clone.filePaths.AddRange(this.filePaths);
            return clone;
        }

        public void Add(string absolutePath)
        {
            this.filePaths.Add(absolutePath);
        }

        public void AddRange(StringArray absolutePathArray)
        {
            foreach (var path in absolutePathArray)
            {
                this.filePaths.Add(path);
            }
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

        // Note: this no longer applies a module's proxy path
        public void Include(Location root, params string[] pathSegments)
        {
            // TODO: replace with Location
            var paths = File.GetFiles(root.CachedPath, pathSegments);
            foreach (var path in paths)
            {
                this.filePaths.Add(path);
            }
        }

        // Note: this no longer applies a module's proxy path
        public void Exclude(Location root, params string[] pathSegments)
        {
            // TODO: replace with the Location
            var paths = File.GetFiles(root.CachedPath, pathSegments);
            foreach (var path in paths)
            {
                this.filePaths.Remove(path);
            }
        }

        // deprecated
        public void Include(object module, params string[] pathSegments)
        {
            var package = PackageUtilities.GetOwningPackage(module);
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", module.GetType().Namespace);
            }

            var packagePath = package.Identifier.Path;
            var proxyPath = (module as BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            var paths = File.GetFiles(packagePath, pathSegments);
            foreach (var path in paths)
            {
                this.filePaths.Add(path);
            }
        }

        // deprecated
        public void Exclude(object module, params string[] pathSegments)
        {
            var package = PackageUtilities.GetOwningPackage(module);
            if (null == package)
            {
                throw new Exception("Unable to locate package '{0}'", module.GetType().Namespace);
            }

            var packagePath = package.Identifier.Path;
            var proxyPath = (module as BaseModule).ProxyPath;
            if (null != proxyPath)
            {
                packagePath = proxyPath.Combine(package.Identifier);
            }

            var paths = File.GetFiles(packagePath, pathSegments);
            foreach (var path in paths)
            {
                this.filePaths.Remove(path);
            }
        }

        public void AddRelativePaths(string baseDirectory, string relativePath)
        {
            if (!System.IO.Directory.Exists(baseDirectory))
            {
                throw new Exception("Base directory '{0}' does not exist", baseDirectory);
            }

            var paths = File.GetFiles(baseDirectory, relativePath);
            foreach (var path in paths)
            {
                this.filePaths.Add(path);
            }
        }

        public Opus.Core.StringArray ToStringArray()
        {
            return new Opus.Core.StringArray(this.filePaths.ToArray());
        }
    }
}

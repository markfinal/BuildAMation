// <copyright file="FileCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class FileCollection : System.ICloneable, System.Collections.IEnumerable
    {
        private Array<Location> fileLocations = new Array<Location>();

        public FileCollection()
        {
        }

        public FileCollection(params FileCollection[] collections)
        {
            foreach (var collection in collections)
            {
                foreach (string path in collection)
                {
                    this.fileLocations.Add(FileLocation.Get(path, Location.EExists.Exists));
                }
            }
        }

        public object Clone()
        {
            var clone = new FileCollection();
            clone.fileLocations.AddRange(this.fileLocations);
            return clone;
        }

#if true
        public void Add(string path)
        {
            this.fileLocations.AddUnique(FileLocation.Get(path, Location.EExists.WillExist));
        }

        public void AddRange(StringArray paths)
        {
            foreach (var path in paths)
            {
                this.Add(path);
            }
        }
#else
        public void Add(string absolutePath)
        {
            // TODO: claim that it will exist, as these paths may not be exact
            this.fileLocations.Add(FileLocation.Get(absolutePath, Location.EExists.WillExist));
        }

        public void AddRange(StringArray absolutePathArray)
        {
            foreach (var path in absolutePathArray)
            {
                // TODO: claim that it will exist, as these paths may not be exact
                this.fileLocations.Add(FileLocation.Get(path, Location.EExists.WillExist));
            }
        }
#endif

        public Location this[int index]
        {
            get
            {
                return this.fileLocations[index];
            }
        }

        public int Count
        {
            get
            {
                return this.fileLocations.Count;
            }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.fileLocations.GetEnumerator();
        }

#if true
        public void Include(Location baseLocation, string pattern)
        {
            var scaffold = new ScaffoldLocation(baseLocation, pattern, ScaffoldLocation.ETypeHint.File);
            // TODO: this should be deferred until much later
            var files = scaffold.GetLocations();
            foreach (var file in files)
            {
                this.fileLocations.Add(file);
            }
        }
#else
#if true
        // Note: this no longer applies a module's proxy path
        public void Include(Location root, params string[] pathSegments)
        {
            var paths = File.GetFiles(root, pathSegments);
            foreach (var path in paths)
            {
                this.fileLocations.Add(path);
            }
        }

        // Note: this no longer applies a module's proxy path
        public void Exclude(Location root, params string[] pathSegments)
        {
            var paths = File.GetFiles(root, pathSegments);
            foreach (var path in paths)
            {
                this.fileLocations.Remove(path);
            }
        }
#else
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
                packagePath = proxyPath.Combine(package.Identifier.Location).CachedPath;
            }

            var paths = File.GetFiles(packagePath, pathSegments);
            foreach (var path in paths)
            {
                this.fileLocations.Add(path);
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
                packagePath = proxyPath.Combine(package.Identifier.Location).CachedPath;
            }

            var paths = File.GetFiles(packagePath, pathSegments);
            foreach (var path in paths)
            {
                this.fileLocations.Remove(path);
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
                this.fileLocations.Add(path);
            }
        }
#endif
#endif

        public StringArray ToStringArray()
        {
            var array = new StringArray();
            foreach (var location in this.fileLocations)
            {
                var locations = location.GetLocations();
                foreach (var loc in locations)
                {
                    array.Add(loc.AbsolutePath);
                }
            }
            return array;
        }
    }
}

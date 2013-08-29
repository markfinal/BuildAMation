// <copyright file="Locations.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public abstract class Location
    {
        protected Location(Location root, StringArray segments)
        {
            if (root.IsFile && segments.Count > 0)
            {
                throw new Exception("Cannot root a location from a file and with additional path segments");
            }

            this.Root = root;
            this.Module = null;
            this.Segments = segments;
            this.CachedPath = null;
            this.Proxy = null;
        }

        protected Location(Location root, ProxyModulePath proxy)
        {
            this.Root = root;
            this.Module = null;
            this.Segments = null;
            this.CachedPath = null;
            this.Proxy = proxy;
        }

        protected Location(BaseModule module, StringArray segments)
        {
            this.Root = null;
            this.Module = module;
            this.Segments = segments;
            this.CachedPath = null;
            this.Proxy = null;
        }

        protected Location(string absolutePath)
        {
            this.Root = null;
            this.Module = null;
            this.Segments = null;
            this.CachedPath = absolutePath;
            this.Proxy = null;
        }

        public abstract bool IsFile
        {
            get;
        }

        public bool MayContainWildcards
        {
            get;
            set;
        }

        private string CalculatePathFromRoot()
        {
            if (null != this.Segments)
            {
                var combinedPath = this.Root.CachedPath;
                foreach (var segment in this.Segments)
                {
                    combinedPath = System.IO.Path.Combine(combinedPath, segment);
                }
                if (this.MayContainWildcards)
                {
                    return combinedPath;
                }
                else
                {
                    return System.IO.Path.GetFullPath(combinedPath);
                }
            }
            else if (null != this.Proxy)
            {
                var newLocation = this.Proxy.Combine(this.Root);
                return newLocation.CachedPath;
            }
            else
            {
                throw new Exception("Do not know how to evaluate the path in this Location");
            }
        }

        private string cachedPath = null;
        public string CachedPath
        {
            get
            {
                if (null == this.cachedPath)
                {
                    if (null != this.Root)
                    {
                        this.cachedPath = this.CalculatePathFromRoot();
                    }
                    else
                    {
                        throw new Exception("Need to calculate cached path");
                    }
                }

                return this.cachedPath;
            }

            private set
            {
                this.cachedPath = value;
            }
        }

        private Location Root
        {
            get;
            set;
        }

#if false
        public string Path
        {
            get
            {
                if (null != this.CachedPath)
                {
                    return this.CachedPath;
                }

                var package = Opus.Core.PackageUtilities.GetOwningPackage(this.Module);
                if (null == package)
                {
                    throw new Opus.Core.Exception("Unable to locate package '{0}'", this.Module.GetType().Namespace);
                }

                var packagePath = package.Identifier.Path;
                var proxyPath = this.Module.ProxyPath;
                if (null != proxyPath)
                {
                    packagePath = proxyPath.Combine(package.Identifier);
                }

                var filePaths = Opus.Core.File.GetFiles(packagePath, this.Segments.ToArray());
                foreach (var path in filePaths)
                {
                    var objectFile = new ObjectFile();
                    (objectFile as Opus.Core.BaseModule).ProxyPath = (this as Opus.Core.BaseModule).ProxyPath;
                    objectFile.SourceFile.SetAbsolutePath(path);
                    this.list.Add(objectFile);
                }

            }
        }

        public abstract bool Exists
        {
            get;
        }
#endif

        private BaseModule Module
        {
            get;
            set;
        }

        private StringArray Segments
        {
            get;
            set;
        }

        private ProxyModulePath Proxy
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.cachedPath;
        }

        public abstract LocationDirectory SubDirectory(params string[] segments);
    }

    public sealed class LocationFile : Location
    {
        public LocationFile(LocationDirectory root, params string[] segments)
            : base(root, new StringArray(segments))
        {
        }

        public LocationFile(Location root, ProxyModulePath proxy)
            : base(root, proxy)
        {
        }

        public LocationFile(BaseModule module, params string[] segments)
            : base(module, new StringArray(segments))
        {
        }

        public LocationFile(string absolutePath)
            : base(absolutePath)
        {
        }

        public override bool IsFile
        {
            get
            {
                return true;
            }
        }

        public override LocationDirectory SubDirectory(params string[] segments)
        {
            throw new Exception("Cannot make a child directory from a file");
        }
    }

    public sealed class LocationDirectory : Location
    {
        public LocationDirectory(Location root, params string[] segments)
            : base(root, new StringArray(segments))
        {
        }

        public LocationDirectory(Location root, StringArray segments)
            : base(root, segments)
        {
        }

        public LocationDirectory(Location root, ProxyModulePath proxy)
            : base(root, proxy)
        {
        }

        public LocationDirectory(BaseModule module, params string[] segments)
            : base(module, new StringArray(segments))
        {
        }

        public LocationDirectory(string absolutePath)
            : base(absolutePath)
        {
        }

        public override bool IsFile
        {
            get
            {
                return false;
            }
        }

        public override LocationDirectory SubDirectory(params string[] segments)
        {
            return new LocationDirectory(this, segments);
        }
    }

    public sealed class LocationMap
    {
        private System.Collections.Generic.Dictionary<string, Location> map = new System.Collections.Generic.Dictionary<string, Location>();

        public Location this[string locationName]
        {
            get
            {
                return this.map[locationName];
            }

            set
            {
                this.map[locationName] = value;
            }
        }

        public bool Contains(string locationName)
        {
            return this.map.ContainsKey(locationName);
        }

        public override string ToString()
        {
            var repr = new System.Text.StringBuilder();
            foreach (var key in this.map.Keys)
            {
                // TODO: add the value too
                repr.AppendFormat("{0} ", key);
            }
            return repr.ToString();
        }
    }

    public class DeferredLocations
    {
        public DeferredLocations(Location root, params string[] pathSegments)
        {
            this.Deferred = new LocationDirectory(root, pathSegments);
            this.Deferred.MayContainWildcards = true;
        }

        public DeferredLocations(Location root, StringArray pathSegments)
        {
            this.Deferred = new LocationDirectory(root, pathSegments);
            this.Deferred.MayContainWildcards = true;
        }

        public DeferredLocations(string absolutePath)
        {
            this.Deferred = new LocationDirectory(absolutePath);
            this.Deferred.MayContainWildcards = true;
        }

        public Location Deferred
        {
            get;
            private set;
        }
    }

    public class DeferredLocationsComparer : System.Collections.Generic.IEqualityComparer<DeferredLocations>
    {
        #region IEqualityComparer<DeferredLocations> Members

        bool System.Collections.Generic.IEqualityComparer<DeferredLocations>.Equals(DeferredLocations x, DeferredLocations y)
        {
            bool equals = (x.Deferred.CachedPath.Equals(y.Deferred.CachedPath));
            return equals;
        }

        int System.Collections.Generic.IEqualityComparer<DeferredLocations>.GetHashCode(DeferredLocations obj)
        {
            return obj.Deferred.CachedPath.GetHashCode();
        }

        #endregion
    }
}

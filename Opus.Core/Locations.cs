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
        }

        protected Location(BaseModule module, StringArray segments)
        {
            this.Root = null;
            this.Module = module;
            this.Segments = segments;
            this.CachedPath = null;
        }

        protected Location(string absolutePath)
        {
            this.Root = null;
            this.Module = null;
            this.Segments = null;
            this.CachedPath = absolutePath;
        }

        public abstract bool IsFile
        {
            get;
        }

        private string CalculatePathFromRoot()
        {
            var combinedPath = this.Root.CachedPath;
            foreach (var segment in this.Segments)
            {
                combinedPath = System.IO.Path.Combine(combinedPath, segment);
            }
            return combinedPath;
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

        public Location this[string name]
        {
            get
            {
                return this.map[name];
            }

            set
            {
                this.map[name] = value;
            }
        }

        public bool Contains(string name)
        {
            return this.map.ContainsKey(name);
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
}

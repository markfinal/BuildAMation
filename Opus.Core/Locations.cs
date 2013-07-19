// <copyright file="Locations.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public abstract class Location
    {
        protected Location(LocationDirectory root, StringArray segments)
        {
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

        private string CachedPath
        {
            get;
            set;
        }

        private LocationDirectory Root
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
    }

    public sealed class LocationDirectory : Location
    {
        public LocationDirectory(LocationDirectory root, params string[] segments)
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
    }
}

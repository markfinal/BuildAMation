// <copyright file="Locations.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public abstract class Location
    {
        protected Location(BaseModule module, StringArray segments)
        {
            this.Module = module;
            this.Segments = segments;
            this.CachedPath = null;
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

                var filePaths = Opus.Core.File.GetFiles(packagePath, this.Segments);
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
        public LocationFile(BaseModule module, params string[] segments)
            : base(module, new StringArray(segments))
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
        public LocationDirectory(BaseModule module, params string[] segments)
            : base(module, new StringArray(segments))
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

        public Location this[string index]
        {
            get
            {
                return this.map[index];
            }

            set
            {
                this.map[index] = value;
            }
        }
    }
}
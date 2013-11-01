// <copyright file="Locations.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    /// <summary>
    /// Location is the abstract base class that can represent any of the higher form of Locations.
    /// </summary>
    public abstract class Location
    {
        public enum EExists
        {
            Exists,
            WillExist
        }

        public abstract Location SubDirectory(string subDirName);

        public virtual string AbsolutePath
        {
            get;
            protected set;
        }

        public abstract Array<Location> GetLocations();
    }

    /// <summary>
    /// DirectoryLocation represents a single directory on disk.
    /// It may or may not exist, but the default behaviour is to assume that it exists and this is tested.
    /// DirectoryLocations are cached internally, so only a single instance of a directory path exists.
    /// </summary>
    public sealed class DirectoryLocation : Location
    {
        private static System.Collections.Generic.Dictionary<int, DirectoryLocation> cache = new System.Collections.Generic.Dictionary<int, DirectoryLocation>();

        private DirectoryLocation(string absolutePath, Location.EExists exists)
        {
            if (exists == EExists.Exists)
            {
                if (!System.IO.Directory.Exists(absolutePath))
                {
                    throw new Exception("Directory '{0}' does not exist", absolutePath);
                }
            }
            this.AbsolutePath = absolutePath;
        }

        public static DirectoryLocation Get(string absolutePath, Location.EExists exists)
        {
            var hash = absolutePath.GetHashCode();
            if (cache.ContainsKey(hash))
            {
                return cache[hash];
            }
            var instance = new DirectoryLocation(absolutePath, exists);
            cache[hash] = instance;
            return instance;
        }

        public static DirectoryLocation Get(string absolutePath)
        {
            return Get(absolutePath, EExists.Exists);
        }

        public override Location SubDirectory(string subDirName)
        {
            var subDirectoryPath = System.IO.Path.Combine(this.AbsolutePath, subDirName);
            return Get(subDirectoryPath) as Location;
        }

        public override string ToString()
        {
            return System.String.Format("Directory '{0}'", this.AbsolutePath);
        }

        public override Array<Location> GetLocations()
        {
            return new Array<Location>(this);
        }
    }

    /// <summary>
    /// FileLocation represents a single file on disk.
    /// It may or may not exist, but the default behaviour is to assume that it exists and this is tested.
    /// FileLocations are cached internally, so only a single instance of a file path exists.
    /// </summary>
    public sealed class FileLocation : Location
    {
        private static System.Collections.Generic.Dictionary<int, FileLocation> cache = new System.Collections.Generic.Dictionary<int, FileLocation>();

        private FileLocation(string absolutePath, Location.EExists exists)
        {
            if (exists == EExists.Exists)
            {
                if (!System.IO.File.Exists(absolutePath))
                {
                    throw new Exception("File '{0}' does not exist", absolutePath);
                }
            }
            this.AbsolutePath = absolutePath;
        }

        public static FileLocation Get(string absolutePath, Location.EExists exists)
        {
            var hash = absolutePath.GetHashCode();
            if (cache.ContainsKey(hash))
            {
                return cache[hash];
            }
            var instance = new FileLocation(absolutePath, exists);
            cache[hash] = instance;
            return instance;
        }

        public static FileLocation Get(string absolutePath)
        {
            return Get(absolutePath, EExists.Exists);
        }

        public static FileLocation Get(Location baseLocation, string nonWildcardedFilename)
        {
            var locations = baseLocation.GetLocations();
            if (locations.Count > 1)
            {
                throw new Exception("Cannot resolve source Location to a single path");
            }
            var path = System.IO.Path.Combine(locations[0].AbsolutePath, nonWildcardedFilename);
            var hash = path.GetHashCode();
            if (cache.ContainsKey(hash))
            {
                return cache[hash];
            }
            var instance = new FileLocation(path, EExists.Exists);
            cache[hash] = instance;
            return instance;
        }

        public override Location SubDirectory(string subDirName)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return System.String.Format("File '{0}'", this.AbsolutePath);
        }

        public override Array<Location> GetLocations()
        {
            return new Array<Location>(this);
        }
    }

    /// <summary>
    /// ScaffoldLocation is an abstract representation of many real Locations on disk.
    /// ScaffoldLocations are constructed from a base Location, with a pattern added to it. This pattern may be wildcarded.
    /// Calling GetLocations() on a ScaffoldLocation will resolve the pattern to a list of real Locations.
    /// </summary>
    public sealed class ScaffoldLocation : Location
    {
        public enum ETypeHint
        {
            Directory,
            File
        }

        private ScaffoldLocation(ETypeHint typeHint)
        {
            this.TypeHint = typeHint;
            this.Results = new Array<Location>();
        }

        public ScaffoldLocation(Location baseLocation, string pattern, ETypeHint typeHint)
            : this(typeHint)
        {
            this.Base = baseLocation;
            this.Pattern = pattern;
        }

        public ScaffoldLocation(Location baseLocation, ProxyModulePath proxyPath, ETypeHint typeHint)
            : this(typeHint)
        {
            this.Base = baseLocation;
            this.ProxyPath = proxyPath;
        }

        public ETypeHint TypeHint
        {
            get;
            private set;
        }

        public Location Base
        {
            get;
            private set;
        }

        public string Pattern
        {
            get;
            private set;
        }

        private ProxyModulePath ProxyPath
        {
            get;
            set;
        }

        private void ResolveDirectory(Location directory)
        {
            if (null == this.Pattern)
            {
                if ((null == this.ProxyPath) || this.ProxyPath.Empty)
                {
                    this.Results.AddUnique(directory);
                    return;
                }

                var proxiedBase = this.ProxyPath.Combine(directory);
                this.Results.AddUnique(proxiedBase);
                return;
            }
            else
            {
                var path = directory.AbsolutePath;
                if ((null != this.ProxyPath) && !this.ProxyPath.Empty)
                {
                    var newLocation = this.ProxyPath.Combine(directory);
                    path = newLocation.AbsolutePath;
                }

                var fullPath = System.IO.Path.Combine(path, this.Pattern);
                if (this.TypeHint == ETypeHint.File)
                {
                    if (this.Pattern.Contains("*"))
                    {
                        var pattern = this.Pattern;
                        var searchType = System.IO.SearchOption.TopDirectoryOnly;

                        // is it a recursive search?
                        if (this.Pattern.Equals("**"))
                        {
                            pattern = "*";
                            searchType = System.IO.SearchOption.AllDirectories;
                        }
                        var dirInfo = new System.IO.DirectoryInfo(path);
                        var files = dirInfo.GetFiles(pattern, searchType);
                        foreach (var file in files)
                        {
                            this.Results.AddUnique(FileLocation.Get(file.FullName));
                        }
                    }
                    else
                    {
                        this.Results.AddUnique(FileLocation.Get(fullPath));
                    }
                }
                else
                {
                    this.Results.AddUnique(DirectoryLocation.Get(fullPath));
                }
            }
        }

        private void Resolve()
        {
#if true
            if (this.Base is ScaffoldLocation)
            {
                var baseScaffold = this.Base as ScaffoldLocation;
                baseScaffold.Resolve();
                foreach (var result in baseScaffold.Results)
                {
                    if (result is DirectoryLocation)
                    {
                        this.ResolveDirectory(result);
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
            }
            else if (this.Base is DirectoryLocation)
            {
                this.ResolveDirectory(this.Base);
            }
#else
            var baseLocation = this.Base.Resolve();
            var path = baseLocation.AbsolutePath;
            if (baseLocation is DirectoryLocation)
            {
                if (null == this.Pattern)
                {
                    if ((null == this.ProxyPath) || this.ProxyPath.Empty)
                    {
                        return baseLocation;
                    }

                    var newLocation = this.ProxyPath.Combine(baseLocation);
                    return newLocation;
                }

                if ((null != this.ProxyPath) && !this.ProxyPath.Empty)
                {
                    var newLocation = this.ProxyPath.Combine(baseLocation);
                    path = newLocation.Resolve().AbsolutePath;
                }

                path = System.IO.Path.Combine(path, this.Pattern);
                if (this.Pattern.Contains(".")) // TODO: perhaps an indication of a file
                {
                    return FileLocation.Get(path);
                }
                else
                {
                    return DirectoryLocation.Get(path);
                }
            }
            else if (baseLocation is FileLocation)
            {
                if (null != this.Pattern)
                {
                    throw new Exception("Cannot subdivide a file further");
                }

                return baseLocation;
            }
            else
            {
                throw new Exception("Not supported");
            }
#endif
        }

        public override Location SubDirectory(string subDirName)
        {
            return new ScaffoldLocation(this, subDirName, ETypeHint.Directory);
        }

        private Array<Location> Results
        {
            get;
            set;
        }

        public override string AbsolutePath
        {
            get
            {
                throw new Exception("Getting scaffold absolute path");
                //return base.AbsolutePath;
            }
            protected set
            {
                throw new Exception("Setting scaffold absolute path");
                //base.AbsolutePath = value;
            }
        }

        public override Array<Location> GetLocations()
        {
            this.Resolve();
            return this.Results;
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

    public class LocationComparer : System.Collections.Generic.IEqualityComparer<Location>
    {
        #region IEqualityComparer<Location> Members

        bool System.Collections.Generic.IEqualityComparer<Location>.Equals(Location x, Location y)
        {
            var xLocations = x.GetLocations();
            var yLocations = y.GetLocations();
            if (xLocations.Count != yLocations.Count)
            {
                return false;
            }
            for (int i = 0; i < xLocations.Count; ++i)
            {
                bool equals = (xLocations[i].AbsolutePath.Equals(yLocations[i].AbsolutePath));
                if (!equals)
                {
                    return false;
                }
            }
            return true;
        }

        int System.Collections.Generic.IEqualityComparer<Location>.GetHashCode(Location obj)
        {
            var locations = obj.GetLocations();
            var combinedPaths = new System.Text.StringBuilder();
            foreach (var location in locations)
            {
                combinedPaths.Append(location.AbsolutePath);
            }
            return combinedPaths.ToString().GetHashCode();
        }

        #endregion
    }
}

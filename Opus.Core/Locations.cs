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
        public abstract Location SubDirectory(string subDirName, EExists exists);

        public virtual string AbsolutePath
        {
            get;
            protected set;
        }

        public abstract LocationArray GetLocations();
        public abstract bool IsValid
        {
            get;
        }

        // TODO: this might have to migrate to the base class, if more subclasses need to be cloneable
        public EExists Exists
        {
            get;
            protected set;
        }

        /// <summary>
        /// This is a special case of GetLocations. It requires a Location resolves to a single path, which is returned.
        /// If the path contains a space, it will be returned quoted.
        /// </summary>
        /// <returns>Single path of the resolved Location</returns>
        public string GetSinglePath()
        {
            var path = this.GetSingleRawPath();
            if (path.Contains(" "))
            {
                path = System.String.Format("\"{0}\"", path);
            }
            return path;
        }

        /// <summary>
        /// This is a special case of GetLocations. It requires a Location resolves to a single path, which is returned.
        /// No checking whether the path contains spaces is performed.
        /// </summary>
        /// <returns>Single path of the resolved Location</returns>
        public string GetSingleRawPath()
        {
            var resolvedLocations = this.GetLocations();
            if (resolvedLocations.Count != 1)
            {
                throw new Exception("Location '{0}' did not resolve just one path, but {1}", this.ToString(), resolvedLocations.Count);
            }
            var path = resolvedLocations[0].AbsolutePath;
            return path;
        }
    }

    public sealed class LocationArray : Array<Location>, System.ICloneable
    {
        public LocationArray(params Location[] input)
        {
            this.AddRange(input);
        }

        public LocationArray(Array<Location> input)
        {
            this.AddRange(input);
        }

        public override string ToString(string separator)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var item in this.list)
            {
                var locationPath = item.ToString(); // this must be immutable
                builder.AppendFormat("{0}{1}", locationPath, separator);
            }
            // remove the trailing separator
            var output = builder.ToString().TrimEnd(separator.ToCharArray());
            return output;
        }

        public string Stringify(string separator)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var item in this.list)
            {
                var locationPath = item.GetSinglePath(); // this can be mutable
                builder.AppendFormat("{0}{1}", locationPath, separator);
            }
            // remove the trailing separator
            var output = builder.ToString().TrimEnd(separator.ToCharArray());
            return output;
        }

        #region ICloneable Members

        object System.ICloneable.Clone()
        {
            var clone = new LocationArray();
            clone.list.AddRange(this.list);
            return clone;
        }

        #endregion
    }

    /// <summary>
    /// DirectoryLocation represents a single directory on disk.
    /// It may or may not exist, but the default behaviour is to assume that it exists and this is tested.
    /// DirectoryLocations are cached internally, so only a single instance of a directory path exists.
    /// </summary>
    public sealed class DirectoryLocation : Location, System.ICloneable
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
            this.Exists = exists;
        }

        public static DirectoryLocation Get(string absolutePath, Location.EExists exists)
        {
            var hash = absolutePath.GetHashCode();
            if (cache.ContainsKey(hash))
            {
                return cache[hash];
            }

            // because the cache might be written into by multiple threads
            lock (cache)
            {
                var instance = new DirectoryLocation(absolutePath, exists);
                cache[hash] = instance;
                return instance;
            }
        }

        public static DirectoryLocation Get(string absolutePath)
        {
            return Get(absolutePath, EExists.Exists);
        }

        public override Location SubDirectory(string subDirName)
        {
            return this.SubDirectory(subDirName, this.Exists);
        }

        public override Location SubDirectory(string subDirName, EExists exists)
        {
            var subDirectoryPath = System.IO.Path.Combine(this.AbsolutePath, subDirName);
            return Get(subDirectoryPath, exists) as Location;
        }

        public override string ToString()
        {
            return System.String.Format("Directory '{0}'", this.AbsolutePath);
        }

        public override LocationArray GetLocations()
        {
            return new LocationArray(this);
        }

        public override bool IsValid
        {
            get
            {
                return true;
            }
        }

        #region ICloneable Members

        object System.ICloneable.Clone()
        {
            // Note, this is not using the hash location, as this really needs to be a separate instance
            var clone = new DirectoryLocation(this.AbsolutePath, this.Exists);
            return clone;
        }

        #endregion
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
            this.Exists = exists;
        }

        public static FileLocation Get(string absolutePath, Location.EExists exists)
        {
            var hash = absolutePath.GetHashCode();
            if (cache.ContainsKey(hash))
            {
                return cache[hash];
            }

            // because the cache might be written into by multiple threads
            lock (cache)
            {
                var instance = new FileLocation(absolutePath, exists);
                cache[hash] = instance;
                return instance;
            }
        }

        public static FileLocation Get(string absolutePath)
        {
            return Get(absolutePath, EExists.Exists);
        }

        public static FileLocation Get(Location baseLocation, string nonWildcardedFilename)
        {
            var locations = baseLocation.GetLocations();
            if (locations.Count != 1)
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

        public override Location SubDirectory(string subDirName, EExists exists)
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return System.String.Format("File '{0}'", this.AbsolutePath);
        }

        public override LocationArray GetLocations()
        {
            return new LocationArray(this);
        }

        public override bool IsValid
        {
            get
            {
                return true;
            }
        }
    }

    /// <summary>
    /// ScaffoldLocation is an abstract representation of many real Locations on disk.
    /// ScaffoldLocations are constructed from a base Location, with a search pattern added to it. This pattern may be wildcarded.
    /// Calling GetLocations() on a ScaffoldLocation will resolve the pattern to a list of real Locations, be they file or directory.
    /// </summary>
    public sealed class ScaffoldLocation : Location
    {
        public enum ETypeHint
        {
            Directory,
            File
        }

        public ScaffoldLocation(ETypeHint typeHint)
        {
            this.TypeHint = typeHint;
            this.Exists = EExists.WillExist; // make no assumptions that a stub will exist
            this.Results = new LocationArray();
            this.Resolved = false;
        }

        public ScaffoldLocation(Location baseLocation, string pattern, ETypeHint typeHint)
            : this(typeHint)
        {
            this.Base = baseLocation;
            this.Pattern = pattern;
        }

        public ScaffoldLocation(Location baseLocation, string pattern, ETypeHint typeHint, EExists exists)
            : this(baseLocation, pattern, typeHint)
        {
            this.Exists = exists;
        }

        public ScaffoldLocation(Location baseLocation, ProxyModulePath proxyPath, ETypeHint typeHint)
            : this(typeHint)
        {
            this.Base = baseLocation;
            this.ProxyPath = proxyPath;
        }

        public ScaffoldLocation(Location baseLocation, ProxyModulePath proxyPath, ETypeHint typeHint, EExists exists)
            : this(baseLocation, proxyPath, typeHint)
        {
            this.Exists = exists;
        }

        private bool Resolved
        {
            get;
            set;
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

        public void SetReference(Location reference)
        {
            if (this.IsValid)
            {
                throw new Exception("Cannot set a reference on a Location that is not a stub");
            }

            this.Base = reference;
            this.Exists = reference.Exists;
        }

        public void SpecifyStub(Location baseLocation, string pattern, Location.EExists exists)
        {
            if (!baseLocation.IsValid)
            {
                throw new Exception("Cannot specify a stub scaffold with an invalid base location");
            }

            this.Base    = baseLocation;
            this.Pattern = pattern;
            this.Exists  = exists;
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
                            this.Results.AddUnique(FileLocation.Get(file.FullName, this.Exists));
                        }
                    }
                    else
                    {
                        this.Results.AddUnique(FileLocation.Get(fullPath, this.Exists));
                    }
                }
                else
                {
                    this.Results.AddUnique(DirectoryLocation.Get(fullPath, this.Exists));
                }
            }
        }

        private void Resolve()
        {
            if (this.Resolved)
            {
                return;
            }

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
                    else if (result is FileLocation)
                    {
                        this.Results.Add(result);
                    }
                    else if (!result.IsValid)
                    {
                        throw new Exception("Resolved scaffold location is undefined");
                    }
                    else
                    {
                        throw new Exception("Resolved scaffold location is of an unknown type, {0}", result.GetType().ToString());
                    }
                }
            }
            else if (this.Base is DirectoryLocation)
            {
                this.ResolveDirectory(this.Base);
            }
            else if (this.Base is FileLocation)
            {
                this.Results.Add(this.Base);
            }
            else if (!this.IsValid) // same as asking if this.Base is null
            {
                throw new Exception("Location has not been set");
            }
            else
            {
                throw new Exception("Base location is of an unknown type, {0}", this.Base.GetType().ToString());
            }

            if (0 == this.Results.Count)
            {
                throw new Exception("Location has been resolved, but has no results. Base was '{0}'. Pattern was '{1}'", this.Base.GetSinglePath(), this.Pattern);
            }

            this.Resolved = true;
        }

        public override Location SubDirectory(string subDirName)
        {
            return new ScaffoldLocation(this, subDirName, ETypeHint.Directory, this.Exists);
        }

        public override Location SubDirectory(string subDirName, EExists exists)
        {
            return new ScaffoldLocation(this, subDirName, ETypeHint.Directory, exists);
        }

        private LocationArray Results
        {
            get;
            set;
        }

        public override string AbsolutePath
        {
            get
            {
                throw new Exception("Scaffold locations do not have an absolute path. Use GetLocations()");
            }
            protected set
            {
                throw new Exception("Scaffold locations cannot have their absolute path set. Use a FileLocation or DirectoryLocation");
            }
        }

        public override LocationArray GetLocations()
        {
            this.Resolve();
            return this.Results;
        }

        public override bool IsValid
        {
            get
            {
                bool valid = (null != this.Base);
                return valid;
            }
        }

        public override string ToString()
        {
            if (this.Resolved)
            {
                return this.Results.ToString(" ");
            }
            else
            {
                return "Scaffold location is unresolved";
            }
        }
    }

    /// <summary>
    /// Abstraction of the key to the LocationMap
    /// </summary>
    public sealed class LocationKey
    {
        public LocationKey(string identifier, ScaffoldLocation.ETypeHint type)
        {
            this.Identifier = identifier;
            this.Type = type;
        }

        private string Identifier
        {
            get;
            set;
        }

        public ScaffoldLocation.ETypeHint Type
        {
            get;
            private set;
        }

        public bool IsFileKey
        {
            get
            {
                bool isFileKey = (this.Type == ScaffoldLocation.ETypeHint.File);
                return isFileKey;
            }
        }

        public bool IsDirectoryKey
        {
            get
            {
                bool isDirectoryKey = (this.Type == ScaffoldLocation.ETypeHint.Directory);
                return isDirectoryKey;
            }
        }

        public override string ToString()
        {
            return this.Identifier;
        }
    }

    public sealed class LocationMap
    {
        private System.Collections.Generic.Dictionary<LocationKey, Location> map = new System.Collections.Generic.Dictionary<LocationKey, Location>();

        public Location this[LocationKey key]
        {
            get
            {
                try
                {
                    return this.map[key];
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    throw new Exception("Cannot find {0} location key '{1}'", key.IsFileKey ? "file" : "directory", key.ToString());
                }
            }

            set
            {
                // TODO: validate the key type is what is to be stored
                if (value is ScaffoldLocation)
                {
                    if ((value as ScaffoldLocation).TypeHint != key.Type)
                    {
                        throw new Exception("Location and LocationKey differ on type hints");
                    }
                }
                else if (key.Type == ScaffoldLocation.ETypeHint.File)
                {
                    if (!(value is FileLocation))
                    {
                        throw new Exception("LocationKey wants a file, but doesn't have one");
                    }
                }
                else if (key.Type == ScaffoldLocation.ETypeHint.Directory)
                {
                    if (!(value is DirectoryLocation))
                    {
                        throw new Exception("LocationKey wants a directory, but doesn't have one");
                    }
                }
                this.map[key] = value;
            }
        }

        public bool Contains(LocationKey key)
        {
            return this.map.ContainsKey(key);
        }

        public Array<LocationKey> Keys(ScaffoldLocation.ETypeHint type, Location.EExists exists)
        {
            var keys = new Array<LocationKey>();
            foreach (var key in this.map.Keys)
            {
                if (key.Type == type)
                {
                    var location = this.map[key];
                    if (location.Exists == exists)
                    {
                        keys.Add(key);
                    }
                }
            }
            return keys;
        }

        public LocationArray FilterByKey(Array<LocationKey> filterKeys)
        {
            var filteredLocations = new LocationArray();
            foreach (var key in filterKeys)
            {
                if (!this.map.ContainsKey(key))
                {
                    continue;
                }
                var loc = this.map[key];
                if (!loc.IsValid)
                {
                    continue;
                }
                filteredLocations.Add(loc);
            }
            return filteredLocations;
        }

        public LocationArray FilterByType(ScaffoldLocation.ETypeHint type, Location.EExists exists)
        {
            var filteredLocations = new LocationArray();
            foreach (var key in this.map.Keys)
            {
                if (key.Type != type)
                {
                    continue;
                }
                var location = this.map[key];
                if (!location.IsValid)
                {
                    continue;
                }
                if (location.Exists != exists)
                {
                    continue;
                }

                filteredLocations.Add(location);
            }
            return filteredLocations;
        }

        public override string ToString()
        {
            var repr = new System.Text.StringBuilder();
            foreach (var key in this.map.Keys)
            {
                repr.AppendFormat("{0}:{1} ", key.ToString(), this.map[key].ToString());
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

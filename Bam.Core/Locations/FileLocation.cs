// <copyright file="FileLocation.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    /// <summary>
    /// FileLocation represents a single file on disk.
    /// It may or may not exist, but the default behaviour is to assume that it exists and this is tested.
    /// FileLocations are cached internally, so only a single instance of a file path exists.
    /// </summary>
    public sealed class FileLocation :
        Location
    {
        private static System.Collections.Generic.Dictionary<int, FileLocation> cache = new System.Collections.Generic.Dictionary<int, FileLocation>();

        private
        FileLocation(
            string absolutePath,
            Location.EExists exists)
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

        public static FileLocation
        Get(
            string absolutePath,
            Location.EExists exists)
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

        public static FileLocation
        Get(
            string absolutePath)
        {
            return Get(absolutePath, EExists.Exists);
        }

        public static FileLocation
        Get(
            Location baseLocation,
            string nonWildcardedFilename,
            EExists exists)
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
            var instance = new FileLocation(path, exists);
            cache[hash] = instance;
            return instance;
        }

        public static FileLocation
        Get(
            Location baseLocation,
            string nonWildcardedFilename)
        {
            return Get(baseLocation, nonWildcardedFilename, EExists.Exists);
        }

        public override Location
        SubDirectory(
            string subDirName)
        {
            throw new System.NotImplementedException();
        }

        public override Location
        SubDirectory(
            string subDirName,
            EExists exists)
        {
            throw new System.NotImplementedException();
        }

        public override string
        ToString()
        {
            return System.String.Format("File '{0}':{1}", this.AbsolutePath, this.Exists.ToString());
        }

        public override LocationArray
        GetLocations()
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
}

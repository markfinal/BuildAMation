// <copyright file="SymlinkLocation.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    /// <summary>
    /// SymlinkLocation represents a single symbol link on disk.
    /// It may or may not exist, but the default behaviour is to assume that it exists and this is tested.
    /// SymlinkLocation are cached internally, so only a single instance of a symlink path exists.
    /// </summary>
    public sealed class SymlinkLocation :
        Location
    {
        private static System.Collections.Generic.Dictionary<int, SymlinkLocation> cache = new System.Collections.Generic.Dictionary<int, SymlinkLocation>();

        private
        SymlinkLocation(
            string absolutePath,
            Location.EExists exists)
        {
            if (exists == EExists.Exists)
            {
                if (!System.IO.File.Exists(absolutePath))
                {
                    throw new Exception("Symlink '{0}' does not exist", absolutePath);
                }
            }
            this.AbsolutePath = absolutePath;
            this.Exists = exists;
        }

        public static SymlinkLocation
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
                var instance = new SymlinkLocation(absolutePath, exists);
                cache[hash] = instance;
                return instance;
            }
        }

        public static SymlinkLocation
        Get(
            string absolutePath)
        {
            return Get(absolutePath, EExists.Exists);
        }

        public static SymlinkLocation
        Get(
            Location baseLocation,
            string nonWildcardedFilename)
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
            var instance = new SymlinkLocation(path, EExists.Exists);
            cache[hash] = instance;
            return instance;
        }

        public override Location
        SubDirectory(
            string subDirName)
        {
            throw new System.NotImplementedException ();
        }

        public override Location
        SubDirectory(
            string subDirName,
            EExists exists)
        {
            throw new System.NotImplementedException ();
        }

        public override string
        ToString()
        {
            return System.String.Format("Symlink '{0}':{1}", this.AbsolutePath, this.Exists.ToString());
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

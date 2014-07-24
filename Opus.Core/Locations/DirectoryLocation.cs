// <copyright file="DirectorLocation.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    /// <summary>
    /// DirectoryLocation represents a single directory on disk.
    /// It may or may not exist, but the default behaviour is to assume that it exists and this is tested.
    /// DirectoryLocations are cached internally, so only a single instance of a directory path exists.
    /// </summary>
    public sealed class DirectoryLocation :
        Location,
        System.ICloneable
    {
        private static System.Collections.Generic.Dictionary<int, DirectoryLocation> cache = new System.Collections.Generic.Dictionary<int, DirectoryLocation>();

        private
        DirectoryLocation(
            string absolutePath,
            Location.EExists exists)
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

        public static DirectoryLocation
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
                var instance = new DirectoryLocation(absolutePath, exists);
                cache[hash] = instance;
                return instance;
            }
        }

        public static DirectoryLocation
        Get(
            string absolutePath)
        {
            return Get(absolutePath, EExists.Exists);
        }

        public override Location
        SubDirectory(
            string subDirName)
        {
            return this.SubDirectory(subDirName, this.Exists);
        }

        public override Location
        SubDirectory(
            string subDirName,
            EExists exists)
        {
            var subDirectoryPath = System.IO.Path.Combine(this.AbsolutePath, subDirName);
            return Get(subDirectoryPath, exists) as Location;
        }

        public override string
        ToString()
        {
            return System.String.Format("Directory '{0}'", this.AbsolutePath);
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

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            // Note, this is not using the hash location, as this really needs to be a separate instance
            var clone = new DirectoryLocation(this.AbsolutePath, this.Exists);
            return clone;
        }

        #endregion
    }
}

#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace Bam.Core
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
            return System.String.Format("Directory '{0}':{1}", this.AbsolutePath, this.Exists.ToString());
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

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
#endregion
namespace Bam.Core
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

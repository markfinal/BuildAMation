#region License
// Copyright 2010-2015 Mark Final
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
                if (value is ScaffoldLocation)
                {
                    if ((value as ScaffoldLocation).TypeHint != key.Type)
                    {
                        throw new Exception("Location and LocationKey differ on type hints. Location is {0}. Key '{1}' is type {2}",
                                            (value as ScaffoldLocation).TypeHint.ToString(), key.ToString(), key.Type.ToString());
                    }
                }
                else if (key.Type == ScaffoldLocation.ETypeHint.File)
                {
                    if (!(value is FileLocation))
                    {
                        throw new Exception("LocationKey '{0}' requires a file but has an {1} instead", key.ToString(), value.GetType().ToString());
                    }
                }
                else if (key.Type == ScaffoldLocation.ETypeHint.Symlink)
                {
                    if (!(value is SymlinkLocation))
                    {
                        throw new Exception("LocationKey '{0}' requires a symbolic link but has an {1} instead", key.ToString(), value.GetType().ToString());
                    }
                }
                else if (key.Type == ScaffoldLocation.ETypeHint.Directory)
                {
                    if (!(value is DirectoryLocation))
                    {
                        throw new Exception("LocationKey '{0}' requires a directory but has an {1} instead", key.ToString(), value.GetType().ToString());
                    }
                }
                this.map[key] = value;
            }
        }

        public bool
        Contains(
            LocationKey key)
        {
            return this.map.ContainsKey(key);
        }

        public Array<LocationKey>
        Keys(
            ScaffoldLocation.ETypeHint type,
            Location.EExists exists)
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

        public LocationArray
        FilterByKey(
            Array<LocationKey> filterKeys)
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

        public LocationArray
        FilterByType(
            ScaffoldLocation.ETypeHint type,
            Location.EExists exists)
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

        public override string
        ToString()
        {
            var repr = new System.Text.StringBuilder();
            foreach (var key in this.map.Keys)
            {
                repr.AppendFormat("{0}:{1} ", key.ToString(), this.map[key].ToString());
            }
            return repr.ToString();
        }
    }
}

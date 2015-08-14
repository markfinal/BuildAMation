#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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

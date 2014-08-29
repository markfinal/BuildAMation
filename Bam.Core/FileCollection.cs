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
    public sealed class FileCollection :
        System.ICloneable,
        System.Collections.IEnumerable
    {
        private LocationArray fileLocations = new LocationArray();

        public
        FileCollection()
        {}

        public
        FileCollection(
            params FileCollection[] collections)
        {
            foreach (var collection in collections)
            {
                foreach (string path in collection)
                {
                    this.fileLocations.Add(FileLocation.Get(path, Location.EExists.Exists));
                }
            }
        }

        public object
        Clone()
        {
            var clone = new FileCollection();
            clone.fileLocations.AddRange(this.fileLocations);
            return clone;
        }

        public void
        Add(
            string path)
        {
            this.fileLocations.AddUnique(FileLocation.Get(path, Location.EExists.WillExist));
        }

        public void
        AddRange(
            StringArray paths)
        {
            foreach (var path in paths)
            {
                this.Add(path);
            }
        }

        public Location this[int index]
        {
            get
            {
                return this.fileLocations[index];
            }
        }

        public int Count
        {
            get
            {
                return this.fileLocations.Count;
            }
        }

        public System.Collections.IEnumerator
        GetEnumerator()
        {
            return this.fileLocations.GetEnumerator();
        }

        public void
        Include(
            Location baseLocation,
            string pattern)
        {
            var scaffold = new ScaffoldLocation(baseLocation, pattern, ScaffoldLocation.ETypeHint.File);
            // TODO: this should be deferred until much later - we should only store the scaffolds
            var files = scaffold.GetLocations();
            foreach (var file in files)
            {
                this.fileLocations.Add(file);
            }
        }

        public StringArray
        ToStringArray()
        {
            var array = new StringArray();
            foreach (var location in this.fileLocations)
            {
                var locations = location.GetLocations();
                foreach (var loc in locations)
                {
                    array.Add(loc.AbsolutePath);
                }
            }
            return array;
        }
    }
}

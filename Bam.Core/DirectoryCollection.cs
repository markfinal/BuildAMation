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
    // TODO: can this be a specific case of a LocationArray? i.e. store locations, but have
    // specialized Add functions
    public sealed class DirectoryCollection :
        System.ICloneable,
        System.Collections.IEnumerable,
        ISetOperations<DirectoryCollection>
    {
        private LocationArray directoryLocations = new LocationArray();

        public object
        Clone()
        {
            var clone = new DirectoryCollection();
            clone.directoryLocations.AddRange(this.directoryLocations);
            return clone;
        }

        public void
        Add(
            string path)
        {
            this.directoryLocations.AddUnique(DirectoryLocation.Get(path, Location.EExists.WillExist));
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

        public void
        Include(
            Location baseLocation)
        {
            var locations = baseLocation.GetLocations();
            this.directoryLocations.AddRangeUnique(locations);
        }

        public void
        Include(
            Location baseLocation,
            string pattern)
        {
            var locations = baseLocation.SubDirectory(pattern).GetLocations();
            this.directoryLocations.AddRangeUnique(locations);
        }

        // TODO: should this return a Location?
        public string this[int index]
        {
            get
            {
                return this.directoryLocations[index].AbsolutePath;
            }
        }

        public int Count
        {
            get
            {
                return this.directoryLocations.Count;
            }
        }

        public System.Collections.IEnumerator
        GetEnumerator()
        {
            return new DirectoryCollectionEnumerator(this);
        }

        public StringArray
        ToStringArray()
        {
            var array = new StringArray();
            foreach (var location in this.directoryLocations)
            {
                array.AddUnique(location.AbsolutePath);
            }
            return array;
        }

        public override bool
        Equals(
            object obj)
        {
            var otherCollection = obj as DirectoryCollection;
            return (this.directoryLocations.Equals(otherCollection.directoryLocations));
        }

        public override int
        GetHashCode()
        {
            return base.GetHashCode();
        }

        DirectoryCollection
        ISetOperations<DirectoryCollection>.Complement(
            DirectoryCollection other)
        {
            var complementPaths = this.directoryLocations.Complement(other.directoryLocations);
            if (0 == complementPaths.Count)
            {
                throw new Exception("DirectoryCollection complement is empty");
            }

            var complementDirectoryCollection = new DirectoryCollection();
            complementDirectoryCollection.directoryLocations.AddRange(complementPaths);
            return complementDirectoryCollection;
        }

        DirectoryCollection
        ISetOperations<DirectoryCollection>.Intersect(
            DirectoryCollection other)
        {
            var intersectPaths = this.directoryLocations.Intersect(other.directoryLocations);
            var intersectDirectoryCollection = new DirectoryCollection();
            intersectDirectoryCollection.directoryLocations.AddRange(intersectPaths);
            return intersectDirectoryCollection;
        }
    }
}

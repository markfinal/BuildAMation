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

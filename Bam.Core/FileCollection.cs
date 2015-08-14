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

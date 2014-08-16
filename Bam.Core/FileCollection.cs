// <copyright file="FileCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
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

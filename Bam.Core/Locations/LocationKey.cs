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
namespace V2
{
    using System.Linq;

    /// <summary>
    /// Unique keys representing a set of files, through a factory method
    /// </summary>
    public sealed class FileKey
    {
        private FileKey(string key)
        {
            this.Id = key;
        }

        private static System.Collections.Generic.List<FileKey> GeneratedKeys = new System.Collections.Generic.List<FileKey>();

        public static FileKey Generate(string key)
        {
            var matches = GeneratedKeys.Where(item => (item.Id == key));
            if (1 == matches.Count())
            {
                return matches.ElementAt(0);
            }
            var newKey = new FileKey(key);
            GeneratedKeys.Add(newKey);
            return newKey;
        }

        public string Id
        {
            get;
            private set;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Id.Equals((obj as FileKey).Id);
        }

        public override string ToString()
        {
            return this.Id;
        }
    }
}

    /// <summary>
    /// Abstraction of the key to the LocationMap
    /// </summary>
    public sealed class LocationKey
    {
        public
        LocationKey(
            string identifier,
            ScaffoldLocation.ETypeHint type)
        {
            this.Identifier = identifier;
            this.Type = type;
        }

        private string Identifier
        {
            get;
            set;
        }

        public ScaffoldLocation.ETypeHint Type
        {
            get;
            private set;
        }

        public bool IsFileKey
        {
            get
            {
                bool isFileKey = (this.Type == ScaffoldLocation.ETypeHint.File);
                return isFileKey;
            }
        }

        public bool IsDirectoryKey
        {
            get
            {
                bool isDirectoryKey = (this.Type == ScaffoldLocation.ETypeHint.Directory);
                return isDirectoryKey;
            }
        }

        public bool IsSymlinkKey
        {
            get
            {
                bool isSymlinkKey = (this.Type == ScaffoldLocation.ETypeHint.Symlink);
                return isSymlinkKey;
            }
        }

        public override string
        ToString()
        {
            return this.Identifier;
        }
    }
}

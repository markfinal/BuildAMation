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

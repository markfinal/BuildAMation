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
namespace Publisher
{
    public class PublishDirectory
    {
        public
        PublishDirectory(
            Bam.Core.Location root,
            string directory) : this(root, directory, null)
        {}

        public
        PublishDirectory(
            Bam.Core.Location root,
            string directory,
            string renamedLeaf)
        {
            this.Root = root;
            this.Directory = directory;
            this.DirectoryLocation = new Bam.Core.ScaffoldLocation(root, directory, Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            this.RenamedLeaf = renamedLeaf;
        }

        public Bam.Core.Location Root
        {
            get;
            private set;
        }

        public string Directory
        {
            get;
            private set;
        }

        public Bam.Core.Location DirectoryLocation
        {
            get;
            private set;
        }

        public string RenamedLeaf
        {
            get;
            private set;
        }
    }
}

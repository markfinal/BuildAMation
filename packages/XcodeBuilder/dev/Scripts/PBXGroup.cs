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
namespace XcodeBuilder
{
    public sealed class PBXGroup :
        XcodeNodeData,
        IWriteableNode
    {
        public
        PBXGroup(
            string name) : base(name)
        {
            this.Children = new Bam.Core.Array<XcodeNodeData>();
        }

        public string Path
        {
            get;
            set;
        }

        public string SourceTree
        {
            get;
            set;
        }

        public Bam.Core.Array<XcodeNodeData> Children
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (0 == this.Children.Count)
            {
                return;
            }
            if (string.IsNullOrEmpty(this.SourceTree))
            {
                throw new Bam.Core.Exception("Source tree not set");
            }

            if (string.IsNullOrEmpty(this.Name))
            {
                // this is the main group
                writer.WriteLine("\t\t{0} = {{", this.UUID);
            }
            else
            {
                writer.WriteLine("\t\t{0} /* {1} */ = {{", this.UUID, this.Name);
            }
            writer.WriteLine("\t\t\tisa = PBXGroup;");
            writer.WriteLine("\t\t\tchildren = (");
            foreach (var child in this.Children)
            {
                if (child is PBXFileReference)
                {
                    writer.WriteLine("\t\t\t\t{0} /* {1} */,", child.UUID, (child as PBXFileReference).ShortPath);
                }
                else
                {
                    writer.WriteLine("\t\t\t\t{0} /* {1} */,", child.UUID, child.Name);
                }
            }
            writer.WriteLine("\t\t\t);");
            if (string.IsNullOrEmpty(this.Path))
            {
                if (!string.IsNullOrEmpty(this.Name))
                {
                    writer.WriteLine("\t\t\tname = {0};", this.Name);
                }
            }
            else
            {
                writer.WriteLine("\t\t\tpath = {0};", this.Path);
            }
            writer.WriteLine("\t\t\tsourceTree = \"{0}\";", this.SourceTree);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}

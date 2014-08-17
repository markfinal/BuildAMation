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
namespace XcodeBuilder
{
    public sealed class PBXGroupSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXGroupSection()
        {
            this.Groups = new Bam.Core.Array<PBXGroup>();
        }

        public PBXGroup
        Get(
            string name)
        {
            lock (this.Groups)
            {
                foreach (var group in this.Groups)
                {
                    if (group.Name == name)
                    {
                        return group;
                    }
                }

                var newGroup = new PBXGroup(name);
                this.Groups.Add(newGroup);
                return newGroup;
            }
        }

        private Bam.Core.Array<PBXGroup> Groups
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.Groups.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXGroup>(this.Groups);
            orderedList.Sort(
                delegate(PBXGroup p1, PBXGroup p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXGroup section */");
            foreach (var group in orderedList)
            {
                (group as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXGroup section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Groups.GetEnumerator();
        }

#endregion
    }
}

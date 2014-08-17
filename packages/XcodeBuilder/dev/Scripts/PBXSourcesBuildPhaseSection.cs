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
    public sealed class PBXSourcesBuildPhaseSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXSourcesBuildPhaseSection()
        {
            this.SourcesBuildPhase = new System.Collections.Generic.List<PBXSourcesBuildPhase>();
        }

        public PBXSourcesBuildPhase
        Get(
            string name,
            string moduleName)
        {
            lock (this.SourcesBuildPhase)
            {
                foreach (var buildPhase in this.SourcesBuildPhase)
                {
                    if ((buildPhase.Name == name) && (buildPhase.ModuleName == moduleName))
                    {
                        return buildPhase;
                    }
                }

                var newBuildPhase = new PBXSourcesBuildPhase(name, moduleName);
                this.SourcesBuildPhase.Add(newBuildPhase);
                return newBuildPhase;
            }
        }

        private System.Collections.Generic.List<PBXSourcesBuildPhase> SourcesBuildPhase
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.SourcesBuildPhase.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXSourcesBuildPhase>(this.SourcesBuildPhase);
            orderedList.Sort(
                delegate(PBXSourcesBuildPhase p1, PBXSourcesBuildPhase p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXSourcesBuildPhase section */");
            foreach (var buildPhase in orderedList)
            {
                (buildPhase as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXSourcesBuildPhase section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.SourcesBuildPhase.GetEnumerator();
        }

#endregion
    }
}

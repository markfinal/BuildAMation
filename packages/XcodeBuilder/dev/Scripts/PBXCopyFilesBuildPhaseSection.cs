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
namespace XcodeBuilder
{
    public sealed class PBXCopyFilesBuildPhaseSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXCopyFilesBuildPhaseSection()
        {
            this.CopyFilesBuildPhases = new System.Collections.Generic.List<PBXCopyFilesBuildPhase>();
        }

        public void
        Add(
            PBXCopyFilesBuildPhase buildPhase)
        {
            lock (this.CopyFilesBuildPhases)
            {
                this.CopyFilesBuildPhases.Add(buildPhase);
            }
        }

        public PBXCopyFilesBuildPhase
        Get(
            string name,
            string moduleName)
        {
            lock (this.CopyFilesBuildPhases)
            {
                foreach (var buildPhase in this.CopyFilesBuildPhases)
                {
                    if ((buildPhase.Name == name) && (buildPhase.ModuleName == moduleName))
                    {
                        return buildPhase;
                    }
                }

                var newBuildPhase = new PBXCopyFilesBuildPhase(name, moduleName);
                this.CopyFilesBuildPhases.Add(newBuildPhase);
                return newBuildPhase;
            }
        }

        private System.Collections.Generic.List<PBXCopyFilesBuildPhase> CopyFilesBuildPhases
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.CopyFilesBuildPhases.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXCopyFilesBuildPhase>(this.CopyFilesBuildPhases);
            orderedList.Sort(
                delegate(PBXCopyFilesBuildPhase p1, PBXCopyFilesBuildPhase p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXCopyFilesBuildPhase section */");
            foreach (var buildPhase in orderedList)
            {
                (buildPhase as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXCopyFilesBuildPhase section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.CopyFilesBuildPhases.GetEnumerator();
        }

#endregion
    }
}

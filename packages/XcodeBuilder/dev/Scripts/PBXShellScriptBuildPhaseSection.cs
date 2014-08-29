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
    public sealed class PBXShellScriptBuildPhaseSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXShellScriptBuildPhaseSection()
        {
            this.ShellScriptBuildPhases = new System.Collections.Generic.List<PBXShellScriptBuildPhase>();
        }

        public void
        Add(
            PBXShellScriptBuildPhase buildPhase)
        {
            lock (this.ShellScriptBuildPhases)
            {
                this.ShellScriptBuildPhases.Add(buildPhase);
            }
        }

        public PBXShellScriptBuildPhase
        Get(
            string name,
            string moduleName)
        {
            lock (this.ShellScriptBuildPhases)
            {
                foreach (var buildPhase in this.ShellScriptBuildPhases)
                {
                    if ((buildPhase.Name == name) && (buildPhase.ModuleName == moduleName))
                    {
                        return buildPhase;
                    }
                }

                var newBuildPhase = new PBXShellScriptBuildPhase(name, moduleName);
                this.ShellScriptBuildPhases.Add(newBuildPhase);
                return newBuildPhase;
            }
        }

        private System.Collections.Generic.List<PBXShellScriptBuildPhase> ShellScriptBuildPhases
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.ShellScriptBuildPhases.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXShellScriptBuildPhase>(this.ShellScriptBuildPhases);
            orderedList.Sort(
                delegate(PBXShellScriptBuildPhase p1, PBXShellScriptBuildPhase p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXShellScriptBuildPhase section */");
            foreach (var buildPhase in orderedList)
            {
                (buildPhase as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXShellScriptBuildPhase section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.ShellScriptBuildPhases.GetEnumerator();
        }

#endregion
    }
}

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
    public sealed class PBXTargetDependencySection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXTargetDependencySection()
        {
            this.TargetDependencies = new System.Collections.Generic.List<PBXTargetDependency>();
        }

        public PBXTargetDependency
        Get(
            string name,
            PBXNativeTarget nativeTarget)
        {
            lock (this.TargetDependencies)
            {
                foreach (var dependency in this.TargetDependencies)
                {
                    if ((dependency.Name == name) && (dependency.NativeTarget == nativeTarget))
                    {
                        return dependency;
                    }
                }

                var newDependency = new PBXTargetDependency(name, nativeTarget);
                this.TargetDependencies.Add(newDependency);
                return newDependency;
            }
        }

        private System.Collections.Generic.List<PBXTargetDependency> TargetDependencies
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.TargetDependencies.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXTargetDependency>(this.TargetDependencies);
            orderedList.Sort(
                delegate(PBXTargetDependency p1, PBXTargetDependency p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXTargetDependency section */");
            foreach (var dependency in orderedList)
            {
                (dependency as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXTargetDependency section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.TargetDependencies.GetEnumerator();
        }

#endregion
    }
}

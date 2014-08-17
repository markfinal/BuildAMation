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
    public sealed class PBXNativeTargetSection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXNativeTargetSection()
        {
            this.NativeTargets = new System.Collections.Generic.List<PBXNativeTarget>();
        }

        public PBXNativeTarget
        Get(
            string name,
            PBXNativeTarget.EType type,
            PBXProject project)
        {
            lock(this.NativeTargets)
            {
                foreach (var target in this.NativeTargets)
                {
                    // check whether the type has been changed, which is still valid if the original type
                    // is what is being sought
                    if ((target.Name == name) &&
                        ((target.Type == type) || (target.OriginalType == type)) &&
                        (target.Project == project))
                    {
                        return target;
                    }
                }

                var newTarget = new PBXNativeTarget(name, type, project);
                this.NativeTargets.Add(newTarget);
                return newTarget;
            }
        }

        public PBXNativeTarget this[int index]
        {
            get
            {
                return this.NativeTargets[index];
            }
        }

        private System.Collections.Generic.List<PBXNativeTarget> NativeTargets
        {
            get;
            set;
        }

        public int Count
        {
            get
            {
                return this.NativeTargets.Count;
            }
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.NativeTargets.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXNativeTarget>(this.NativeTargets);
            orderedList.Sort(
                delegate(PBXNativeTarget p1, PBXNativeTarget p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXNativeTarget section */");
            foreach (var nativeTarget in orderedList)
            {
                (nativeTarget as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXNativeTarget section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.NativeTargets.GetEnumerator();
        }

#endregion
    }
}

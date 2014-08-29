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
    public sealed class PBXTargetDependency :
        XcodeNodeData,
        IWriteableNode
    {
        public
        PBXTargetDependency(
            string name,
            PBXNativeTarget nativeTarget) : base(name)
        {
            this.NativeTarget = nativeTarget;
        }

        public PBXNativeTarget NativeTarget
        {
            get;
            private set;
        }

        public PBXContainerItemProxy TargetProxy
        {
            get;
            set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.NativeTarget == null)
            {
                throw new Bam.Core.Exception("Native target not set on this dependency");
            }
            if (this.TargetProxy == null)
            {
                throw new Bam.Core.Exception("Target proxy not set on this dependency");
            }

            writer.WriteLine("\t\t{0} /* PBXTargetDependency */ = {{", this.UUID);
            writer.WriteLine("\t\t\tisa = PBXTargetDependency;");
            writer.WriteLine("\t\t\ttarget = {0} /* {1} */;", this.NativeTarget.UUID, this.NativeTarget.Name);
            writer.WriteLine("\t\t\ttargetProxy = {0} /* {1} */;", this.TargetProxy.UUID, this.TargetProxy.GetType().Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}

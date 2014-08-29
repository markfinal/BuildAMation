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
    public sealed class PBXContainerItemProxy :
        XcodeNodeData,
        IWriteableNode
    {
        public
        PBXContainerItemProxy(
            string name,
            XcodeNodeData remote,
            XcodeNodeData portal) : base(name)
        {
            this.Remote = remote;
            this.Portal = portal;
        }

        public XcodeNodeData Remote
        {
            get;
            private set;
        }

        public XcodeNodeData Portal
        {
            get;
            private set;
        }

#region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.Remote == null)
            {
                throw new Bam.Core.Exception("Remote was not set on this container proxy");
            }
            if (this.Portal == null)
            {
                throw new Bam.Core.Exception("Portal was not set on this container proxy");
            }

            writer.WriteLine("\t\t{0} /* PBXContainerItemProxy */ = {{", this.UUID);
            writer.WriteLine("\t\t\tisa = PBXContainerItemProxy;");
            writer.WriteLine("\t\t\tcontainerPortal = {0} /* {1} object */;", this.Portal.UUID, this.Portal.GetType().Name);
            writer.WriteLine("\t\t\tproxyType = 1;");
            writer.WriteLine("\t\t\tremoteGlobalIDString = {0};", this.Remote.UUID);
            writer.WriteLine("\t\t\tremoteInfo = {0};", this.Remote.Name);
            writer.WriteLine("\t\t};");
        }

#endregion
    }
}

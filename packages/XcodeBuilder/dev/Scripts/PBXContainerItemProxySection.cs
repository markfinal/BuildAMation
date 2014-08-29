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
    public sealed class PBXContainerItemProxySection :
        IWriteableNode,
        System.Collections.IEnumerable
    {
        public
        PBXContainerItemProxySection()
        {
            this.ContainerItemProxies = new System.Collections.Generic.List<PBXContainerItemProxy>();
        }

        public PBXContainerItemProxy
        Get(
            string name,
            XcodeNodeData remote,
            XcodeNodeData portal)
        {
            lock (this.ContainerItemProxies)
            {
                foreach (var containerItem in this.ContainerItemProxies)
                {
                    if ((containerItem.Name == name) && (containerItem.Remote == remote) && (containerItem.Portal == portal))
                    {
                        return containerItem;
                    }
                }

                var newContainerItem = new PBXContainerItemProxy(name, remote, portal);
                this.ContainerItemProxies.Add(newContainerItem);
                return newContainerItem;
            }
        }

        private System.Collections.Generic.List<PBXContainerItemProxy> ContainerItemProxies
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            if (this.ContainerItemProxies.Count == 0)
            {
                return;
            }

            var orderedList = new System.Collections.Generic.List<PBXContainerItemProxy>(this.ContainerItemProxies);
            orderedList.Sort(
                delegate(PBXContainerItemProxy p1, PBXContainerItemProxy p2)
                {
                    return p1.UUID.CompareTo(p2.UUID);
                }
            );

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXContainerItemProxy section */");
            foreach (var item in orderedList)
            {
                (item as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXContainerItemProxy section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.ContainerItemProxies.GetEnumerator();
        }

#endregion
    }
}

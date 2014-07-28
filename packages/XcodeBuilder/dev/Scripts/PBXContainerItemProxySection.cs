// <copyright file="PBXContainerItemProxySection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
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
            XCodeNodeData remote,
            XCodeNodeData portal)
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

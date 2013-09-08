// <copyright file="PBXContainerItemProxySection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XCodeBuilder package</summary>
// <author>Mark Final</author>
namespace XCodeBuilder
{
    public sealed class PBXContainerItemProxySection : IWriteableNode, System.Collections.IEnumerable
    {
        public PBXContainerItemProxySection()
        {
            this.ContainerItemProxies = new System.Collections.Generic.List<PBXContainerItemProxy>();
        }

        public void Add(PBXContainerItemProxy item)
        {
            lock (this.ContainerItemProxies)
            {
                this.ContainerItemProxies.Add(item);
            }
        }

        private System.Collections.Generic.List<PBXContainerItemProxy> ContainerItemProxies
        {
            get;
            set;
        }

#region IWriteableNode implementation
        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            if (this.ContainerItemProxies.Count == 0)
            {
                return;
            }

            writer.WriteLine("");
            writer.WriteLine("/* Begin PBXContainerItemProxy section */");
            foreach (var item in this.ContainerItemProxies)
            {
                (item as IWriteableNode).Write(writer);
            }
            writer.WriteLine("/* End PBXContainerItemProxy section */");
        }
#endregion

#region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.ContainerItemProxies.GetEnumerator();
        }

#endregion
    }
}

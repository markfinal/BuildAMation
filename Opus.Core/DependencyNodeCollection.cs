// <copyright file="DependencyNodeCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class DependencyNodeCollection : System.Collections.Generic.ICollection<DependencyNode>, System.ICloneable
    {
        private System.Collections.Generic.List<DependencyNode> list = new System.Collections.Generic.List<DependencyNode>();
        private int completedNodeCount = 0;

        public DependencyNodeCollection()
            : this(-1)
        {
        }

        public DependencyNodeCollection(int rank)
        {
            this.Rank = rank;
            this.AllNodesCompletedEvent = new System.Threading.ManualResetEvent[1];
            this.AllNodesCompletedEvent[0] = new System.Threading.ManualResetEvent(false);
        }

        private void CompletedNode(DependencyNode node)
        {
            if (this.list.Contains(node))
            {
                if (0 == System.Threading.Interlocked.Decrement(ref this.completedNodeCount))
                {
                    this.AllNodesCompletedEvent[0].Set();
                }
            }

            node.CompletedEvent -= CompletedNode;
        }

        public void Add(DependencyNode item)
        {
            if (!this.list.Contains(item))
            {
                this.list.Add(item);
                System.Threading.Interlocked.Increment(ref this.completedNodeCount);
                item.CompletedEvent += CompletedNode;
            }
            else
            {
                Log.DebugMessage("Not duplicating already present Node in the collection: '{0}'", item.ToString());
            }
        }

        public void AddRange(DependencyNodeCollection itemCollection)
        {
            foreach (DependencyNode item in itemCollection)
            {
                this.Add(item);
            }
        }

        public void Clear()
        {
            this.list.Clear();
            this.completedNodeCount = 0;
        }

        public bool Contains(DependencyNode item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(DependencyNode[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(DependencyNode item)
        {
            lock (this.list)
            {
                if (this.list.Contains(item))
                {
                    System.Threading.Interlocked.Decrement(ref this.completedNodeCount);
                }
            }

            // do not remove the CompletedEvent delegate
            return this.list.Remove(item);
        }

        public System.Collections.Generic.IEnumerator<DependencyNode> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public DependencyNode this[int index]
        {
            get
            {
                return this.list[index];
            }
        }

        public int Rank
        {
            get;
            private set;
        }

        public System.Threading.ManualResetEvent[] AllNodesCompletedEvent
        {
            get;
            private set;
        }

        public void FilterOutputPaths(System.Enum filter, StringArray paths)
        {
            foreach (DependencyNode node in this.list)
            {
                if (node.Module is IModuleCollection)
                {
                    DependencyNodeCollection childNodes = node.Children;
                    if (null != childNodes)
                    {
                        childNodes.FilterOutputPaths(filter, paths);
                    }
                }
                else
                {
                    node.FilterOutputPaths(filter, paths);
                }
            }
        }

        public object Clone()
        {
            DependencyNodeCollection clone = new DependencyNodeCollection();
            clone.Rank = this.Rank;
            clone.AddRange(this);
            return clone;
        }

        public override string ToString()
        {
            System.Text.StringBuilder description = new System.Text.StringBuilder();
            description.AppendFormat("DependencyNode: rank {0} with {1} nodes",
                                     this.Rank, this.list.Count);
            return description.ToString();
        }
    }
}
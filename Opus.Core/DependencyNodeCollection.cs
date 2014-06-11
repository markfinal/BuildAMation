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
                //Log.DebugMessage("Not duplicating already present Node in the collection: '{0}'", item.ToString());
            }
        }

        public void AddRange(DependencyNodeCollection itemCollection)
        {
            foreach (var item in itemCollection)
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

        public void FilterOutputLocations(Array<LocationKey> filterKeys, LocationArray filteredLocations)
        {
            foreach (var node in this.list)
            {
                if (node.Module is IModuleCollection)
                {
                    var childNodes = node.Children;
                    if (null != childNodes)
                    {
                        childNodes.FilterOutputLocations(filterKeys, filteredLocations);
                    }
                }
                else
                {
                    node.FilterOutputLocations(filterKeys, filteredLocations);
                }
            }
        }

        public object Clone()
        {
            var clone = new DependencyNodeCollection();
            clone.Rank = this.Rank;
            clone.AddRange(this);
            return clone;
        }

        public override string ToString()
        {
            var description = new System.Text.StringBuilder();
            description.AppendFormat("DependencyNodeCollection: rank {0} with {1} nodes", this.Rank, this.list.Count);
            return description.ToString();
        }

        public void ReassignRank(int newRank)
        {
            // the Rank set accessor is private, so this function is to ensure that users are aware that
            // changing a collection's rank may have side-effects
            this.Rank = newRank;
        }
    }
}
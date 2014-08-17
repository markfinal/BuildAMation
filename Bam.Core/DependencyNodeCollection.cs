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
namespace Bam.Core
{
    public sealed class DependencyNodeCollection :
        System.Collections.Generic.ICollection<DependencyNode>,
        System.ICloneable
    {
        private System.Collections.Generic.List<DependencyNode> list = new System.Collections.Generic.List<DependencyNode>();
        private int completedNodeCount = 0;

        public
        DependencyNodeCollection() : this(-1)
        {}

        public
        DependencyNodeCollection(
            int rank)
        {
            this.Rank = rank;
            this.AllNodesCompletedEvent = new System.Threading.ManualResetEvent[1];
            this.AllNodesCompletedEvent[0] = new System.Threading.ManualResetEvent(false);
        }

        private void
        CompletedNode(
            DependencyNode node)
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

        public void
        Add(
            DependencyNode item)
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

        public void
        AddRange(
            DependencyNodeCollection itemCollection)
        {
            foreach (var item in itemCollection)
            {
                this.Add(item);
            }
        }

        public void
        Clear()
        {
            this.list.Clear();
            this.completedNodeCount = 0;
        }

        public bool
        Contains(
            DependencyNode item)
        {
            return this.list.Contains(item);
        }

        public void
        CopyTo(
            DependencyNode[] array,
            int arrayIndex)
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

        public bool
        Remove(
            DependencyNode item)
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

        public System.Collections.Generic.IEnumerator<DependencyNode>
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
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

        public void
        FilterOutputLocations(
            Array<LocationKey> filterKeys,
            LocationArray filteredLocations)
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

        public object
        Clone()
        {
            var clone = new DependencyNodeCollection();
            clone.Rank = this.Rank;
            clone.AddRange(this);
            return clone;
        }

        public override string
        ToString()
        {
            var description = new System.Text.StringBuilder();
            description.AppendFormat("DependencyNodeCollection: rank {0} with {1} nodes", this.Rank, this.list.Count);
            return description.ToString();
        }

        public void
        ReassignRank(
            int newRank)
        {
            // the Rank set accessor is private, so this function is to ensure that users are aware that
            // changing a collection's rank may have side-effects
            this.Rank = newRank;
        }
    }
}

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

        public DependencyNodeCollection()
            : this(-1)
        {
        }

        public DependencyNodeCollection(int rank)
        {
            this.Rank = rank;
        }

        public void Add(DependencyNode item)
        {
            if (!this.list.Contains(item))
            {
                this.list.Add(item);
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

        public bool Complete
        {
            get
            {
                bool complete = true;

                if (this.list.Count > 0)
                {
                    foreach (DependencyNode node in this.list)
                    {
                        complete &= (EBuildState.Succeeded == node.BuildState);
                        if (!complete)
                        {
                            break;
                        }
                    }
                }

                return complete;
            }
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
            clone.list.AddRange(this.list);
            return clone;
        }
    }
}
// <copyright file="ModuleCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Bam.Core
{
    public class ModuleCollection :
        System.Collections.Generic.ICollection<IModule>
    {
        private System.Collections.Generic.List<IModule> list = new System.Collections.Generic.List<IModule>();

        public void
        Add(
            IModule item)
        {
            this.list.Add(item);
        }

        public void
        AddRange(
            ModuleCollection itemCollection)
        {
            foreach (var module in itemCollection)
            {
                this.list.Add(module);
            }
        }

        public void
        Clear()
        {
            this.list.Clear();
        }

        public bool
        Contains(
            IModule item)
        {
            return this.list.Contains(item);
        }

        public void
        CopyTo(
            IModule[] array,
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
            IModule item)
        {
            return this.list.Remove(item);
        }

        public System.Collections.Generic.IEnumerator<IModule>
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }
    }
}

// <copyright file="TargetCollection.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public sealed class TargetCollection : System.Collections.Generic.ICollection<Target>
    {
        private System.Collections.Generic.List<Target> list = new System.Collections.Generic.List<Target>();

        public void Add(Target item)
        {
            this.list.Add(item);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public bool Contains(Target item)
        {
            foreach (Target target in this.list)
            {
                if (0 == target.CompareTo(item))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(Target[] array, int arrayIndex)
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

        public bool Remove(Target item)
        {
            return this.list.Remove(item);
        }

        public System.Collections.Generic.IEnumerator<Target> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public Target this[int index]
        {
            get
            {
                return this.list[index];
            }
        }

        public override string ToString()
        {
            string message = null;

            foreach (Target target in this.list)
            {
                message += target.Key + " ";
            }

            return message;
        }
    }
}
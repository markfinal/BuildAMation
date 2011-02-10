// <copyright file="Array.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public class Array<T> : System.Collections.Generic.ICollection<T>
    {
        protected System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();

        public Array()
        {
        }

        public Array(params T[] itemsToAdd)
        {
            this.list.AddRange(itemsToAdd);
        }

        public Array(System.Collections.ICollection collection)
        {
            this.list.AddRange(collection as System.Collections.Generic.IEnumerable<T>);
        }

        public void Add(T item)
        {
            this.list.Add(item);
        }

        public void AddRange(T[] itemsToAdd)
        {
            this.list.AddRange(itemsToAdd);
        }

        public void AddRange(Array<T> array)
        {
            foreach (T item in array)
            {
                this.list.Add(item);
            }
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public bool Contains(T item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
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

        public bool Remove(T item)
        {
            return this.list.Remove(item);
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public object this[int index]
        {
            get
            {
                return this.list[index];
            }
        }

        public T[] ToArray()
        {
            T[] array = this.list.ToArray();
            return array;
        }

        public override string ToString()
        {
            string message = null;
            foreach (T item in this.list)
            {
                message += item.ToString() + " ";
            }
            return message;
        }
    }
}
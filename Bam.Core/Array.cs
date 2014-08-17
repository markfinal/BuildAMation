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
    public class Array<T> :
        System.Collections.Generic.ICollection<T>,
        System.ICloneable
    {
        protected System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();

        public
        Array()
        {}

        public
        Array(
            params T[] itemsToAdd)
        {
            this.list.AddRange(itemsToAdd);
        }

        public
        Array(
            System.Collections.ICollection collection)
        {
            this.list.AddRange(collection as System.Collections.Generic.IEnumerable<T>);
        }

        public virtual void
        Add(
            T item)
        {
            this.list.Add(item);
        }

        public void
        AddUnique(
            T item)
        {
            if (!this.list.Contains(item))
            {
                this.Add(item);
            }
        }

        public void
        AddRange(
            T[] itemsToAdd)
        {
            this.list.AddRange(itemsToAdd);
        }

        public void
        AddRange(
            Array<T> array)
        {
            if (this == array)
            {
                throw new Exception("Cannot add an array to itself");
            }

            foreach (var item in array)
            {
                this.list.Add(item);
            }
        }

        public void
        AddRangeUnique(
            Array<T> array)
        {
            if (this == array)
            {
                throw new Exception("Cannot add an array to itself");
            }

            foreach (var item in array)
            {
                if (!this.list.Contains(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public void
        Insert(
            int index,
            T item)
        {
            this.list.Insert(index, item);
        }

        public void
        Clear()
        {
            this.list.Clear();
        }

        public virtual bool
        Contains(
            T item)
        {
            return this.list.Contains(item);
        }

        public void
        CopyTo(
            T[] array,
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
            T item)
        {
            return this.list.Remove(item);
        }

        public bool
        RemoveAll(
            Array<T> items)
        {
            var success = true;
            foreach (var item in items)
            {
                success &= this.list.Remove(item);
            }

            return success;
        }

        public System.Collections.Generic.IEnumerator<T>
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public T this[int index]
        {
            get
            {
                return this.list[index];
            }
        }

        public T[]
        ToArray()
        {
            var array = this.list.ToArray();
            return array;
        }

        public override string
        ToString()
        {
            return this.ToString(" ");
        }

        public virtual string
        ToString(
            string separator)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var item in this.list)
            {
                builder.AppendFormat("{0}{1}", item.ToString(), separator);
            }
            // remove the trailing separator
            var output = builder.ToString().TrimEnd(separator.ToCharArray());
            return output;
        }

        public void
        Sort()
        {
            this.list.Sort();
        }

        public Array<T>
        Union(
            Array<T> other)
        {
            var union = new Array<T>();
            union.AddRangeUnique(this);
            union.AddRangeUnique(other);
            return union;
        }

        public Array<T>
        Intersect(
            Array<T> other)
        {
            var intersect = new Array<T>();
            foreach (var item in this.list)
            {
                if (other.list.Contains(item))
                {
                    intersect.list.Add(item);
                }
            }
            return intersect;
        }

        public Array<T>
        Complement(
            Array<T> other)
        {
            var complement = new Array<T>();
            foreach (var item in this.list)
            {
                if (!other.list.Contains(item))
                {
                    complement.list.Add(item);
                }
            }
            return complement;
        }

        public override bool
        Equals(
            object obj)
        {
            var other = obj as Array<T>;
            if (this.list.Count != other.list.Count)
            {
                return false;
            }

            foreach (var item in this.list)
            {
                if (!other.list.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        public override int
        GetHashCode()
        {
            return base.GetHashCode();
        }

        public Array<T>
        SubArray(
            int firstIndex,
            int count)
        {
            var newArray = new Array<T>();
            int lastIndex = firstIndex + count;
            for (int index = firstIndex; index < lastIndex; ++index)
            {
                newArray.Add(this[index]);
            }
            return newArray;
        }

        #region ICloneable Members

        object
        System.ICloneable.Clone()
        {
            var clone = new Array<T>();
            foreach (var item in this.list)
            {
                clone.Add(item);
            }
            return clone;
        }

        #endregion
    }
}

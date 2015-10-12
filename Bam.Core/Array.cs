#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace Bam.Core
{
    public class Array<T> :
        System.Collections.Generic.ICollection<T>
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
            System.Collections.Generic.IEnumerable<T> items)
        {
            this.list.AddRange(items);
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
        AddRangeUnique(
            System.Collections.Generic.IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                if (this.list.Contains(item))
                {
                    continue;
                }
                this.list.Add(item);
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

        public virtual string
        ToString(
            char separator)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var item in this.list)
            {
                builder.AppendFormat("{0}{1}", item.ToString(), separator);
            }
            // remove the trailing separator
            var output = builder.ToString().TrimEnd(separator);
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

        public System.Collections.ObjectModel.ReadOnlyCollection<T>
        ToReadOnlyCollection()
        {
            return new System.Collections.ObjectModel.ReadOnlyCollection<T>(this.list);
        }
    }
}

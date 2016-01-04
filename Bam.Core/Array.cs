#region License
// Copyright (c) 2010-2016, Mark Final
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
    /// <summary>
    /// Generic wrapper around a List, with set functionality.
    /// </summary>
    public class Array<T> :
        System.Collections.Generic.ICollection<T>
    {
        /// <summary>
        /// The underlying list storage, initialized to an empty list.
        /// </summary>
        protected System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();

        private void
        AddToEnd(
            T item)
        {
            if (null == item)
            {
                throw new Exception("Cannot add null");
            }
            this.list.Add(item);
        }

        private void
        AddMultipleToEnd(
            System.Collections.Generic.IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.AddToEnd(item);
            }
        }

        /// <summary>
        /// Default constructor. The list is empty.
        /// </summary>
        public
        Array()
        {}

        /// <summary>
        /// Create a list from an array of objects.
        /// </summary>
        /// <param name="itemsToAdd">Array of objects to add to the list.</param>
        public
        Array(
            params T[] itemsToAdd)
        {
            this.AddMultipleToEnd(itemsToAdd);
        }

        /// <summary>
        /// Create a list from an enumeration of objects.
        /// </summary>
        /// <param name="items">IEnumerable of objects to add to the list.</param>
        public
        Array(
            System.Collections.Generic.IEnumerable<T> items)
        {
            this.AddMultipleToEnd(items);
        }

        /// <summary>
        /// Add a single object to the end of an existing list.
        /// Virtual as can be overridden in subclasses.
        /// </summary>
        /// <param name="item">Object to be added</param>
        public virtual void
        Add(
            T item)
        {
            this.AddToEnd(item);
        }

        /// <summary>
        /// Add a single object to the end of an existing list, if that list does not already contain it.
        /// </summary>
        /// <param name="item">Object to be added uniquely.</param>
        public void
        AddUnique(
            T item)
        {
            if (this.list.Contains(item))
            {
                return;
            }
            this.AddToEnd(item);
        }

        /// <summary>
        /// Add an array of objects to the end of an existing list.
        /// </summary>
        /// <param name="itemsToAdd">Array of objects to add.</param>
        public void
        AddRange(
            T[] itemsToAdd)
        {
            this.AddMultipleToEnd(itemsToAdd);
        }

        /// <summary>
        /// Append an existing Array to the end of an existing list.
        /// </summary>
        /// <param name="array">Array of objects to add.</param>
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
                this.AddToEnd(item);
            }
        }

        /// <summary>
        /// Append an enumeration of objects to the end of an existing list.
        /// </summary>
        /// <param name="items">An enumeration of objects to add.</param>
        public void
        AddRange(
            System.Collections.Generic.IEnumerable<T> items)
        {
            this.AddMultipleToEnd(items);
        }

        /// <summary>
        /// Appends the contents of an existing Array to the end of an existing list, if they are not already in that list.
        /// </summary>
        /// <param name="array">Array of objects to add uniquely.</param>
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
                if (this.list.Contains(item))
                {
                    continue;
                }
                this.AddToEnd(item);
            }
        }

        /// <summary>
        /// Appends an enumeration of objects to the end of an existing list, if they are not already in that list.
        /// </summary>
        /// <param name="list">Enumeration of objects to add uniquely.</param>
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
                this.AddToEnd(item);
            }
        }

        /// <summary>
        /// Insert an object into the specified position in the existing list.
        /// Any objects in the list at, or subsequent, to this index are moved to accomodate the new object.
        /// </summary>
        /// <param name="index">Zero-based index to insert the new object.</param>
        /// <param name="item">Object to insert into the list.</param>
        public void
        Insert(
            int index,
            T item)
        {
            if (null == item)
            {
                throw new Exception("Cannot insert null at index {0}", index);
            }
            this.list.Insert(index, item);
        }

        /// <summary>
        /// Remove all objects from the list.
        /// </summary>
        public void
        Clear()
        {
            this.list.Clear();
        }

        /// <summary>
        /// Determine whether an item is stored in the list.
        /// Virtual so that sub classes can override the behaviour.
        /// </summary>
        /// <param name="item">Object to check whether it is contained in the list.</param>
        /// <returns><c>true</c> if the object is in the list. <c>false</c> otherwise.</returns>
        public virtual bool
        Contains(
            T item)
        {
            return this.list.Contains(item);
        }

        /// <summary>
        /// Copy a number of elements, starting from the index, to the specified array.
        /// </summary>
        /// <param name="array">Array to receive the sub-section of the list.</param>
        /// <param name="arrayIndex">Index of the list to being copying.</param>
        public void
        CopyTo(
            T[] array,
            int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Determine the number of elements in the list.
        /// </summary>
        /// <value>An integer count of the list size.</value>
        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        /// <summary>
        /// Is the list readonly? Required by the ICollection interface.
        /// </summary>
        /// <value>Always <c>false</c>.</value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Remove the specified object from the list if it is contained within it.
        /// </summary>
        /// <param name="item">Object to remove from the list.</param>
        /// <returns><c>true</c> if the object was removed. <c>false</c> if the object was not removed, or not found.</returns>
        public bool
        Remove(
            T item)
        {
            return this.list.Remove(item);
        }

        /// <summary>
        /// Removes all objects from the list if they are contained within it.
        /// </summary>
        /// <returns><c>true</c>, if all objects were removed, <c>false</c> if not all objects were removed, or not found.</returns>
        /// <param name="items">Array of objects to remove.</param>
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

        /// <summary>
        /// Part of the IEnumerable interface.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public System.Collections.Generic.IEnumerator<T>
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        /// <summary>
        /// Part of the IEnumerable interface.
        /// </summary>
        /// <returns>The enumerator.</returns>
        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        /// <summary>
        /// Gets the object specified by the given index.
        /// </summary>
        /// <param name="index">Index of the object in the list desired.</param>
        /// <value>Object at the given index.</value>
        public T this[int index]
        {
            get
            {
                return this.list[index];
            }
        }

        /// <summary>
        /// Convert the list to an array.
        /// </summary>
        /// <returns>An array containing the elements of the list.</returns>
        public T[]
        ToArray()
        {
            var array = this.list.ToArray();
            return array;
        }

        /// <summary>
        /// String conversion of the array.
        /// </summary>
        /// <returns>A <see cref="string"/> of all items in the array separated by a space.</returns>
        public override string
        ToString()
        {
            return this.ToString(" ");
        }

        /// <summary>
        /// String conversion of the array, with a custom separator.
        /// Virtual to allow sub-classes to override the behaviour.
        /// </summary>
        /// <returns>A <see cref="string"/> of all items in the array separated by the given separator string.</returns>
        /// <param name="separator">The separator between each element of the array in the returned string.</param>
        public virtual string
        ToString(
            string separator)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var item in this.list)
            {
                if (null == item)
                {
                    throw new Exception("Item in array was null");
                }
                builder.AppendFormat("{0}{1}", item.ToString(), separator);
            }
            // remove the trailing separator
            var output = builder.ToString().TrimEnd(separator.ToCharArray());
            return output;
        }

        /// <summary>
        /// String conversion of the array, with a custom single character separator.
        /// Virtual to allow sub-classes to override the behaviour.
        /// </summary>
        /// <returns>A <see cref="string"/> of all items in the array separated by the given character.</returns>
        /// <param name="separator">The single character separator between each element of the array in the returned string.</param>
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

        /// <summary>
        /// Sorts the list, in place, using the default comparer for the type.
        /// </summary>
        public void
        Sort()
        {
            this.list.Sort();
        }

        /// <summary>
        /// Combine the elements of two lists uniquely.
        /// </summary>
        /// <param name="other">The second list to union with.</param>
        /// <returns>The array containing the combination of all elements of the arrays.</returns>
        public Array<T>
        Union(
            Array<T> other)
        {
            var union = new Array<T>();
            union.AddRangeUnique(this);
            union.AddRangeUnique(other);
            return union;
        }

        /// <summary>
        /// Find all elements that exist in two lists.
        /// </summary>
        /// <param name="other">The second list to intersect with.</param>
        /// <returns>The array containing just those elements in both arrays.</returns>
        public Array<T>
        Intersect(
            Array<T> other)
        {
            var intersect = new Array<T>();
            foreach (var item in this.list)
            {
                if (!other.list.Contains(item))
                {
                    continue;
                }
                intersect.AddToEnd(item);
            }
            return intersect;
        }

        /// <summary>
        /// Find all elements in this but not in <paramref name="other"/>
        /// </summary>
        /// <param name="other">The other list</param>
        /// <returns>The array containing the complement of the two arrays.</returns>
        public Array<T>
        Complement(
            Array<T> other)
        {
            var complement = new Array<T>();
            foreach (var item in this.list)
            {
                if (other.list.Contains(item))
                {
                    continue;
                }
                complement.AddToEnd(item);
            }
            return complement;
        }

        /// <summary>
        /// Do the two arrays contain exactly the same elements?
        /// </summary>
        /// <param name="obj">The other array to compare against</param>
        /// <returns><c>true</c> if the two arrays contain the same elements. <c>false</c> otherwise.</returns>
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

        /// <summary>
        /// Return the hash code of the list within.
        /// Required by overriding Equals.
        /// </summary>
        /// <returns>Hash code of the internal list.</returns>
        public override int
        GetHashCode()
        {
            return this.list.GetHashCode();
        }

        /// <summary>
        /// Extracts part of the existing array into another.
        /// </summary>
        /// <returns>The sub-array</returns>
        /// <param name="firstIndex">First index of the existing array to start at.</param>
        /// <param name="count">Number of elements to extract.</param>
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

        /// <summary>
        /// Convert the internal list into a ReadOnlyCollection/> 
        /// </summary>
        /// <returns>A read-only version of the internal list.</returns>
        public System.Collections.ObjectModel.ReadOnlyCollection<T>
        ToReadOnlyCollection()
        {
            return new System.Collections.ObjectModel.ReadOnlyCollection<T>(this.list);
        }
    }
}

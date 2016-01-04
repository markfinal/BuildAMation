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
    /// Array of strings class, derived from the Array class.
    /// </summary>
    public sealed class StringArray :
        Array<string>,
        ISetOperations<StringArray>
    {
        /// <summary>
        /// Create an instance.
        /// </summary>
        public
        StringArray() : base()
        {}

        /// <summary>
        /// Construct a new instance from an array of strings.
        /// </summary>
        /// <param name="itemsToAdd">Items to add.</param>
        public
        StringArray(
            params string[] itemsToAdd)
        {
            // specialization, to avoid empty strings from being added
            foreach (var item in itemsToAdd)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        /// <summary>
        /// Construct an instance from an enumerable of strings.
        /// </summary>
        /// <param name="items">Items.</param>
        public
        StringArray(
            System.Collections.Generic.IEnumerable<string> items)
        {
            // specialization, to avoid empty strings from being added
            foreach (string item in items)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        /// <summary>
        /// Construct an instance from another StringArray.
        /// </summary>
        /// <param name="array">Array.</param>
        public
        StringArray(
            StringArray array)
        {
            // specialization, to avoid empty strings from being added
            foreach (var item in array)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        /// <summary>
        /// Construct an instance from an array of strings.
        /// </summary>
        /// <param name="array">Array.</param>
        public
        StringArray(
            Array<string> array)
        {
            // specialization, to avoid empty strings from being added
            foreach (var item in array)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        /// <summary>
        /// Add a single string to the end of the array.
        /// </summary>
        /// <param name="item">Item.</param>
        public override void
        Add(
            string item)
        {
            // specialization, to avoid empty strings from being added
            if (System.String.IsNullOrEmpty(item))
            {
                return;
            }

            this.list.Add(item);
        }

        /// <summary>
        /// Convert the array of strings to a single string separated by spaces.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Bam.Core.StringArray"/>.</returns>
        public override string
        ToString()
        {
            return this.ToString(' ');
        }

        /// <summary>
        /// Convert the array of strings to a single string separated by the specified character.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="separator">Separator.</param>
        public override string
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
        /// Remove any duplicates from the array.
        /// </summary>
        public void
        RemoveDuplicates()
        {
            var newList = new System.Collections.Generic.List<string>();
            foreach (var item in this.list)
            {
                if (!newList.Contains(item))
                {
                    newList.Add(item);
                }
            }

            this.list = newList;
        }

        #region ISetOperations implementation

        StringArray
        ISetOperations<StringArray>.Complement(
            StringArray other)
        {
            return new StringArray((this as Array<string>).Complement(other as Array<string>));
        }

        StringArray
        ISetOperations<StringArray>.Intersect(
            StringArray other)
        {
            return new StringArray((this as Array<string>).Intersect(other as Array<string>));
        }

        #endregion
    }
}

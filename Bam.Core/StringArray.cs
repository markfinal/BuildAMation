#region License
// Copyright 2010-2015 Mark Final
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
#endregion // License
namespace Bam.Core
{
    public sealed class StringArray :
        Array<string>,
        System.ICloneable,
        ISetOperations<StringArray>
    {
        public
        StringArray() : base()
        {}

        public
        StringArray(
            params string[] itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public
        StringArray(
            System.Collections.ICollection collection)
        {
            foreach (string item in collection)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public
        StringArray(
            StringArray array)
        {
            foreach (var item in array)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public
        StringArray(
            Array<string> array)
        {
            foreach (var item in array)
            {
                if (!System.String.IsNullOrEmpty(item))
                {
                    this.list.Add(item);
                }
            }
        }

        public override void
        Add(
            string item)
        {
            if (System.String.IsNullOrEmpty(item))
            {
                return;
            }

            this.list.Add(item);
        }

        public override string
        ToString()
        {
            return this.ToString(' ');
        }

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

        object
        System.ICloneable.Clone()
        {
            var clone = new StringArray();
            clone.list.AddRange(this.list);
            return clone;
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

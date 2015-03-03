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
    public sealed class OutputPaths :
        System.Collections.IEnumerable
    {
        private System.Collections.Generic.SortedDictionary<System.Enum, string> fileDictionary;

        public
        OutputPaths()
        {
            this.fileDictionary = new System.Collections.Generic.SortedDictionary<System.Enum, string>();
        }

        public
        OutputPaths(
            OutputPaths source)
        {
            this.fileDictionary = new System.Collections.Generic.SortedDictionary<System.Enum, string>(source.fileDictionary);
        }

        public string this[System.Enum key]
        {
            get
            {
                if (this.Has(key))
                {
                    return this.fileDictionary[key];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value != null)
                {
                    this.fileDictionary[key] = value;
                }
                else if (this.Has(key))
                {
                    this.fileDictionary.Remove(key);
                }
            }
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.fileDictionary.GetEnumerator();
        }

        public bool
        Has(
            System.Enum key)
        {
            bool containsKey = this.fileDictionary.ContainsKey(key);
            return containsKey;
        }

        public void
        Remove(
            System.Enum key)
        {
            this.fileDictionary.Remove(key);
        }

        public Array<System.Enum> Types
        {
            get
            {
                return new Array<System.Enum>(this.fileDictionary.Keys);
            }
        }

        public StringArray Paths
        {
            get
            {
                return new StringArray(this.fileDictionary.Values);
            }
        }
    }
}

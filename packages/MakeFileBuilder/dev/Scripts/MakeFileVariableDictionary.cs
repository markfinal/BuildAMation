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
namespace MakeFileBuilder
{
    public sealed class MakeFileVariableDictionary :
        System.Collections.Generic.Dictionary<Bam.Core.LocationKey, Bam.Core.StringArray>
    {
        public void
        Append(
            MakeFileVariableDictionary dictionary)
        {
            foreach (var itemPair in dictionary)
            {
                this.Add(itemPair.Key, itemPair.Value);
            }
        }

        public new void
        Add(
            Bam.Core.LocationKey key,
            Bam.Core.StringArray value)
        {
            if (this.ContainsKey(key))
            {
                this[key].AddRange(value);
            }
            else
            {
                base.Add(key, value);
            }
        }

        public Bam.Core.StringArray Variables
        {
            get
            {
                var variables = new Bam.Core.StringArray();
                foreach (var item in this.Values)
                {
                    variables.AddRange(item);
                }

                return variables;
            }
        }

        public MakeFileVariableDictionary
        Filter(
            Bam.Core.Array<Bam.Core.LocationKey> filterKeys)
        {
            var filteredDictionary = new MakeFileVariableDictionary();
            foreach (var key in this.Keys)
            {
                if (filterKeys.Contains(key))
                {
                    filteredDictionary.Add(key, this[key]);
                }
            }
            return filteredDictionary;
        }
    }
}

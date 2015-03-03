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
    public sealed class DirectoryCollectionEnumerator :
        System.Collections.IEnumerator
    {
        private DirectoryCollection collection;
        private int enumeratorIndex;

        public
        DirectoryCollectionEnumerator(
            DirectoryCollection collection)
        {
            this.collection = collection;
            this.Reset();
        }

        public object Current
        {
            get
            {
                return this.collection[this.enumeratorIndex];
            }
        }

        public bool
        MoveNext()
        {
            ++this.enumeratorIndex;
            if (this.enumeratorIndex >= this.collection.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void
        Reset()
        {
            this.enumeratorIndex = -1;
        }
    }
}

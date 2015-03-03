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
    public class ModuleCollection :
        System.Collections.Generic.ICollection<IModule>
    {
        private System.Collections.Generic.List<IModule> list = new System.Collections.Generic.List<IModule>();

        public void
        Add(
            IModule item)
        {
            this.list.Add(item);
        }

        public void
        AddRange(
            ModuleCollection itemCollection)
        {
            foreach (var module in itemCollection)
            {
                this.list.Add(module);
            }
        }

        public void
        Clear()
        {
            this.list.Clear();
        }

        public bool
        Contains(
            IModule item)
        {
            return this.list.Contains(item);
        }

        public void
        CopyTo(
            IModule[] array,
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
            IModule item)
        {
            return this.list.Remove(item);
        }

        public System.Collections.Generic.IEnumerator<IModule>
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator
        System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }
    }
}

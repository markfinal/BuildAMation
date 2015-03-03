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
namespace VSSolutionBuilder
{
    public sealed class ProjectFileConfigurationCollection :
        System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectFileConfiguration> list = new System.Collections.Generic.List<ProjectFileConfiguration>();

        public System.Collections.IEnumerator
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public void
        Add(
            ProjectFileConfiguration fileConfiguration)
        {
            this.list.Add(fileConfiguration);
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public bool
        Contains(
            string name)
        {
            bool containsName = false;
            foreach (var fileConfiguration in this.list)
            {
                if (fileConfiguration.Configuration.Name == name)
                {
                    containsName = true;
                    break;
                }
            }

            return containsName;
        }
    }
}

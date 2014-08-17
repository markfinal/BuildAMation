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
namespace VSSolutionBuilder
{
    public sealed class ProjectToolCollection :
        System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectTool> list = new System.Collections.Generic.List<ProjectTool>();

        public void
        Add(
            ProjectTool tool)
        {
            this.list.Add(tool);
        }

        public int Count
        {
            get
            {
                var count = this.list.Count;
                return count;
            }
        }

        public bool
        Contains(
            string toolName)
        {
            foreach (var tool in this.list)
            {
                if (toolName == tool.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public System.Collections.IEnumerator
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public ProjectTool this[string toolName]
        {
            get
            {
                foreach (var tool in this.list)
                {
                    if (toolName == tool.Name)
                    {
                        return tool;
                    }
                }

                throw new Bam.Core.Exception("There is no ProjectTool called '{0}'", toolName);
            }
        }

        public void
        SerializeMSBuild(
            MSBuildProject project,
            ProjectConfiguration configuration,
            System.Uri projectUri)
        {
            var toolItemGroup = project.CreateItemDefinitionGroup();
            var split = configuration.ConfigurationPlatform();
            toolItemGroup.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);

            foreach (var tool in this.list)
            {
                tool.SerializeMSBuild(toolItemGroup, configuration, projectUri);
            }
        }
    }
}

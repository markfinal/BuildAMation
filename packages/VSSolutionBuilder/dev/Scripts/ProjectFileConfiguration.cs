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
#endregion // License
namespace VSSolutionBuilder
{
    public sealed class ProjectFileConfiguration
    {
        public
        ProjectFileConfiguration(
            ProjectConfiguration configuration,
            ProjectTool tool,
            bool excludedFromBuild)
        {
            this.Configuration = configuration;
            this.Tool = tool;
            this.ExcludedFromBuild = excludedFromBuild;
        }

        public ProjectConfiguration Configuration
        {
            get;
            private set;
        }

        public ProjectTool Tool
        {
            get;
            private set;
        }

        public bool ExcludedFromBuild
        {
            get;
            private set;
        }

        public System.Xml.XmlElement
        Serialize(
            System.Xml.XmlDocument document,
            System.Uri projectUri)
        {
            ProjectTool parentTool = null;
            // TODO: convert to var
            foreach (ProjectTool tool in this.Configuration.Tools)
            {
                if (tool.Name == this.Tool.Name)
                {
                    parentTool = tool;
                    break;
                }
            }

            if (null != parentTool)
            {
                if (this.Tool.Equals(parentTool) && !this.ExcludedFromBuild)
                {
                    return null;
                }
            }

            if (this.ExcludedFromBuild || (this.Tool.AttributeCount > 1))
            {
                var fileConfigurationElement = document.CreateElement("FileConfiguration");

                fileConfigurationElement.SetAttribute("Name", this.Configuration.Name);
                if (this.ExcludedFromBuild)
                {
                    fileConfigurationElement.SetAttribute("ExcludedFromBuild", this.ExcludedFromBuild.ToString());

                    if (this.Tool.AttributeCount > 1) // always 1 because of the name
                    {
                        fileConfigurationElement.AppendChild(this.Tool.Serialize(document, this, projectUri, parentTool));
                    }
                }
                else
                {
                    if (this.Tool.AttributeCount > 1) // always 1 because of the name
                    {
                        fileConfigurationElement.AppendChild(this.Tool.Serialize(document, this, projectUri, parentTool));
                    }
                }
                return fileConfigurationElement;
            }
            else
            {
                return null;
            }
        }
    }
}

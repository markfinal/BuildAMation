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
    public class MSBuildProject :
        MSBuildBaseElement
    {
        public
        MSBuildProject(
            System.Xml.XmlDocument document,
            string toolsVersion,
            string defaultTargets) : base(document, "Project")
        {
            if (null != defaultTargets)
            {
                this.XmlElement.SetAttribute("DefaultTargets", defaultTargets);
            }
            this.XmlElement.SetAttribute("ToolsVersion", toolsVersion);
            this.XmlDocument.AppendChild(this.XmlElement);
        }

        public
        MSBuildProject(
            System.Xml.XmlDocument document,
            string toolsVersion)
            : this(document, toolsVersion, null)
        {}

        public MSBuildPropertyGroup
        CreatePropertyGroup()
        {
            var propertyGroup = new MSBuildPropertyGroup(this.XmlDocument);
            this.AppendChild(propertyGroup);
            return propertyGroup;
        }

        public MSBuildItemGroup
        CreateItemGroup()
        {
            var itemGroup = new MSBuildItemGroup(this.XmlDocument);
            this.AppendChild(itemGroup);
            return itemGroup;
        }

        public MSBuildItemDefinitionGroup
        CreateItemDefinitionGroup()
        {
            var itemDefGroup = new MSBuildItemDefinitionGroup(this.XmlDocument);
            this.AppendChild(itemDefGroup);
            return itemDefGroup;
        }

        public MSBuildImport
        CreateImport(
            string projectFile)
        {
            var import = new MSBuildImport(this.XmlDocument, projectFile);
            this.AppendChild(import);
            return import;
        }
    }
}

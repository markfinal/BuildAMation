// <copyright file="MSBuildProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class MSBuildProject : MSBuildBaseElement
    {
        public MSBuildProject(System.Xml.XmlDocument document, string toolsVersion, string defaultTargets)
            : base(document, "Project")
        {
            if (null != defaultTargets)
            {
                this.XmlElement.SetAttribute("DefaultTargets", defaultTargets);
            }
            this.XmlElement.SetAttribute("ToolsVersion", toolsVersion);
            this.XmlDocument.AppendChild(this.XmlElement);
        }

        public MSBuildProject(System.Xml.XmlDocument document, string toolsVersion)
            : this(document, toolsVersion, null)
        {
        }

        public MSBuildPropertyGroup CreatePropertyGroup()
        {
            MSBuildPropertyGroup propertyGroup = new MSBuildPropertyGroup(this.XmlDocument);
            this.AppendChild(propertyGroup);
            return propertyGroup;
        }

        public MSBuildItemGroup CreateItemGroup()
        {
            MSBuildItemGroup itemGroup = new MSBuildItemGroup(this.XmlDocument);
            this.AppendChild(itemGroup);
            return itemGroup;
        }

        public MSBuildItemDefinitionGroup CreateItemDefinitionGroup()
        {
            MSBuildItemDefinitionGroup itemDefGroup = new MSBuildItemDefinitionGroup(this.XmlDocument);
            this.AppendChild(itemDefGroup);
            return itemDefGroup;
        }

        public MSBuildImport CreateImport(string projectFile)
        {
            MSBuildImport import = new MSBuildImport(this.XmlDocument, projectFile);
            this.AppendChild(import);
            return import;
        }
    }
}
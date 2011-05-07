// <copyright file="MSBuildProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class MSBuildProject : MSBuildBaseElement
    {
        public MSBuildProject(System.Xml.XmlDocument document, string toolsVersion)
            : base(document, "Project")
        {
            this.XmlElement.SetAttribute("DefaultTargets", "Build");
            this.XmlElement.SetAttribute("ToolsVersion", toolsVersion);
            this.XmlDocument.AppendChild(this.XmlElement);
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
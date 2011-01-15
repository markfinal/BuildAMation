// <copyright file="ProjectFileConfiguration.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectFileConfiguration
    {
        public ProjectFileConfiguration(ProjectConfiguration configuration, ProjectTool tool, bool excludedFromBuild)
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

        public void Serialize(System.Xml.XmlWriter xmlWriter, System.Uri projectUri)
        {
            ProjectTool parentTool = null;
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
                    return;
                }
            }

            xmlWriter.WriteStartElement("FileConfiguration");
            {
                xmlWriter.WriteAttributeString("Name", this.Configuration.Name);
                if (this.ExcludedFromBuild)
                {
                    xmlWriter.WriteAttributeString("ExcludedFromBuild", this.ExcludedFromBuild.ToString());

                    if (this.Tool.AttributeCount > 1) // always 1 because of the name
                    {
                        this.Tool.Serialize(xmlWriter, this, projectUri, parentTool);
                    }
                }
                else
                {
                    this.Tool.Serialize(xmlWriter, this, projectUri, parentTool);
                }
            }
            xmlWriter.WriteEndElement();
        }
    }
}
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

        public System.Xml.XmlElement Serialize(System.Xml.XmlDocument document, System.Uri projectUri)
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
                    return null;
                }
            }

            if (this.ExcludedFromBuild || (this.Tool.AttributeCount > 1))
            {
                System.Xml.XmlElement fileConfigurationElement = document.CreateElement("FileConfiguration");

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

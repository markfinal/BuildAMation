// <copyright file="ProjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectFile
    {
        public ProjectFile(string pathName)
        {
            this.RelativePath = pathName;
        }

        public string RelativePath
        {
            get;
            private set;
        }

        public ProjectFileConfigurationCollection FileConfigurations
        {
            get;
            set;
        }

        public void Serialize(System.Xml.XmlDocument document, System.Xml.XmlElement parentElement, System.Uri projectUri, string[] splitFirDirs, int index)
        {
            if (index == splitFirDirs.Length - 1)
            {
                string relativePath = Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);

                System.Xml.XmlElement fileElement = document.CreateElement("File");
                fileElement.SetAttribute("RelativePath", relativePath);
                if (null != this.FileConfigurations)
                {
                    foreach (ProjectFileConfiguration configuration in this.FileConfigurations)
                    {
                        System.Xml.XmlElement configurationElement = configuration.Serialize(document, projectUri);
                        if (null != configurationElement)
                        {
                            fileElement.AppendChild(configurationElement);
                        }
                    }
                }

                parentElement.AppendChild(fileElement);
            }
            else
            {
                string dirName = splitFirDirs[index];

                System.Xml.XmlElement directoryElement = null;
                foreach (System.Xml.XmlElement child in parentElement.ChildNodes)
                {
                    if ("Filter" == child.Name)
                    {
                        bool hasFilterAttribute = child.HasAttribute("Name");
                        if (hasFilterAttribute)
                        {
                            string filterAttributeValue = child.GetAttribute("Name");
                            if (dirName == filterAttributeValue)
                            {
                                directoryElement = child;
                                break;
                            }
                        }
                    }
                }

                if (null == directoryElement)
                {
                    directoryElement = document.CreateElement("Filter");
                    directoryElement.SetAttribute("Name", dirName);
                    parentElement.AppendChild(directoryElement);
                }

                this.Serialize(document, directoryElement, projectUri, splitFirDirs, ++index);
            }
        }

        public void SerializeMSBuild(MSBuildItemGroup fileCollectionGroup, System.Uri projectUri, string name)
        {
            if (null == this.FileConfigurations)
            {
                string relativePath = Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);
                fileCollectionGroup.CreateItem(name, relativePath);
            }
            else
            {
                foreach (ProjectFileConfiguration configuration in this.FileConfigurations)
                {
                    ProjectTool parentTool = null;
                    foreach (ProjectTool tool in configuration.Configuration.Tools)
                    {
                        if (tool.Name == configuration.Tool.Name)
                        {
                            parentTool = tool;
                            break;
                        }
                    }

                    string relativePath = Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);
                    configuration.Tool.SerializeMSBuild(fileCollectionGroup, configuration, projectUri, relativePath, parentTool);
                }
            }
        }

        public void SerializeCSBuild(MSBuildItemGroup fileCollectionGroup, System.Uri projectUri)
        {
            string relativePath = Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);
            fileCollectionGroup.CreateItem("Compile", relativePath);
        }
    }
}
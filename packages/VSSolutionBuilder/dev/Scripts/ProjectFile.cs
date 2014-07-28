// <copyright file="ProjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectFile
    {
        public
        ProjectFile(
            string pathName)
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

        public void
        Serialize(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentElement,
            System.Uri projectUri,
            string[] splitFileDirs,
            int index)
        {
            if (index == splitFileDirs.Length - 1)
            {
                var relativePath = Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);

                var fileElement = document.CreateElement("File");
                fileElement.SetAttribute("RelativePath", relativePath);
                if (null != this.FileConfigurations)
                {
                    // TODO: convert to var
                    foreach (ProjectFileConfiguration configuration in this.FileConfigurations)
                    {
                        var configurationElement = configuration.Serialize(document, projectUri);
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
                var dirName = splitFileDirs[index];

                System.Xml.XmlElement directoryElement = null;
                // TODO: convert to var
                foreach (System.Xml.XmlElement child in parentElement.ChildNodes)
                {
                    if ("Filter" == child.Name)
                    {
                        var hasFilterAttribute = child.HasAttribute("Name");
                        if (hasFilterAttribute)
                        {
                            var filterAttributeValue = child.GetAttribute("Name");
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

                this.Serialize(document, directoryElement, projectUri, splitFileDirs, ++index);
            }
        }

        public void
        SerializeMSBuild(
            MSBuildItemGroup fileCollectionGroup,
            System.Uri projectUri,
            string name)
        {
            if (null == this.FileConfigurations)
            {
                var relativePath = Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);
                fileCollectionGroup.CreateItem(name, relativePath);
            }
            else
            {
                // TODO: convert to var
                foreach (ProjectFileConfiguration configuration in this.FileConfigurations)
                {
                    ProjectTool parentTool = null;
                    // TODO: convert to var
                    foreach (ProjectTool tool in configuration.Configuration.Tools)
                    {
                        if (tool.Name == configuration.Tool.Name)
                        {
                            parentTool = tool;
                            break;
                        }
                    }

                    var relativePath = Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);
                    configuration.Tool.SerializeMSBuild(fileCollectionGroup, configuration, projectUri, relativePath, parentTool);
                }
            }
        }

        public void
        SerializeCSBuild(
            MSBuildItemGroup fileCollectionGroup,
            System.Uri projectUri,
            System.Uri packageDirectoryUri)
        {
            var relativePath = Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);
            var compileItem = fileCollectionGroup.CreateItem("Compile", relativePath);

            var relativeToPackage = Opus.Core.RelativePathUtilities.GetPath(this.RelativePath, packageDirectoryUri);
            if (relativePath != relativeToPackage)
            {
                compileItem.CreateMetaData("Link", relativeToPackage);
            }
        }
    }
}

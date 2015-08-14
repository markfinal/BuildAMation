#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
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
                var relativePath = Bam.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);

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
                var relativePath = Bam.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);
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

                    var relativePath = Bam.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);
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
            var relativePath = Bam.Core.RelativePathUtilities.GetPath(this.RelativePath, projectUri);
            var compileItem = fileCollectionGroup.CreateItem("Compile", relativePath);

            var relativeToPackage = Bam.Core.RelativePathUtilities.GetPath(this.RelativePath, packageDirectoryUri);
            if (relativePath != relativeToPackage)
            {
                compileItem.CreateMetaData("Link", relativeToPackage);
            }
        }
    }
}

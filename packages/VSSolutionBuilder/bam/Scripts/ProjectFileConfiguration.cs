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

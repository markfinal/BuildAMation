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
    public sealed class ProjectTool
    {
        System.Collections.Generic.Dictionary<string, string> attributes = new System.Collections.Generic.Dictionary<string, string>();

        public
        ProjectTool(
            string name)
        {
            this.attributes.Add("Name", name);
        }

        public string Name
        {
            get
            {
                return this.attributes["Name"];
            }
        }

        public string this[string key]
        {
            get
            {
                return this.attributes[key];
            }

            set
            {
                this.attributes[key] = value;
            }
        }

        public void
        AddAttribute(
            string name,
            string value)
        {
            this.attributes.Add(name, value);
        }

        public bool
        HasAttribute(
            string name)
        {
            var hasAttribute = this.attributes.ContainsKey(name);
            return hasAttribute;
        }

        public int AttributeCount
        {
            get
            {
                int count = this.attributes.Count;
                return count;
            }
        }

        public override bool
        Equals(
            object o)
        {
            var tool = o as ProjectTool;

            if (this.attributes.Count != tool.attributes.Count)
            {
                // this can happen when source files are excluded from a configuration
                return false;
            }

            foreach (var attribute in this.attributes)
            {
                if (!tool.HasAttribute(attribute.Key))
                {
                    //Core.Log.Message("Attribute '{0}' is not present in both ProjectTools '{1}'", attribute.Key, this.Name);
                    return false;
                }

                if (attribute.Value != tool[attribute.Key])
                {
                    //Core.Log.Message("Attribute '{0}' is different: '{1}' vs '{2}'", attribute.Key, attribute.Value, tool[attribute.Key]);
                    return false;
                }
            }

            return true;
        }

        // need to override this because Equals is overridden
        public override int
        GetHashCode()
        {
            return base.GetHashCode();
        }

        public System.Xml.XmlElement
        Serialize(
            System.Xml.XmlDocument document,
            ProjectConfiguration configuration,
            System.Uri projectUri)
        {
            var projectName = configuration.Project.Name;
            var outputDirectory = configuration.OutputDirectory;
            var intermediateDirectory = configuration.IntermediateDirectory;

            var toolElement = document.CreateElement("Tool");

            foreach (var attribute in this.attributes)
            {
                if (System.String.IsNullOrEmpty(attribute.Value))
                {
                    continue;
                }

                // TODO: need to fix the ObjectFileName reference so that it's per object file
                if ("ObjectFile" == attribute.Key)
                {
                    continue;
                }

                var value = VSSolutionBuilder.RefactorPathForVCProj(attribute.Value, outputDirectory, intermediateDirectory, projectName, projectUri);
                toolElement.SetAttribute(attribute.Key, value);
            }

            return toolElement;
        }

        public System.Xml.XmlElement
        Serialize(
            System.Xml.XmlDocument document,
            ProjectFileConfiguration configuration,
            System.Uri projectUri,
            ProjectTool parent)
        {
            var projectName = configuration.Configuration.Project.Name;
            var outputDirectory = configuration.Configuration.OutputDirectory;
            var intermediateDirectory = configuration.Configuration.IntermediateDirectory;

            var toolElement = document.CreateElement("Tool");

            foreach (var attribute in this.attributes)
            {
                if (System.String.IsNullOrEmpty(attribute.Value))
                {
                    continue;
                }

                var value = attribute.Value;
                value = VSSolutionBuilder.RefactorPathForVCProj(value, outputDirectory, intermediateDirectory, projectName, projectUri);
                toolElement.SetAttribute(attribute.Key, value);
            }

            return toolElement;
        }

        public void
        SerializeMSBuild(
            MSBuildItemDefinitionGroup itemDefGroup,
            ProjectConfiguration configuration,
            System.Uri projectUri)
        {
            var projectName = configuration.Project.Name;
            var outputDirectory = configuration.OutputDirectory;
            var intermediateDirectory = configuration.IntermediateDirectory;

            string toolElementName = null;
            switch (this.Name)
            {
                case "VCCLCompilerTool":
                    toolElementName = "ClCompile";
                    break;

                case "VCLibrarianTool":
                    toolElementName = "Lib";
                    break;

                case "VCLinkerTool":
                    toolElementName = "Link";
                    break;

                case "VCCustomBuildTool":
                    toolElementName = "CustomBuild";
                    break;

                case "VCPreBuildEventTool":
                    toolElementName = "PreBuildEvent";
                    break;

                case "VCPostBuildEventTool":
                    toolElementName = "PostBuildEvent";
                    break;

                case "VCResourceCompilerTool":
                    toolElementName = "ResourceCompile";
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported VisualStudio tool name, '{0}'", this.Name);
            }

            var toolItem = itemDefGroup.CreateItem(toolElementName);
            foreach (var attribute in this.attributes)
            {
                if (System.String.IsNullOrEmpty(attribute.Value))
                {
                    continue;
                }

                // No ObjectFileName either, as this will be in the common area
                if (("Name" != attribute.Key) &&
                    ("ObjectFileName" != attribute.Key))
                {
                    var value = attribute.Value;
                    value = VSSolutionBuilder.RefactorPathForVCProj(value, outputDirectory, intermediateDirectory, projectName, projectUri);
                    toolItem.CreateMetaData(attribute.Key, value);
                }
            }
        }

        public void
        SerializeMSBuild(
            MSBuildItemGroup itemGroup,
            ProjectFileConfiguration configuration,
            System.Uri projectUri,
            string relativePath,
            ProjectTool parentTool)
        {
            var projectName = configuration.Configuration.Project.Name;
            var outputDirectory = configuration.Configuration.OutputDirectory;
            var intermediateDirectory = configuration.Configuration.IntermediateDirectory;

            string toolElementName = null;
            switch (this.Name)
            {
                case "VCCLCompilerTool":
                    toolElementName = "ClCompile";
                    break;

                case "VCLibrarianTool":
                    toolElementName = "Lib";
                    break;

                case "VCLinkerTool":
                    toolElementName = "Link";
                    break;

                case "VCCustomBuildTool":
                    toolElementName = "CustomBuild";
                    break;

                case "VCPostBuildEventTool":
                    toolElementName = "PostBuildEvent";
                    break;

                case "VCResourceCompilerTool":
                    toolElementName = "ResourceCompile";
                    break;

                default:
                    throw new Bam.Core.Exception("Unsupported VisualStudio tool name, '{0}'", this.Name);
            }

            var toolItem = itemGroup.FindItem(toolElementName, relativePath);
            if (null == toolItem)
            {
                toolItem = itemGroup.CreateItem(toolElementName, relativePath);
            }
            var split = configuration.Configuration.ConfigurationPlatform();
            if (configuration.ExcludedFromBuild)
            {
                var excluded = toolItem.CreateMetaData("ExcludedFromBuild", "true");
                excluded.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
            }
            foreach (var attribute in this.attributes)
            {
                if ("Name" == attribute.Key)
                {
                    continue;
                }

                var value = attribute.Value;
                value = VSSolutionBuilder.RefactorPathForVCProj(value, outputDirectory, intermediateDirectory, projectName, projectUri);

                MSBuildMetaData metaData;
                if (this.Name == "VCCLCompilerTool")
                {
                    // per-source file compiler options extend the defaults for some keys
                    // TODO: find a better way to identify an extension, possibly marking attributes as absolute or delta
                    switch (attribute.Key)
                    {
                        case "PreprocessorDefinitions":
                        case "AdditionalIncludeDirectories":
                            {
                                metaData = toolItem.CreateMetaData(attribute.Key, value + ";%(" + attribute.Key + ")");
                                metaData.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                            }
                            break;

                        default:
                            metaData = toolItem.CreateMetaData(attribute.Key, value);
                            metaData.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                            break;
                    }
                }
                else
                {
                    metaData = toolItem.CreateMetaData(attribute.Key, value);
                    metaData.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                }
            }
        }

        public void
        SerializeCSBuild(
            MSBuildPropertyGroup configurationGroup,
            ProjectConfiguration configuration,
            System.Uri projectUri)
        {
            var projectName = configuration.Project.Name;
            var outputDirectory = configuration.OutputDirectory;

            foreach (var attribute in this.attributes)
            {
                if ("Name" != attribute.Key)
                {
                    var value = attribute.Value;
                    // no intermediate directory
                    value = VSSolutionBuilder.RefactorPathForVCProj(value, outputDirectory, projectName, projectUri);
                    configurationGroup.CreateProperty(attribute.Key, value);
                }
            }
        }
    }
}

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
    public sealed class ProjectConfiguration
    {
        private EProjectConfigurationType type;
        private EProjectCharacterSet characterSet = EProjectCharacterSet.Undefined;

        public
        ProjectConfiguration(
            string name,
            IProject project)
        {
            this.Name = name;
            this.type = EProjectConfigurationType.Undefined;
            this.CharacterSet = EProjectCharacterSet.Undefined;
            this.Tools = new ProjectToolCollection();
            this.Project = project;
            this.Properties = new System.Collections.Generic.Dictionary<string, string>();
        }

        public string Name
        {
            get;
            private set;
        }

        public IProject Project
        {
            get;
            private set;
        }

        public Bam.Core.Location OutputDirectory
        {
            get;
            set;
        }

        public Bam.Core.Location IntermediateDirectory
        {
            get;
            set;
        }

        public string TargetName
        {
            get;
            set;
        }

        public EProjectConfigurationType Type
        {
            get
            {
                return this.type;
            }

            set
            {
                if (EProjectConfigurationType.Undefined == this.type)
                {
                    this.type = value;
                }
                else if (this.type != value)
                {
                    throw new Bam.Core.Exception("Project configuration type already set to '{0}'; cannot change to '{1}'", this.type.ToString(), value.ToString());
                }
            }
        }

        public EProjectCharacterSet CharacterSet
        {
            get
            {
                return this.characterSet;
            }

            set
            {
                if (EProjectCharacterSet.Undefined == this.characterSet)
                {
                    this.characterSet = value;
                }
                else if (this.characterSet != value)
                {
                    throw new Bam.Core.Exception("Project configuration character set already set to '{0}'; cannot change to '{1}'", this.characterSet.ToString(), value.ToString());
                }
            }
        }

        // TODO: Ideally this should now return a read-only collection
        public ProjectToolCollection Tools
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, string> Properties
        {
            get;
            private set;
        }

        public void
        AddToolIfMissing(
            ProjectTool tool)
        {
            lock (this.Tools)
            {
                if (!this.HasTool(tool.Name))
                {
                    this.Tools.Add(tool);
                }
            }
        }

        public bool
        HasTool(
            string toolName)
        {
            var hasTool = this.Tools.Contains(toolName);
            return hasTool;
        }

        public ProjectTool
        GetTool(
            string toolName)
        {
            lock (this.Tools)
            {
                if (this.Tools.Contains(toolName))
                {
                    return this.Tools[toolName];
                }
                else
                {
                    return null;
                }
            }
        }

        public System.Xml.XmlElement
        Serialize(
            System.Xml.XmlDocument document,
            System.Uri projectUri)
        {
            if (this.Type == EProjectConfigurationType.Undefined)
            {
                throw new Bam.Core.Exception("Project type is undefined");
            }
#if false
            if (this.CharacterSet == EProjectCharacterSet.Undefined)
            {
                throw new Bam.Core.Exception("Project character set is undefined");
            }
#endif

            var configurationElement = document.CreateElement("Configuration");

            configurationElement.SetAttribute("Name", this.Name);
            if (null != this.OutputDirectory)
            {
                var outputDir = Bam.Core.RelativePathUtilities.GetPath(this.OutputDirectory, projectUri);
                // MSBuild complains if the output directory does not end with a trailing slash - not strictly necessary here, but consistent
                if (!outputDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                {
                    outputDir += System.IO.Path.DirectorySeparatorChar;
                }
                configurationElement.SetAttribute("OutputDirectory", outputDir);
            }
            if (null != this.IntermediateDirectory)
            {
                var intermediateDir = Bam.Core.RelativePathUtilities.GetPath(this.IntermediateDirectory, projectUri);
                // MSBuild complains if the intermediate directory does not end with a trailing slash - not strictly necessary here, but consistent
                if (!intermediateDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                {
                    intermediateDir += System.IO.Path.DirectorySeparatorChar;
                }
                configurationElement.SetAttribute("IntermediateDirectory", intermediateDir);
            }
            configurationElement.SetAttribute("ConfigurationType", this.Type.ToString("D"));
#if false
            configurationElement.SetAttribute("CharacterSet", this.CharacterSet.ToString("D"));
#endif

            if (this.Tools.Count > 0)
            {
                // TODO: convert to var
                foreach (ProjectTool tool in this.Tools)
                {
                    configurationElement.AppendChild(tool.Serialize(document, this, projectUri));
                }
            }

            return configurationElement;
        }

        public string[]
        ConfigurationPlatform()
        {
            var split = this.Name.Split('|');
            return split;
        }

        public void
        SerializeMSBuild(
            MSBuildItemGroup configurationGroup,
            System.Uri projectUri)
        {
            if (this.Type == EProjectConfigurationType.Undefined)
            {
                throw new Bam.Core.Exception("Project type is undefined");
            }
#if false
            if (this.CharacterSet == EProjectCharacterSet.Undefined)
            {
                throw new Bam.Core.Exception("Project character set is undefined");
            }
#endif

            var projectConfiguration = configurationGroup.CreateItem("ProjectConfiguration", this.Name);

            var split = this.Name.Split('|');

            projectConfiguration.CreateMetaData("Configuration", split[0]);
            projectConfiguration.CreateMetaData("Platform", split[1]);
        }

        public void
        SerializeCSBuild(
            MSBuildProject project,
            System.Uri projectUri)
        {
            if (this.Type == EProjectConfigurationType.Undefined)
            {
                throw new Bam.Core.Exception("Project type is undefined");
            }
            if (this.CharacterSet == EProjectCharacterSet.Undefined)
            {
                throw new Bam.Core.Exception("Project character set is undefined");
            }

            var configurationGroup = project.CreatePropertyGroup();

            var split = this.Name.Split('|');
            configurationGroup.Condition = System.String.Format(" '$(Configuration)|$(Platform)' == '{0}|{1}' ", split[0], split[1]);

            configurationGroup.CreateProperty("OutputPath", Bam.Core.RelativePathUtilities.GetPath(this.OutputDirectory, projectUri));

            // TODO: convert to var
            foreach (ProjectTool tool in this.Tools)
            {
                tool.SerializeCSBuild(configurationGroup, this, projectUri);
            }
        }
    }
}

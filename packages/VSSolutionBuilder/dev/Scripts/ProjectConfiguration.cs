#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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

// <copyright file="ProjectConfiguration.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectConfiguration
    {
        private EProjectConfigurationType type;
        private EProjectCharacterSet characterSet = EProjectCharacterSet.Undefined;

        public ProjectConfiguration(string name, C.IToolchainOptions optionCollection, ProjectData project)
        {
            this.Name = name;
            this.type = EProjectConfigurationType.Undefined;
            this.CharacterSet = (EProjectCharacterSet)optionCollection.CharacterSet;
            this.Tools = new ProjectToolCollection();
            this.Project = project;
        }

        public string Name
        {
            get;
            private set;
        }

        public ProjectData Project
        {
            get;
            private set;
        }

        public string OutputDirectory
        {
            get;
            set;
        }

        public string IntermediateDirectory
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
                    throw new Opus.Core.Exception(System.String.Format("Project configuration type already set to '{0}'; cannot change to '{1}'", this.type.ToString(), value.ToString()));
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
                    throw new Opus.Core.Exception(System.String.Format("Project configuration character set already set to '{0}'; cannot change to '{1}'", this.characterSet.ToString(), value.ToString()));
                }
            }
        }

        // TODO: Ideally this should now return a read-only collection
        public ProjectToolCollection Tools
        {
            get;
            private set;
        }

        public void AddToolIfMissing(ProjectTool tool)
        {
            lock (this.Tools)
            {
                if (!this.HasTool(tool.Name))
                {
                    this.Tools.Add(tool);
                }
            }
        }

        public bool HasTool(string toolName)
        {
            bool hasTool = this.Tools.Contains(toolName);
            return hasTool;
        }

        public ProjectTool GetTool(string toolName)
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

        public void Serialize(System.Xml.XmlWriter xmlWriter, System.Uri projectUri)
        {
            if (this.Type == EProjectConfigurationType.Undefined)
            {
                throw new Opus.Core.Exception("Project type is undefined");
            }
            if (this.CharacterSet == EProjectCharacterSet.Undefined)
            {
                throw new Opus.Core.Exception("Project character set is undefined");
            }

            xmlWriter.WriteStartElement("Configuration");
            {
                xmlWriter.WriteAttributeString("Name", this.Name);
                xmlWriter.WriteAttributeString("OutputDirectory", Opus.Core.RelativePathUtilities.GetPath(this.OutputDirectory, projectUri));
                xmlWriter.WriteAttributeString("IntermediateDirectory", Opus.Core.RelativePathUtilities.GetPath(this.IntermediateDirectory, projectUri));
                xmlWriter.WriteAttributeString("ConfigurationType", this.Type.ToString("D"));
                xmlWriter.WriteAttributeString("CharacterSet", this.CharacterSet.ToString("D"));

                if (this.Tools.Count > 0)
                {
                    foreach (ProjectTool tool in this.Tools)
                    {
                        tool.Serialize(xmlWriter, this, projectUri);
                    }
                }
            }
            xmlWriter.WriteEndElement();
        }
    }
}
// <copyright file="ProjectConfigurationCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectConfigurationCollection : System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectConfiguration> list = new System.Collections.Generic.List<ProjectConfiguration>();

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public void Add(ProjectConfiguration configuration)
        {
            this.list.Add(configuration);
        }

        public bool Contains(string configurationName)
        {
            foreach (ProjectConfiguration configuration in this.list)
            {
                if (configurationName == configuration.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public ProjectConfiguration this[string configurationName]
        {
            get
            {
                foreach (ProjectConfiguration configuration in this.list)
                {
                    if (configurationName == configuration.Name)
                    {
                        return configuration;
                    }
                }

                throw new Opus.Core.Exception(System.String.Format("There is no ProjectConfiguration called '{0}'", configurationName));
            }
        }

        public void SerializeMSBuild(System.Xml.XmlDocument document, System.Xml.XmlElement projectElement, System.Uri projectUri, string xmlNamespace)
        {
            // ProjectConfigurations item group
            {
                System.Xml.XmlElement configurationsElement = document.CreateElement("", "ItemGroup", xmlNamespace);
                configurationsElement.SetAttribute("Label", "ProjectConfigurations");
                foreach (ProjectConfiguration configuration in this.list)
                {
                    configurationsElement.AppendChild(configuration.SerializeMSBuild(document, projectUri, xmlNamespace));
                }
                projectElement.AppendChild(configurationsElement);
            }

            // configuration type and character set
            foreach (ProjectConfiguration configuration in this.list)
            {
                string[] split = configuration.ConfigurationPlatform();

                {
                    System.Xml.XmlElement configurationElement = document.CreateElement("", "PropertyGroup", xmlNamespace);
                    configurationElement.SetAttribute("Condition", System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]));
                    configurationElement.SetAttribute("Label", "Configuration");
                    {
                        System.Xml.XmlElement configurationTypeElement = document.CreateElement("", "ConfigurationType", xmlNamespace);
                        configurationTypeElement.InnerText = configuration.Type.ToString();
                        configurationElement.AppendChild(configurationTypeElement);
                    }
                    {
                        System.Xml.XmlElement characterSetElement = document.CreateElement("", "CharacterSet", xmlNamespace);
                        characterSetElement.InnerText = configuration.CharacterSet.ToString();
                        configurationElement.AppendChild(characterSetElement);
                    }
                    projectElement.AppendChild(configurationElement);
                }
            }

            // import property sheets AFTER the configuration types
            {
                System.Xml.XmlElement importElement = document.CreateElement("", "Import", xmlNamespace);
                importElement.SetAttribute("Project", @"$(VCTargetsPath)\Microsoft.Cpp.props");
                projectElement.AppendChild(importElement);
            }

            // output and intermediate directories
            foreach (ProjectConfiguration configuration in this.list)
            {
                string[] split = configuration.ConfigurationPlatform();

                System.Xml.XmlElement dirElement = document.CreateElement("", "PropertyGroup", xmlNamespace);
                {
                    System.Xml.XmlElement projectFileVersion = document.CreateElement("", "_ProjectFileVersion", xmlNamespace);
                    projectFileVersion.InnerText = "10.0.40219.1"; // TODO and this means what?
                    dirElement.AppendChild(projectFileVersion);
                }
                {
                    System.Xml.XmlElement outDirElement = document.CreateElement("", "OutDir", xmlNamespace);
                    outDirElement.SetAttribute("Condition", System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]));
                    string outputDir = Opus.Core.RelativePathUtilities.GetPath(configuration.OutputDirectory, projectUri);
                    if (!outputDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                    {
                        outputDir += System.IO.Path.DirectorySeparatorChar;
                    }
                    outDirElement.InnerText = outputDir;
                    dirElement.AppendChild(outDirElement);
                }
                {
                    System.Xml.XmlElement intDirElement = document.CreateElement("", "IntDir", xmlNamespace);
                    intDirElement.SetAttribute("Condition", System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]));
                    string intermediateDir = Opus.Core.RelativePathUtilities.GetPath(configuration.IntermediateDirectory, projectUri);
                    if (!intermediateDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                    {
                        intermediateDir += System.IO.Path.DirectorySeparatorChar;
                    }
                    intDirElement.InnerText = intermediateDir;
                    dirElement.AppendChild(intDirElement);
                }
                projectElement.AppendChild(dirElement);
            }

            // tools
            foreach (ProjectConfiguration configuration in this.list)
            {
                projectElement.AppendChild(configuration.Tools.SerializeMSBuild(document, configuration, projectUri, xmlNamespace));
            }
        }
    }
}
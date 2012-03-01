// <copyright file="ProjectTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectTool
    {
        System.Collections.Generic.Dictionary<string, string> attributes = new System.Collections.Generic.Dictionary<string, string>();

        public ProjectTool(string name)
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

        public void AddAttribute(string name, string value)
        {
            this.attributes.Add(name, value);
        }

        public bool HasAttribute(string name)
        {
            bool hasAttribute = this.attributes.ContainsKey(name);
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

        public override bool Equals(object o)
        {
            ProjectTool tool = o as ProjectTool;

            if (this.attributes.Count != tool.attributes.Count)
            {
                // this can happen when source files are excluded from a configuration
                return false;
            }

            foreach (System.Collections.Generic.KeyValuePair<string, string> attribute in this.attributes)
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
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public System.Xml.XmlElement Serialize(System.Xml.XmlDocument document, ProjectConfiguration configuration, System.Uri projectUri)
        {
            string projectName = configuration.Project.Name;
            string outputDirectory = configuration.OutputDirectory;
            string intermediateDirectory = configuration.IntermediateDirectory;

            System.Xml.XmlElement toolElement = document.CreateElement("Tool");

            foreach (System.Collections.Generic.KeyValuePair<string, string> attribute in this.attributes)
            {
                if (System.String.IsNullOrEmpty(attribute.Value))
                {
                    continue;
                }

                string value = VSSolutionBuilder.RefactorPathForVCProj(attribute.Value, outputDirectory, intermediateDirectory, projectName, projectUri);
                toolElement.SetAttribute(attribute.Key, value);
            }

            return toolElement;
        }

        public System.Xml.XmlElement Serialize(System.Xml.XmlDocument document, ProjectFileConfiguration configuration, System.Uri projectUri, ProjectTool parent)
        {
            string projectName = configuration.Configuration.Project.Name;
            string outputDirectory = configuration.Configuration.OutputDirectory;
            string intermediateDirectory = configuration.Configuration.IntermediateDirectory;

            System.Xml.XmlElement toolElement = document.CreateElement("Tool");

            foreach (System.Collections.Generic.KeyValuePair<string, string> attribute in this.attributes)
            {
                // this is necessary in case the parent (from the ProjectConfiguration) is
                // a C interface, while the ProjectFileConfiguration tool is C++
                if ((parent != null) && (parent.HasAttribute(attribute.Key)))
                {
                    string thisValue = attribute.Value;
                    string parentValue = parent[attribute.Key];

                    if ("Name" == attribute.Key || thisValue != parentValue)
                    {
                        thisValue = VSSolutionBuilder.RefactorPathForVCProj(thisValue, outputDirectory, intermediateDirectory, projectName, projectUri);
                        toolElement.SetAttribute(attribute.Key, thisValue);
                    }
                }
                else
                {
                    string value = VSSolutionBuilder.RefactorPathForVCProj(attribute.Value, outputDirectory, intermediateDirectory, projectName, projectUri);
                    toolElement.SetAttribute(attribute.Key, value);
                }
            }

            return toolElement;
        }

        public void SerializeMSBuild(MSBuildItemDefinitionGroup itemDefGroup, ProjectConfiguration configuration, System.Uri projectUri)
        {
            string projectName = configuration.Project.Name;
            string outputDirectory = configuration.OutputDirectory;
            string intermediateDirectory = configuration.IntermediateDirectory;

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

                // TODO: case for VCResourceCompilerTool

                default:
                    throw new Opus.Core.Exception(System.String.Format("Unsupported VisualStudio tool name, '{0}'", this.Name), false);
            }

            MSBuildItem toolItem = itemDefGroup.CreateItem(toolElementName);
            foreach (System.Collections.Generic.KeyValuePair<string, string> attribute in this.attributes)
            {
                if (System.String.IsNullOrEmpty(attribute.Value))
                {
                    continue;
                }

                // No ObjectFileName either, as this will be in the common area
                if (("Name" != attribute.Key) &&
                    ("ObjectFileName" != attribute.Key))
                {
                    string value = attribute.Value;
                    value = VSSolutionBuilder.RefactorPathForVCProj(value, outputDirectory, intermediateDirectory, projectName, projectUri);
                    toolItem.CreateMetaData(attribute.Key, value);
                }
            }
        }

        public void SerializeMSBuild(MSBuildItemGroup itemGroup, ProjectFileConfiguration configuration, System.Uri projectUri, string relativePath, ProjectTool parentTool)
        {
            string projectName = configuration.Configuration.Project.Name;
            string outputDirectory = configuration.Configuration.OutputDirectory;
            string intermediateDirectory = configuration.Configuration.IntermediateDirectory;

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

                default:
                    throw new Opus.Core.Exception(System.String.Format("Unsupported VisualStudio tool name, '{0}'", this.Name), false);
            }

            MSBuildItem toolItem = itemGroup.FindItem(toolElementName, relativePath);
            if (null == toolItem)
            {
                toolItem = itemGroup.CreateItem(toolElementName, relativePath);
            }
            string[] split = configuration.Configuration.ConfigurationPlatform();
            if (configuration.ExcludedFromBuild)
            {
                MSBuildMetaData excluded = toolItem.CreateMetaData("ExcludedFromBuild", "true");
                excluded.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
            }
            foreach (System.Collections.Generic.KeyValuePair<string, string> attribute in this.attributes)
            {
                if ("Name" != attribute.Key)
                {
                    string value = attribute.Value;
                    value = VSSolutionBuilder.RefactorPathForVCProj(value, outputDirectory, intermediateDirectory, projectName, projectUri);

                    // this is necessary in case the parent (from the ProjectConfiguration) is
                    // a C interface, while the ProjectFileConfiguration tool is C++
                    if ((parentTool != null) && (parentTool.HasAttribute(attribute.Key)))
                    {
                        string thisValue = attribute.Value;
                        string parentValue = parentTool[attribute.Key];

                        if ("Name" == attribute.Key || "ObjectFileName" == attribute.Key || thisValue != parentValue)
                        {
                            MSBuildMetaData metaData = toolItem.CreateMetaData(attribute.Key, value);
                            metaData.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                        }
                    }
                    else
                    {
                        MSBuildMetaData metaData = toolItem.CreateMetaData(attribute.Key, value);
                        metaData.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                    }
                }
            }
        }

        public void SerializeCSBuild(MSBuildPropertyGroup configurationGroup, ProjectConfiguration configuration, System.Uri projectUri)
        {
            string projectName = configuration.Project.Name;
            string outputDirectory = configuration.OutputDirectory;

            foreach (System.Collections.Generic.KeyValuePair<string, string> attribute in this.attributes)
            {
                if ("Name" != attribute.Key)
                {
                    string value = attribute.Value;
                    // no intermediate directory
                    value = VSSolutionBuilder.RefactorPathForVCProj(value, outputDirectory, projectName, projectUri);
                    configurationGroup.CreateProperty(attribute.Key, value);
                }
            }
        }
    }
}

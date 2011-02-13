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
    }
}

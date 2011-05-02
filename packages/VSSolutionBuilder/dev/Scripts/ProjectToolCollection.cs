// <copyright file="ProjectToolCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectToolCollection : System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectTool> list = new System.Collections.Generic.List<ProjectTool>();

        public void Add(ProjectTool tool)
        {
            this.list.Add(tool);
        }

        public int Count
        {
            get
            {
                int count = this.list.Count;
                return count;
            }
        }

        public bool Contains(string toolName)
        {
            foreach (ProjectTool tool in this.list)
            {
                if (toolName == tool.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public ProjectTool this[string toolName]
        {
            get
            {
                foreach (ProjectTool tool in this.list)
                {
                    if (toolName == tool.Name)
                    {
                        return tool;
                    }
                }

                throw new Opus.Core.Exception(System.String.Format("There is no ProjectTool called '{0}'", toolName));
            }
        }

        public System.Xml.XmlElement SerializeMSBuild(System.Xml.XmlDocument document, ProjectConfiguration configuration, System.Uri projectUri, string xmlNamespace)
        {
            System.Xml.XmlElement itemDefinitionGroup = document.CreateElement("", "ItemDefinitionGroup", xmlNamespace);
            string[] split = configuration.ConfigurationPlatform();
            itemDefinitionGroup.SetAttribute("Condition", System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]));
            foreach (ProjectTool tool in this.list)
            {
                itemDefinitionGroup.AppendChild(tool.SerializeMSBuild(document, configuration, projectUri, xmlNamespace));
            }
            return itemDefinitionGroup;
        }
    }
}
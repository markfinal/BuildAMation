// <copyright file="ProjectToolCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectToolCollection :
        System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectTool> list = new System.Collections.Generic.List<ProjectTool>();

        public void
        Add(
            ProjectTool tool)
        {
            this.list.Add(tool);
        }

        public int Count
        {
            get
            {
                var count = this.list.Count;
                return count;
            }
        }

        public bool
        Contains(
            string toolName)
        {
            foreach (var tool in this.list)
            {
                if (toolName == tool.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public System.Collections.IEnumerator
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public ProjectTool this[string toolName]
        {
            get
            {
                foreach (var tool in this.list)
                {
                    if (toolName == tool.Name)
                    {
                        return tool;
                    }
                }

                throw new Bam.Core.Exception("There is no ProjectTool called '{0}'", toolName);
            }
        }

        public void
        SerializeMSBuild(
            MSBuildProject project,
            ProjectConfiguration configuration,
            System.Uri projectUri)
        {
            var toolItemGroup = project.CreateItemDefinitionGroup();
            var split = configuration.ConfigurationPlatform();
            toolItemGroup.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);

            foreach (var tool in this.list)
            {
                tool.SerializeMSBuild(toolItemGroup, configuration, projectUri);
            }
        }
    }
}

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
    }
}
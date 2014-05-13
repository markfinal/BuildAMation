// <copyright file="ProjectFileConfigurationCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectFileConfigurationCollection : System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectFileConfiguration> list = new System.Collections.Generic.List<ProjectFileConfiguration>();

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public void Add(ProjectFileConfiguration fileConfiguration)
        {
            this.list.Add(fileConfiguration);
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public bool Contains(string name)
        {
            bool containsName = false;
            foreach (var fileConfiguration in this.list)
            {
                if (fileConfiguration.Configuration.Name == name)
                {
                    containsName = true;
                    break;
                }
            }

            return containsName;
        }
    }
}
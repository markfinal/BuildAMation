// <copyright file="ProjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class ProjectFileCollection : System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectFile> list = new System.Collections.Generic.List<ProjectFile>();

        public System.Collections.IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public void Add(ProjectFile projectFile)
        {
            this.list.Add(projectFile);
        }

        public bool Contains(string sourcePathName)
        {
            string convertedSourcePathName = ProjectFile.ConvertDirectorySeparators(sourcePathName);

            foreach (ProjectFile projectFile in this.list)
            {
                if (convertedSourcePathName == projectFile.RelativePath)
                {
                    return true;
                }
            }

            return false;
        }

        public ProjectFile this[string sourcePathName]
        {
            get
            {
                string convertedSourcePathName = ProjectFile.ConvertDirectorySeparators(sourcePathName);

                foreach (ProjectFile projectFile in this.list)
                {
                    if (convertedSourcePathName == projectFile.RelativePath)
                    {
                        return projectFile;
                    }
                }

                throw new Opus.Core.Exception(System.String.Format("There is no ProjectFile for source path '{0}'", sourcePathName));
            }
        }
    }
}
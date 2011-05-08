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
            foreach (ProjectFile projectFile in this.list)
            {
                if (sourcePathName == projectFile.RelativePath)
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
                foreach (ProjectFile projectFile in this.list)
                {
                    if (sourcePathName == projectFile.RelativePath)
                    {
                        return projectFile;
                    }
                }

                throw new Opus.Core.Exception(System.String.Format("There is no ProjectFile for source path '{0}'", sourcePathName));
            }
        }

        public System.Xml.XmlElement Serialize(System.Xml.XmlDocument document, string filterName, System.Uri projectUri, System.Uri packageDirectoryUri)
        {
            System.Xml.XmlElement sourceFilesFilterElement = document.CreateElement("Filter");
            sourceFilesFilterElement.SetAttribute("Name", filterName);

            foreach (ProjectFile file in this.list)
            {
                string fileRelativeToPackage = Opus.Core.RelativePathUtilities.GetPath(file.RelativePath, packageDirectoryUri);
                string[] splitFileDirs = fileRelativeToPackage.Split(System.IO.Path.DirectorySeparatorChar);
                file.Serialize(document, sourceFilesFilterElement, projectUri, splitFileDirs, 0);
            }

            return sourceFilesFilterElement;
        }

        public void SerializeMSBuild(MSBuildProject project, string childElementName, System.Uri projectUri, System.Uri packageDirectoryUri)
        {
            MSBuildItemGroup fileItemGroup = project.CreateItemGroup();
            foreach (ProjectFile file in this.list)
            {
                file.SerializeMSBuild(fileItemGroup, projectUri, childElementName);
            }
        }

        public void SerializeCSBuild(MSBuildProject project, System.Uri projectUri, System.Uri packageDirectoryUri)
        {
            MSBuildItemGroup fileItemGroup = project.CreateItemGroup();
            foreach (ProjectFile file in this.list)
            {
                file.SerializeCSBuild(fileItemGroup, projectUri);
            }
        }
    }
}
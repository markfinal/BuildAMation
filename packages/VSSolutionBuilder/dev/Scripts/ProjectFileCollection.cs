#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace VSSolutionBuilder
{
    public sealed class ProjectFileCollection :
        System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectFile> list = new System.Collections.Generic.List<ProjectFile>();

        public System.Collections.IEnumerator
        GetEnumerator()
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

        public void
        Add(
            ProjectFile projectFile)
        {
            this.list.Add(projectFile);
        }

        public bool
        Contains(
            string sourcePathName)
        {
            foreach (var projectFile in this.list)
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
                foreach (var projectFile in this.list)
                {
                    if (sourcePathName == projectFile.RelativePath)
                    {
                        return projectFile;
                    }
                }

                throw new Bam.Core.Exception("There is no ProjectFile for source path '{0}'", sourcePathName);
            }
        }

        public System.Xml.XmlElement
        Serialize(
            System.Xml.XmlDocument document,
            string filterName,
            System.Uri projectUri,
            System.Uri packageDirectoryUri)
        {
            var sourceFilesFilterElement = document.CreateElement("Filter");
            sourceFilesFilterElement.SetAttribute("Name", filterName);

            foreach (var file in this.list)
            {
                var fileRelativeToPackage = Bam.Core.RelativePathUtilities.GetPath(file.RelativePath, packageDirectoryUri);
                var splitFileDirs = fileRelativeToPackage.Split(System.IO.Path.DirectorySeparatorChar);
                file.Serialize(document, sourceFilesFilterElement, projectUri, splitFileDirs, 0);
            }

            return sourceFilesFilterElement;
        }

        public void
        SerializeMSBuild(
            MSBuildProject project,
            string childElementName,
            System.Uri projectUri,
            System.Uri packageDirectoryUri)
        {
            var fileItemGroup = project.CreateItemGroup();
            foreach (var file in this.list)
            {
                file.SerializeMSBuild(fileItemGroup, projectUri, childElementName);
            }
        }

        public void
        SerializeCSBuild(
            MSBuildProject project,
            System.Uri projectUri,
            System.Uri packageDirectoryUri)
        {
            var fileItemGroup = project.CreateItemGroup();
            foreach (var file in this.list)
            {
                file.SerializeCSBuild(fileItemGroup, projectUri, packageDirectoryUri);
            }
        }
    }
}

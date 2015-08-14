#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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

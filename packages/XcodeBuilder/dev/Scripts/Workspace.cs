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
namespace XcodeBuilder
{
    public class Workspace :
        IWriteableNode
    {
        public
        Workspace()
        {
            var mainPackage = Bam.Core.State.PackageInfo[0];
            var workspaceBundle = mainPackage.Name + ".xcworkspace";
            this.BundlePath = System.IO.Path.Combine(Bam.Core.State.BuildRoot, workspaceBundle);
            this.WorkspaceDataPath = System.IO.Path.Combine(this.BundlePath, "contents.xcworkspacedata");
            this.Projects = new Bam.Core.Array<PBXProject>();
        }

        public string BundlePath
        {
            get;
            private set;
        }

        public string WorkspaceDataPath
        {
            get;
            private set;
        }

        public Bam.Core.Array<PBXProject> Projects
        {
            get;
            private set;
        }

        public PBXProject
        GetProject(
            Bam.Core.DependencyNode node)
        {
            lock(this.Projects)
            {
                foreach (var project in this.Projects)
                {
                    if (project.Name == node.ModuleName)
                    {
                        return project;
                    }
                }

                var newProject = new PBXProject(node);
                this.Projects.Add(newProject);
                return newProject;
            }
        }

        #region IWriteableNode implementation

        void
        IWriteableNode.Write(
            System.IO.TextWriter writer)
        {
            var settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.NewLineChars = "\n";
            settings.Indent = true;
            using (var xmlWriter = System.Xml.XmlWriter.Create(writer, settings))
            {
                var document = new System.Xml.XmlDocument();

                var root = document.CreateElement("Workspace");
                var versionAttr = document.CreateAttribute("version");
                versionAttr.Value = "1.0";
                root.Attributes.Append(versionAttr);
                document.AppendChild(root);

                foreach (var project in this.Projects)
                {
                    var fileRef = document.CreateElement("FileRef");
                    var locationAttr = document.CreateAttribute("location");
                    var relativeProjectPath = Bam.Core.RelativePathUtilities.GetPath(project.RootUri.AbsoluteUri, this.BundlePath);
                    locationAttr.Value = "group:" + relativeProjectPath;
                    fileRef.Attributes.Append(locationAttr);
                    root.AppendChild(fileRef);
                }

                document.WriteTo(xmlWriter);
                xmlWriter.WriteWhitespace(settings.NewLineChars);
            }
        }

        #endregion
    }
}

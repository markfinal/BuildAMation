#region License
// Copyright 2010-2015 Mark Final
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

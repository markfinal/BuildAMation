// <copyright file="Workspace.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public class Workspace : IWriteableNode
    {
        public Workspace()
        {
            var mainPackage = Opus.Core.State.PackageInfo[0];
            var workspaceBundle = mainPackage.Name + ".xcworkspace";
            this.BundlePath = System.IO.Path.Combine(Opus.Core.State.BuildRoot, workspaceBundle);
            this.WorkspaceDataPath = System.IO.Path.Combine(this.BundlePath, "contents.xcworkspacedata");
            this.Projects = new Opus.Core.Array<PBXProject>();
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

        public Opus.Core.Array<PBXProject> Projects
        {
            get;
            private set;
        }

        public PBXProject GetProject(Opus.Core.DependencyNode node)
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

        void IWriteableNode.Write (System.IO.TextWriter writer)
        {
            var settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.NewLineChars = "\n";
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
                    var relativeProjectPath = Opus.Core.RelativePathUtilities.GetPath(project.RootUri.AbsoluteUri, this.BundlePath);
                    locationAttr.Value = "group:" + relativeProjectPath;
                    fileRef.Attributes.Append(locationAttr);
                    root.AppendChild(fileRef);
                }

                document.WriteTo(xmlWriter);
            }
        }

        #endregion
    }
}

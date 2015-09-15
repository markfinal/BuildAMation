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
    public abstract class XcodeMeta
    {
        public enum Type
        {
            NA,
            StaticLibrary,
            Application,
            DynamicLibrary
        }

        protected XcodeMeta(Bam.Core.Module module, Type type)
        {
            var graph = Bam.Core.Graph.Instance;
            var isReferenced = graph.IsReferencedModule(module);
            this.IsProjectModule = isReferenced;

            var workspace = graph.MetaData as WorkspaceMeta;
            this.ProjectModule = isReferenced ? module : module.GetEncapsulatingReferencedModule();
            this.Project = workspace.FindOrCreateProject(this.ProjectModule, type);

            module.MetaData = this;
        }

        public bool IsProjectModule
        {
            get;
            private set;
        }

        public Bam.Core.Module ProjectModule
        {
            get;
            private set;
        }

        public Project Project
        {
            get;
            set;
        }

        public Target Target
        {
            get;
            protected set;
        }

        public Configuration Configuration
        {
            get;
            protected set;
        }

        public static void PreExecution()
        {
            Bam.Core.Graph.Instance.MetaData = new WorkspaceMeta();
        }

        public static void PostExecution()
        {
            // TODO: some alternatives
            // all modules in the same namespace -> targets in the .xcodeproj
            // one .xcodeproj for all modules -> each a target
            // one project per module, each with one target

            // TODO:
            // create folder <name>.xcodeproj
            // write file project.pbxproj
            // create folder project.xcworkspace
            // create folder xcuserdata

            var workspaceMeta = Bam.Core.Graph.Instance.MetaData as WorkspaceMeta;

            var workspaceContents = new System.Xml.XmlDocument();
            var workspace = workspaceContents.CreateElement("Workspace");
            workspace.Attributes.Append(workspaceContents.CreateAttribute("version")).Value = "1.0";
            workspaceContents.AppendChild(workspace);

            var generateProjectSchemes = Bam.Core.CommandLineProcessor.Evaluate(new GenerateXcodeSchemes());

            foreach (var project in workspaceMeta)
            {
                project.FixupPerConfigurationData();

                var text = new System.Text.StringBuilder();
                text.AppendLine("// !$*UTF8*$!");
                text.AppendLine("{");
                var indentLevel = 1;
                var indent = new string('\t', indentLevel);
                text.AppendFormat("{0}archiveVersion = 1;", indent);
                text.AppendLine();
                text.AppendFormat("{0}classes = {{", indent);
                text.AppendLine();
                text.AppendFormat("{0}}};", indent);
                text.AppendLine();
                text.AppendFormat("{0}objectVersion = 46;", indent);
                text.AppendLine();
                text.AppendFormat("{0}objects = {{", indent);
                text.AppendLine();
                project.Serialize(text, indentLevel + 1);
                text.AppendFormat("{0}}};", indent);
                text.AppendLine();
                text.AppendFormat("{0}rootObject = {1} /* Project object */;", indent, project.GUID);
                text.AppendLine();
                text.AppendLine("}");

                var projectDir = project.ProjectDir;
                if (!System.IO.Directory.Exists(projectDir))
                {
                    System.IO.Directory.CreateDirectory(projectDir);
                }

                //Bam.Core.Log.DebugMessage(text.ToString());
                using (var writer = new System.IO.StreamWriter(project.ProjectPath))
                {
                    writer.Write(text.ToString());
                }

                if (generateProjectSchemes)
                {
                    var projectSchemeCache = new ProjectSchemeCache(project);
                    projectSchemeCache.Serialize();
                }

                var workspaceFileRef = workspaceContents.CreateElement("FileRef");
                workspaceFileRef.Attributes.Append(workspaceContents.CreateAttribute("location")).Value = System.String.Format("group:{0}", projectDir);
                workspace.AppendChild(workspaceFileRef);
            }

            var workspacePath = Bam.Core.TokenizedString.Create("$(buildroot)/$(masterpackagename).xcworkspace/contents.xcworkspacedata", null);
            workspacePath.Parse();

            var workspaceDir = System.IO.Path.GetDirectoryName(workspacePath.ToString());
            if (!System.IO.Directory.Exists(workspaceDir))
            {
                System.IO.Directory.CreateDirectory(workspaceDir);
            }

            var settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Encoding = new System.Text.UTF8Encoding(false); // no BOM
            settings.NewLineChars = System.Environment.NewLine;
            settings.Indent = true;
            settings.ConformanceLevel = System.Xml.ConformanceLevel.Document;

            using (var xmlwriter = System.Xml.XmlWriter.Create(workspacePath.ToString(), settings))
            {
                workspaceContents.WriteTo(xmlwriter);
            }

            var workspaceSettings = new WorkspaceSettings(workspaceDir);
            workspaceSettings.Serialize();

            Bam.Core.Log.Info("Successfully created Xcode workspace for package '{0}'\n\t{1}", Bam.Core.Graph.Instance.MasterPackage.Name, workspaceDir);
        }
    }
}

#region License
// Copyright (c) 2010-2017, Mark Final
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
    public sealed class WorkspaceMeta
    {
        private bool ProjectPerModule = false;
        private System.Collections.Generic.Dictionary<string, Project> ProjectMap = new System.Collections.Generic.Dictionary<string, Project>();
        private System.Collections.Generic.Dictionary<System.Type, Target> TargetMap = new System.Collections.Generic.Dictionary<System.Type, Target>();

        private Project
        EnsureProjectExists(
            Bam.Core.Module module,
            string key)
        {
            lock (this.ProjectMap)
            {
                if (!this.ProjectMap.ContainsKey(key))
                {
                    var newProject = new Project(module, key);
                    this.ProjectMap.Add(key, newProject);
                }
                var project = this.ProjectMap[key];
                project.EnsureProjectConfigurationExists(module);
                return project;
            }
        }

        public Target
        EnsureTargetExists(
            Bam.Core.Module module)
        {
            var moduleType = module.GetType();
            lock (this.TargetMap)
            {
                if (!this.TargetMap.ContainsKey(moduleType))
                {
                    Project project = null;
                    // TODO: remember projects, both by a Module or by a Package
                    if (this.ProjectPerModule)
                    {
                        throw new System.NotSupportedException();
                    }
                    else
                    {
                        project = this.EnsureProjectExists(module, module.PackageDefinition.FullName);
                    }

                    var target = new Target(module, project);
                    this.TargetMap.Add(moduleType, target);

                    project.Targets.Add(moduleType, target);
                }
            }
            if (null == module.MetaData)
            {
                module.MetaData = this.TargetMap[moduleType];
            }
            return module.MetaData as Target;
        }

        private static bool
        AreTextFilesIdentical(
            string targetPath,
            string tempPath)
        {
            var targetSize = new System.IO.FileInfo(targetPath).Length;
            var tempSize = new System.IO.FileInfo(targetPath).Length;
            if (targetSize != tempSize)
            {
                return false;
            }
            using (System.IO.TextReader targetReader = new System.IO.StreamReader(targetPath))
            {
                using (System.IO.TextReader tempReader = new System.IO.StreamReader(tempPath))
                {
                    var targetContents = targetReader.ReadToEnd();
                    var tempContents = tempReader.ReadToEnd();
                    if (0 != System.String.Compare(targetContents, tempContents, false))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void
        WriteXMLIfDifferent(
            string targetPath,
            System.Xml.XmlWriterSettings settings,
            System.Xml.XmlDocument document)
        {
            var targetExists = System.IO.File.Exists(targetPath);
            var writePath = targetExists ? System.IO.Path.GetTempFileName() : targetPath;
            using (var xmlwriter = System.Xml.XmlWriter.Create(writePath, settings))
            {
                //Bam.Core.Log.MessageAll("Writing {0}", writePath);
                document.WriteTo(xmlwriter);
            }
            if (targetExists && !AreTextFilesIdentical(targetPath, writePath))
            {
                //Bam.Core.Log.MessageAll("\tXML has changed, moving {0} to {1}", writePath, targetPath);
                System.IO.File.Delete(targetPath);
                System.IO.File.Move(writePath, targetPath);
            }
        }

        private static void
        WriteProjectFileIfDifferent(
            string targetPath,
            System.Text.StringBuilder contents)
        {
            var targetExists = System.IO.File.Exists(targetPath);
            var writePath = targetExists ? System.IO.Path.GetTempFileName() : targetPath;
            using (var writer = new System.IO.StreamWriter(writePath))
            {
                //Bam.Core.Log.MessageAll("Writing {0}", writePath);
                writer.Write(contents);
            }
            if (targetExists && !AreTextFilesIdentical(targetPath, writePath))
            {
                //Bam.Core.Log.MessageAll("\tText has changed, moving {0} to {1}", writePath, targetPath);
                System.IO.File.Delete(targetPath);
                System.IO.File.Move(writePath, targetPath);
            }
        }

        public string
        Serialize()
        {
            var workspacePath = Bam.Core.TokenizedString.Create("$(buildroot)/$(masterpackagename).xcworkspace/contents.xcworkspacedata", null);
            var workspaceDir = Bam.Core.TokenizedString.Create("@dir($(0))", null, positionalTokens: new Bam.Core.TokenizedStringArray(workspacePath));
            workspaceDir.Parse();
            var workspaceDirectory = workspaceDir.ToString();

            var workspaceDoc = new System.Xml.XmlDocument();
            var workspaceEl = workspaceDoc.CreateElement("Workspace");
            workspaceEl.SetAttribute("version", "1.0");
            workspaceDoc.AppendChild(workspaceEl);

            var generateProjectSchemes = true; // used to be based on a command line option, but now needed for working directory
            foreach (var project in this.ProjectMap.Values)
            {
                project.ResolveDeferredSetup();

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
                Bam.Core.IOWrapper.CreateDirectoryIfNotExists(projectDir.ToString());

                WriteProjectFileIfDifferent(project.ProjectPath, text);

                if (generateProjectSchemes)
                {
                    var projectSchemeCache = new ProjectSchemeCache(project);
                    projectSchemeCache.Serialize();
                }

                var relativeProjectDir = Bam.Core.RelativePathUtilities.GetPath(projectDir.ToString(), workspaceDirectory);
                var workspaceFileRef = workspaceDoc.CreateElement("FileRef");
                workspaceFileRef.SetAttribute("location", System.String.Format("group:{0}", relativeProjectDir));
                workspaceEl.AppendChild(workspaceFileRef);
            }

            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(workspaceDirectory);

            var settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Encoding = new System.Text.UTF8Encoding(false); // no BOM
            settings.NewLineChars = System.Environment.NewLine;
            settings.Indent = true;
            settings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
            WriteXMLIfDifferent(workspacePath.ToString(), settings, workspaceDoc);

            return workspaceDirectory;
        }
    }
}

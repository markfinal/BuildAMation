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
using System.Linq;
namespace XcodeBuilder
{
    public sealed class ProjectSchemeCache
    {
        public
        ProjectSchemeCache(
            Project project)
        {
            this.Project = project;
            this.SchemeDocuments = new System.Collections.Generic.Dictionary<string, System.Xml.XmlDocument>();

            foreach (var target in project.Targets)
            {
                var schemeFilename = System.String.Format("{0}.xcscheme", target.Value.Name);
                var schemePathname = System.String.Format("{0}/xcuserdata/{1}.xcuserdatad/xcschemes/{2}",
                        project.ProjectDir,
                        System.Environment.GetEnvironmentVariable("USER"),
                        schemeFilename);
                var doc = this.CreateSchemePlist(target.Value);
                this.SchemeDocuments.Add(schemePathname, doc);
            }
            this.CreateManagementPlist();
        }

        private void
        CreateBuildActionEntry(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement buildActionEntriesEl,
            Target target,
            Bam.Core.Array<Target> buildActionsCreated)
        {
#if true
#else
            // add all required dependencies in first (order matters)
            foreach (var required in target.TargetDependencies)
            {
                this.CreateBuildActionEntry(doc, buildActionEntriesEl, required., buildActionsCreated);
            }
#endif

            // the same target might appear again while iterating through the required targets of dependencies
            // only add it once (first one is important for ordering)
            if (buildActionsCreated.Contains(target))
            {
                return;
            }

            var buildActionEntry = doc.CreateElement("BuildActionEntry");
            buildActionEntriesEl.AppendChild(buildActionEntry);
            var buildableReference = doc.CreateElement("BuildableReference");
            buildActionEntry.AppendChild(buildableReference);
            {
                buildableReference.SetAttribute("BuildableIdentifier", "primary");
                buildableReference.SetAttribute("BlueprintIdentifier", target.GUID);
                if (null != target.FileReference)
                {
                    buildableReference.SetAttribute("BuildableName", target.FileReference.Name);
                }
                buildableReference.SetAttribute("BlueprintName", target.Name);
                if (target.Project.ProjectDir == this.Project.ProjectDir)
                {
                    buildableReference.SetAttribute("ReferencedContainer",
                        "container:" + System.IO.Path.GetFileName(this.Project.ProjectDir.Parse()));
                }
                else
                {
                    var relative = this.Project.GetRelativePathToProject(target.Project.ProjectDir);
                    buildableReference.SetAttribute("ReferencedContainer",
                        "container:" + relative);
                }
            }

            buildActionsCreated.Add(target);
        }

        private void
        CreateBuildActions(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement schemeElement,
            Target target)
        {
            var buildActionEl = doc.CreateElement("BuildAction");
            schemeElement.AppendChild(buildActionEl);
            {
                buildActionEl.SetAttribute("parallelizeBuildables", "YES");
                buildActionEl.SetAttribute("buildImplicitDependencies", "YES");

                var buildActionEntries = doc.CreateElement("BuildActionEntries");
                buildActionEl.AppendChild(buildActionEntries);

                var buildActionsCreated = new Bam.Core.Array<Target>();
                this.CreateBuildActionEntry(doc, buildActionEntries, target, buildActionsCreated);
            }
        }

        private void
        CreateTestActions(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement schemeElement,
            Target target)
        {
            var testActionEl = doc.CreateElement("TestAction");
            schemeElement.AppendChild(testActionEl);
            {
                testActionEl.SetAttribute("selectedDebuggerIdentifier", "Xcode.DebuggerFoundation.Debugger.LLDB");
                testActionEl.SetAttribute("selectedLauncherIdentifier", "Xcode.DebuggerFoundation.Launcher.LLDB");
                testActionEl.SetAttribute("shouldUseLaunchSchemeArgsEnv", "YES");
                testActionEl.SetAttribute("buildConfiguration", target.ConfigurationList.ElementAt(0).Name);
            }
        }

        private void
        CreateLaunchActions(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement schemeElement,
            Target target)
        {
            var launchActionEl = doc.CreateElement("LaunchAction");
            schemeElement.AppendChild(launchActionEl);
            {
                launchActionEl.SetAttribute("selectedDebuggerIdentifier", "Xcode.DebuggerFoundation.Debugger.LLDB");
                launchActionEl.SetAttribute("selectedLauncherIdentifier", "Xcode.DebuggerFoundation.Launcher.LLDB");
                launchActionEl.SetAttribute("buildConfiguration", target.ConfigurationList.ElementAt(0).Name);

                var productRunnableEl = doc.CreateElement("BuildableProductRunnable");
                launchActionEl.AppendChild(productRunnableEl);

                var buildableRefEl = doc.CreateElement("BuildableReference");
                productRunnableEl.AppendChild(buildableRefEl);

                buildableRefEl.SetAttribute("BuildableIdentifier", "primary");

                if (null != target.FileReference)
                {
                    buildableRefEl.SetAttribute("BlueprintIdentifier", target.FileReference.GUID);
                    buildableRefEl.SetAttribute("BuildableName", target.FileReference.Name);
                    buildableRefEl.SetAttribute("BlueprintName", target.FileReference.Name);
                }

                if (target.Project.ProjectDir == this.Project.ProjectDir)
                {
                    buildableRefEl.SetAttribute("ReferencedContainer",
                        "container:" + System.IO.Path.GetFileName(target.Project.ProjectDir.Parse()));
                }
                else
                {
                    var relative = this.Project.GetRelativePathToProject(target.Project.ProjectDir);
                    buildableRefEl.SetAttribute("ReferencedContainer",
                        "container:" + relative);
                }

                if ((target.Module is C.ConsoleApplication) && (target.Module as C.ConsoleApplication).WorkingDirectory != null)
                {
                    launchActionEl.SetAttribute("useCustomWorkingDirectory", "YES");
                    launchActionEl.SetAttribute("customWorkingDirectory", (target.Module as C.ConsoleApplication).WorkingDirectory.Parse());
                }
            }
        }

        private System.Xml.XmlDocument
        CreateSchemePlist(
            Target target)
        {
            var doc = new System.Xml.XmlDocument();

            var schemeEl = doc.CreateElement("Scheme");
            doc.AppendChild(schemeEl);

            this.CreateBuildActions(doc, schemeEl, target);
            this.CreateTestActions(doc, schemeEl, target);
            this.CreateLaunchActions(doc, schemeEl, target);

            var profileActionEl = doc.CreateElement("ProfileAction");
            schemeEl.AppendChild(profileActionEl);
            profileActionEl.SetAttribute("buildConfiguration", target.ConfigurationList.ElementAt(0).Name);

            var analyzeActionEl = doc.CreateElement("AnalyzeAction");
            schemeEl.AppendChild(analyzeActionEl);
            analyzeActionEl.SetAttribute("buildConfiguration", target.ConfigurationList.ElementAt(0).Name);

            var archiveActionEl = doc.CreateElement("ArchiveAction");
            schemeEl.AppendChild(archiveActionEl);
            archiveActionEl.SetAttribute("buildConfiguration", target.ConfigurationList.ElementAt(0).Name);

            return doc;
        }

        private void
        CreateManagementPlist()
        {
            var doc = new System.Xml.XmlDocument();
            // don't resolve any URLs, or if there is no internet, the process will pause for some time
            doc.XmlResolver = null;

            {
                var type = doc.CreateDocumentType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
                doc.AppendChild(type);
            }
            var plistEl = doc.CreateElement("plist");
            plistEl.SetAttribute("version", "1.0");

            var dictEl = doc.CreateElement("dict");
            plistEl.AppendChild(dictEl);
            doc.AppendChild(plistEl);

            {
                var key = doc.CreateElement("key");
                key.InnerText = "SchemeUserState";
                dictEl.AppendChild(key);

                var valueDict = doc.CreateElement("dict");
                dictEl.AppendChild(valueDict);

                foreach (var scheme in this.SchemeDocuments)
                {
                    var schemeKey = doc.CreateElement("key");
                    schemeKey.InnerText = System.IO.Path.GetFileName(scheme.Key);
                    valueDict.AppendChild(schemeKey);
                }
            }

            this.ManagementDocument = doc;
        }

        private System.Collections.Generic.Dictionary<string, System.Xml.XmlDocument> SchemeDocuments
        {
            get;
            set;
        }

        private System.Xml.XmlDocument ManagementDocument
        {
            get;
            set;
        }

        private Project Project
        {
            get;
            set;
        }

        private void
        Write(
            System.Xml.XmlDocument document,
            string path)
        {
            // do not write a Byte-Ordering-Mark (BOM)
            var encoding = new System.Text.UTF8Encoding(false);

            Bam.Core.IOWrapper.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            using (var writer = new System.IO.StreamWriter(path, false, encoding))
            {
                var settings = new System.Xml.XmlWriterSettings();
                settings.OmitXmlDeclaration = false;
                settings.NewLineChars = "\n";
                settings.Indent = true;
                settings.IndentChars = "   ";
                settings.NewLineOnAttributes = true;
                using (var xmlWriter = System.Xml.XmlWriter.Create(writer, settings))
                {
                    document.WriteTo(xmlWriter);
                    xmlWriter.WriteWhitespace(settings.NewLineChars);
                }
            }
        }

        public void
        Serialize()
        {
            foreach (var scheme in this.SchemeDocuments)
            {
                this.Write(scheme.Value, scheme.Key);
            }

            var schemePathname = System.String.Format("{0}/xcuserdata/{1}.xcuserdatad/xcschemes/xcschememanagement.plist",
                this.Project.ProjectDir,
                System.Environment.GetEnvironmentVariable("USER"));
            this.Write(this.ManagementDocument, schemePathname);
        }
    }
}

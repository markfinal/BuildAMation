// <copyright file="ProjectSchemeCache.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class ProjectSchemeCache
    {
        public
        ProjectSchemeCache(
            PBXProject project)
        {
            if (project.NativeTargets.Count > 1)
            {
                var message = new System.Text.StringBuilder();
                message.AppendFormat("Cannot create scheme cache for project '{0}' with more than one PBXNativeTarget:\n", project.Name);
                // TODO: convert to var
                foreach (PBXNativeTarget target in project.NativeTargets)
                {
                    message.AppendLine("\t" + target.Name);
                }
                throw new Opus.Core.Exception(message.ToString());
            }

            var nativeTarget = project.NativeTargets[0];
            var schemeName = nativeTarget.Name;
            var schemeFilename = schemeName + ".xcscheme";

            this.SchemePath = project.RootUri.AbsolutePath;
            this.SchemePath = System.IO.Path.Combine(this.SchemePath, "xcuserdata");
            this.SchemePath = System.IO.Path.Combine(this.SchemePath, System.Environment.GetEnvironmentVariable("USER") + ".xcuserdatad");
            this.SchemePath = System.IO.Path.Combine(this.SchemePath, "xcschemes");
            this.ManagementPath = System.IO.Path.Combine(this.SchemePath, "xcschememanagement.plist");
            this.SchemePath = System.IO.Path.Combine(this.SchemePath, schemeFilename);
            this.CreateSchemePlist(nativeTarget, project);
            this.CreateManagementPlist(schemeFilename);
        }

        private void
        CreateBuildActionEntry(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement buildActionEntriesEl,
            PBXNativeTarget target,
            PBXProject primaryProject,
            Opus.Core.Array<PBXNativeTarget> buildActionsCreated)
        {
            // add all required dependencies in first (order matters)
            foreach (var required in target.RequiredTargets)
            {
                this.CreateBuildActionEntry(doc, buildActionEntriesEl, required, primaryProject, buildActionsCreated);
            }

            // the same PBXNativeTarget might appear again while iterating through the required targets of dependencies
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
                var buildable = doc.CreateAttribute("BuildableIdentifier");
                buildable.Value = "primary";
                var blueprint = doc.CreateAttribute("BlueprintIdentifier");
                blueprint.Value = target.UUID;
                var buildableName = doc.CreateAttribute("BuildableName");
                buildableName.Value = target.ProductReference.ShortPath;
                var blueprintName = doc.CreateAttribute("BlueprintName");
                blueprintName.Value = target.Name;
                var refContainer = doc.CreateAttribute("ReferencedContainer");
                if (target.Project.Path == primaryProject.Path)
                {
                    refContainer.Value = "container:" + target.Name + ".xcodeproj";
                }
                else
                {
                    var relative = Opus.Core.RelativePathUtilities.GetPath(target.Project.RootUri, primaryProject.RootUri);
                    refContainer.Value = "container:" + relative;
                }
                buildableReference.Attributes.Append(buildable);
                buildableReference.Attributes.Append(blueprint);
                buildableReference.Attributes.Append(buildableName);
                buildableReference.Attributes.Append(blueprintName);
                buildableReference.Attributes.Append(refContainer);
            }

            buildActionsCreated.Add(target);
        }

        private void
        CreateBuildActions(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement schemeElement,
            PBXNativeTarget target,
            PBXProject project)
        {
            var buildActionEl = doc.CreateElement("BuildAction");
            schemeElement.AppendChild(buildActionEl);
            {
                // disable parallel builds, because the order defined must be honoured, and implicit dependencies
                // may not exist, e.g. between static libs, or depending on the output of an executable
                var parallelBuildsAttr = doc.CreateAttribute("parallelizeBuildables");
                parallelBuildsAttr.Value = "NO";
                buildActionEl.Attributes.Append(parallelBuildsAttr);

                var buildImplicitDepsAttr = doc.CreateAttribute("buildImplicitDependencies");
                buildImplicitDepsAttr.Value = "YES";
                buildActionEl.Attributes.Append(buildImplicitDepsAttr);

                var buildActionEntries = doc.CreateElement("BuildActionEntries");
                buildActionEl.AppendChild(buildActionEntries);

                var buildActionsCreated = new Opus.Core.Array<PBXNativeTarget>();
                this.CreateBuildActionEntry(doc, buildActionEntries, target, target.Project, buildActionsCreated);
            }
        }

        private void
        CreateTestActions(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement schemeElement,
            PBXNativeTarget target)
        {
            var testActionEl = doc.CreateElement("TestAction");
            schemeElement.AppendChild(testActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.BuildConfigurationList.BuildConfigurations[0].Name;
                testActionEl.Attributes.Append(buildConfigurationAttr);

                var selectedDebuggerAttr = doc.CreateAttribute("selectedDebuggerIdentifier");
                selectedDebuggerAttr.Value = "Xcode.DebuggerFoundation.Debugger.LLDB";
                testActionEl.Attributes.Append(selectedDebuggerAttr);

                var selectedLauncherAttr = doc.CreateAttribute("selectedLauncherIdentifier");
                selectedLauncherAttr.Value = "Xcode.DebuggerFoundation.Debugger.LLDB";
                testActionEl.Attributes.Append(selectedLauncherAttr);

                var useLaunchSchemeArgsAttr = doc.CreateAttribute("shouldUseLaunchSchemeArgsEnv");
                useLaunchSchemeArgsAttr.Value = "YES";
                testActionEl.Attributes.Append(useLaunchSchemeArgsAttr);
            }
        }

        private void
        CreateLaunchActions(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement schemeElement,
            PBXNativeTarget target,
            PBXProject primaryProject)
        {
            var launchActionEl = doc.CreateElement("LaunchAction");
            schemeElement.AppendChild(launchActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.BuildConfigurationList.BuildConfigurations[0].Name;
                launchActionEl.Attributes.Append(buildConfigurationAttr);

                var selectedDebuggerAttr = doc.CreateAttribute("selectedDebuggerIdentifier");
                selectedDebuggerAttr.Value = "Xcode.DebuggerFoundation.Debugger.LLDB";
                launchActionEl.Attributes.Append(selectedDebuggerAttr);

                var selectedLauncherAttr = doc.CreateAttribute("selectedLauncherIdentifier");
                selectedLauncherAttr.Value = "Xcode.DebuggerFoundation.Debugger.LLDB";
                launchActionEl.Attributes.Append(selectedLauncherAttr);

                var productRunnableEl = doc.CreateElement("BuildableProductRunnable");
                launchActionEl.AppendChild(productRunnableEl);

                var buildableRefEl = doc.CreateElement("BuildableReference");
                productRunnableEl.AppendChild(buildableRefEl);

                var buildableIdAttr = doc.CreateAttribute("BuildableIdentifier");
                buildableIdAttr.Value = "primary";
                buildableRefEl.Attributes.Append(buildableIdAttr);

                var blueprintIdAttr = doc.CreateAttribute("BlueprintIdentifier");
                blueprintIdAttr.Value = target.ProductReference.UUID;
                buildableRefEl.Attributes.Append(blueprintIdAttr);

                var buildableNameAttr = doc.CreateAttribute("BuildableName");
                buildableNameAttr.Value = target.ProductReference.ShortPath;
                buildableRefEl.Attributes.Append(buildableNameAttr);

                var blueprintNameAttr = doc.CreateAttribute("BlueprintName");
                blueprintNameAttr.Value = target.ProductReference.Name;
                buildableRefEl.Attributes.Append(blueprintNameAttr);

                var refContainerAttr = doc.CreateAttribute("ReferencedContainer");
                if (target.Project.Path == primaryProject.Path)
                {
                    refContainerAttr.Value = "container:" + target.Name + ".xcodeproj";
                }
                else
                {
                    var relative = Opus.Core.RelativePathUtilities.GetPath(target.Project.RootUri, primaryProject.RootUri);
                    refContainerAttr.Value = "container:" + relative;
                }
                buildableRefEl.Attributes.Append(refContainerAttr);
            }
        }

        private void
        CreateSchemePlist(
            PBXNativeTarget target,
            PBXProject project)
        {
            var doc = new System.Xml.XmlDocument();

            var schemeEl = doc.CreateElement("Scheme");
            doc.AppendChild(schemeEl);

            this.CreateBuildActions(doc, schemeEl, target, project);
            this.CreateTestActions(doc, schemeEl, target);
            this.CreateLaunchActions(doc, schemeEl, target, project);

            var profileActionEl = doc.CreateElement("ProfileAction");
            schemeEl.AppendChild(profileActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.BuildConfigurationList.BuildConfigurations[0].Name;
                profileActionEl.Attributes.Append(buildConfigurationAttr);
            }

            var analyzeActionEl = doc.CreateElement("AnalyzeAction");
            schemeEl.AppendChild(analyzeActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.BuildConfigurationList.BuildConfigurations[0].Name;
                analyzeActionEl.Attributes.Append(buildConfigurationAttr);
            }

            var archiveActionEl = doc.CreateElement("ArchiveAction");
            schemeEl.AppendChild(archiveActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.BuildConfigurationList.BuildConfigurations[0].Name;
                archiveActionEl.Attributes.Append(buildConfigurationAttr);
            }

            this.SchemeDocument = doc;
        }

        private void
        CreateManagementPlist(
            string schemeFilename)
        {
            var doc = new System.Xml.XmlDocument();
            // don't resolve any URLs, or if there is no internet, the process will pause for some time
            doc.XmlResolver = null;

            {
                var type = doc.CreateDocumentType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
                doc.AppendChild(type);
            }
            var plistEl = doc.CreateElement("plist");
            {
                var versionAttr = doc.CreateAttribute("version");
                versionAttr.Value = "1.0";
                plistEl.Attributes.Append(versionAttr);
            }

            var dictEl = doc.CreateElement("dict");
            plistEl.AppendChild(dictEl);
            doc.AppendChild(plistEl);

            {
                var key = doc.CreateElement("key");
                key.InnerText = "SchemeUserState";
                dictEl.AppendChild(key);

                var valueDict = doc.CreateElement("dict");
                dictEl.AppendChild(valueDict);

                {
                    var schemeKey = doc.CreateElement("key");
                    schemeKey.InnerText = schemeFilename;
                    valueDict.AppendChild(schemeKey);
                }
            }

            this.ManagementDocument = doc;
        }

        private string SchemePath
        {
            get;
            set;
        }

        private System.Xml.XmlDocument SchemeDocument
        {
            get;
            set;
        }

        private string ManagementPath
        {
            get;
            set;
        }

        private System.Xml.XmlDocument ManagementDocument
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

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            using (var writer = new System.IO.StreamWriter(path, false, encoding))
            {
                var settings = new System.Xml.XmlWriterSettings();
                settings.OmitXmlDeclaration = false;
                settings.NewLineChars = "\n";
                settings.Indent = true;
                settings.NewLineOnAttributes = true;
                using (var xmlWriter = System.Xml.XmlWriter.Create(writer, settings))
                {
                    document.WriteTo(xmlWriter);
                }
            }
        }

        public void
        Serialize()
        {
            this.Write(this.SchemeDocument, this.SchemePath);
            this.Write(this.ManagementDocument, this.ManagementPath);
        }
    }
}

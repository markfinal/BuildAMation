// <copyright file="ProjectSchemeCache.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public sealed class ProjectSchemeCache
    {
        public ProjectSchemeCache(PBXProject project)
        {
            if (project.NativeTargets.Count > 1)
            {
                throw new Opus.Core.Exception("Cannot create scheme cache for projects with more than one PBXNativeTarget");
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
            PBXProject primaryProject)
        {
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
                buildableName.Value = target.Name;
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
        }

        private void CreateSchemePlist(PBXNativeTarget target, PBXProject project)
        {
            var doc = new System.Xml.XmlDocument();

            var schemeEl = doc.CreateElement("Scheme");
            doc.AppendChild(schemeEl);

            var buildActionEl = doc.CreateElement("BuildAction");
            schemeEl.AppendChild(buildActionEl);
            {
                var buildActionEntries = doc.CreateElement("BuildActionEntries");
                buildActionEl.AppendChild(buildActionEntries);

                // add all required dependencies in first (order matters)
                foreach (var required in target.RequiredTargets)
                {
                    this.CreateBuildActionEntry(doc, buildActionEntries, required, target.Project);
                }

                this.CreateBuildActionEntry(doc, buildActionEntries, target, target.Project);
            }

            var testActionEl = doc.CreateElement("TestAction");
            schemeEl.AppendChild(testActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.BuildConfigurationList.BuildConfigurations[0].Name;
                testActionEl.Attributes.Append(buildConfigurationAttr);
            }

            var launchActionEl = doc.CreateElement("LaunchAction");
            schemeEl.AppendChild(launchActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.BuildConfigurationList.BuildConfigurations[0].Name;
                launchActionEl.Attributes.Append(buildConfigurationAttr);
            }

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

        private void CreateManagementPlist(string schemeFilename)
        {
            var doc = new System.Xml.XmlDocument();

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

        private void Write(System.Xml.XmlDocument document, string path)
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

        public void Serialize()
        {
            this.Write(this.SchemeDocument, this.SchemePath);
            this.Write(this.ManagementDocument, this.ManagementPath);
        }
    }
}

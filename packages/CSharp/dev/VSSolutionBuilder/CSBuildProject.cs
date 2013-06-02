// <copyright file="CSBuildProject.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public class CSBuildProject : ICSProject
    {
        private string ProjectName = null;
        private string PathName = null;
        private System.Uri PackageUri = null;
        private System.Guid ProjectGuid;
        private System.Collections.Generic.List<string> PlatformList = new System.Collections.Generic.List<string>();
        private ProjectConfigurationCollection ProjectConfigurations = new ProjectConfigurationCollection();
        private ProjectFileCollection SourceFileCollection = new ProjectFileCollection();
        private System.Collections.Generic.List<IProject> DependentProjectList = new System.Collections.Generic.List<IProject>();
        private Opus.Core.UniqueList<string> ReferencesList = new Opus.Core.UniqueList<string>();
        private ProjectFile ApplicationDefinitionFile = null;
        private ProjectFileCollection PageFiles = new ProjectFileCollection();

        public CSBuildProject(string moduleName, string projectPathName, Opus.Core.PackageIdentifier packageId, Opus.Core.ProxyModulePath proxyPath)
        {
            this.ProjectName = moduleName;
            this.PathName = projectPathName;
            this.PackageDirectory = packageId.Path;
            if (null != proxyPath)
            {
                this.PackageDirectory = proxyPath.Combine(packageId);
            }

            bool isPackageDirAbsolute = Opus.Core.RelativePathUtilities.IsPathAbsolute(this.PackageDirectory);
            System.UriKind kind = isPackageDirAbsolute ? System.UriKind.Absolute : System.UriKind.Relative;

            if (this.PackageDirectory[this.PackageDirectory.Length - 1] == System.IO.Path.DirectorySeparatorChar)
            {
                this.PackageUri = new System.Uri(this.PackageDirectory, kind);
            }
            else
            {
                this.PackageUri = new System.Uri(this.PackageDirectory + System.IO.Path.DirectorySeparatorChar, kind);
            }

            this.ProjectGuid = new DeterministicGuid(this.PathName).Guid;
        }

        string IProject.Name
        {
            get
            {
                return this.ProjectName;
            }
        }

        string IProject.PathName
        {
            get
            {
                return this.PathName;
            }
        }

        System.Guid IProject.Guid
        {
            get
            {
                return this.ProjectGuid;
            }
        }

        public string PackageDirectory
        {
            get;
            private set;
        }

        System.Collections.Generic.List<string> IProject.Platforms
        {
            get
            {
                return this.PlatformList;
            }
        }

        ProjectConfigurationCollection IProject.Configurations
        {
            get
            {
                return this.ProjectConfigurations;
            }
        }

        ProjectFileCollection IProject.SourceFiles
        {
            get
            {
                return this.SourceFileCollection;
            }
        }

        System.Collections.Generic.List<IProject> IProject.DependentProjects
        {
            get
            {
                return this.DependentProjectList;
            }
        }

        Opus.Core.UniqueList<string> IProject.References
        {
            get
            {
                return this.ReferencesList;
            }
        }

        public string GroupName
        {
            get;
            set;
        }

        ProjectFile ICSProject.ApplicationDefinition
        {
            get
            {
                return this.ApplicationDefinitionFile;
            }

            set
            {
                this.ApplicationDefinitionFile = value;
            }
        }

        ProjectFileCollection ICSProject.Pages
        {
            get
            {
                return this.PageFiles;
            }
        }

        void IProject.Serialize()
        {
            System.Xml.XmlDocument xmlDocument = null;
            try
            {
                System.Uri projectLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                xmlDocument = new System.Xml.XmlDocument();

                xmlDocument.AppendChild(xmlDocument.CreateComment("Automatically generated by Opus v" + Opus.Core.State.OpusVersionString));

                // TODO: this needs to be from the Toolset
                string versionString = DotNetFramework.DotNet.VersionString;
                MSBuildProject project = new MSBuildProject(xmlDocument, versionString);

                MSBuildPropertyGroup generalGroup = project.CreatePropertyGroup();
                generalGroup.CreateProperty("ProjectGuid", this.ProjectGuid.ToString("B").ToUpper());
                // default configuration and platform
                {
                    MSBuildProperty defaultConfiguration = generalGroup.CreateProperty("Configuration", "Debug");
                    defaultConfiguration.Condition = " '$(Configuration)' == '' ";
                }
                {
                    MSBuildProperty defaultPlatform = generalGroup.CreateProperty("Platform", "AnyCPU");
                    defaultPlatform.Condition = " '$(Platform)' == '' ";
                }
                generalGroup.CreateProperty("TargetFrameworkVersion", "v" + versionString);

                // configurations
                foreach (ProjectConfiguration configuration in this.ProjectConfigurations)
                {
                    configuration.SerializeCSBuild(project, projectLocationUri);
                }

                // source files
                if (this.SourceFileCollection.Count > 0)
                {
                    this.SourceFileCollection.SerializeCSBuild(project, projectLocationUri, this.PackageUri);
                }

                // application definition and page files
                if ((this.ApplicationDefinitionFile != null) ||
                    (this.PageFiles.Count > 0))
                {
                    MSBuildItemGroup applicationDefinitionGroup = project.CreateItemGroup();

                    // application definition
                    if (this.ApplicationDefinitionFile != null)
                    {
                        string xamlRelativePath = Opus.Core.RelativePathUtilities.GetPath(this.ApplicationDefinitionFile.RelativePath, projectLocationUri);

                        MSBuildItem applicationDefinition = applicationDefinitionGroup.CreateItem("ApplicationDefinition", xamlRelativePath);
                        applicationDefinition.CreateMetaData("Generator", "MSBuild:Compile");
                        applicationDefinition.CreateMetaData("SubType", "Designer");

                        string sourcePathname = xamlRelativePath + ".cs";
                        MSBuildItem associatedSource = applicationDefinitionGroup.CreateItem("Compile", sourcePathname);
                        associatedSource.CreateMetaData("DependentUpon", System.IO.Path.GetFileName(xamlRelativePath));
                        associatedSource.CreateMetaData("SubType", "Code");
                    }

                    // page files
                    foreach (ProjectFile pageFile in this.PageFiles)
                    {
                        string xamlRelativePath = Opus.Core.RelativePathUtilities.GetPath(pageFile.RelativePath, projectLocationUri);

                        MSBuildItem applicationDefinition = applicationDefinitionGroup.CreateItem("Page", xamlRelativePath);
                        applicationDefinition.CreateMetaData("Generator", "MSBuild:Compile");
                        applicationDefinition.CreateMetaData("SubType", "Designer");

                        string sourcePathname = xamlRelativePath + ".cs";
                        MSBuildItem associatedSource = applicationDefinitionGroup.CreateItem("Compile", sourcePathname);
                        associatedSource.CreateMetaData("DependentUpon", System.IO.Path.GetFileName(xamlRelativePath));
                        associatedSource.CreateMetaData("SubType", "Code");
                    }
                }

                // project dependencies
                if (this.DependentProjectList.Count > 0)
                {
                    MSBuildItemGroup dependencyItemGroup = project.CreateItemGroup();
                    foreach (IProject dependentProject in this.DependentProjectList)
                    {
                        string relativePath = Opus.Core.RelativePathUtilities.GetPath(dependentProject.PathName, this.PathName);
                        MSBuildItem projectReference = dependencyItemGroup.CreateItem("ProjectReference", relativePath);
                        projectReference.CreateMetaData("Project", dependentProject.Guid.ToString("B").ToUpper());
                        projectReference.CreateMetaData("Name", dependentProject.Name);
                    }
                }

                // project references
                if (this.ReferencesList.Count > 0)
                {
                    MSBuildItemGroup referenceItemGroup = project.CreateItemGroup();
                    foreach (string reference in this.ReferencesList)
                    {
                        string noExtReference = reference;
                        if (noExtReference.EndsWith(".dll"))
                        {
                            noExtReference = reference.Remove(noExtReference.Length - 4);
                        }
                        referenceItemGroup.CreateItem("Reference", noExtReference);
                    }
                }

                // import C# project props
                project.CreateImport(@"$(MSBuildBinPath)\Microsoft.CSharp.targets");
            }
            catch (Opus.Core.Exception exception)
            {
                string message = System.String.Format("Xml construction error from project '{0}'", this.PathName);
                throw new Opus.Core.Exception(message, exception);
            }

            // write XML to disk
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.PathName));

            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = false;

            try
            {
                using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(this.PathName, xmlWriterSettings))
                {
                    xmlDocument.Save(xmlWriter);
                }
            }
            catch (Opus.Core.Exception exception)
            {
                string message = System.String.Format("Serialization error from project '{0}'", this.PathName);
                throw new Opus.Core.Exception(message, exception);
            }
        }

        VisualStudioProcessor.EVisualStudioTarget IProject.VSTarget
        {
            get
            {
                return VisualStudioProcessor.EVisualStudioTarget.MSBUILD;
            }
        }
    }
}

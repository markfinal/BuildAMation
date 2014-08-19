#region License
// Copyright 2010-2014 Mark Final
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
#endregion
namespace VSSolutionBuilder
{
    public class CSBuildProject :
        ICSProject
    {
        private string ProjectName = null;
        private string PathName = null;
        private System.Uri PackageUri = null;
        private System.Guid ProjectGuid;
        private System.Collections.Generic.List<string> PlatformList = new System.Collections.Generic.List<string>();
        private ProjectConfigurationCollection ProjectConfigurations = new ProjectConfigurationCollection();
        private ProjectFileCollection SourceFileCollection = new ProjectFileCollection();
        private System.Collections.Generic.List<IProject> DependentProjectList = new System.Collections.Generic.List<IProject>();
        private Bam.Core.UniqueList<string> ReferencesList = new Bam.Core.UniqueList<string>();
        private ProjectFile ApplicationDefinitionFile = null;
        private ProjectFileCollection PageFiles = new ProjectFileCollection();

        public
        CSBuildProject(
            string moduleName,
            string projectPathName,
            Bam.Core.PackageIdentifier packageId,
            Bam.Core.ProxyModulePath proxyPath)
        {
            this.ProjectName = moduleName;
            this.PathName = projectPathName;
            this.PackageDirectory = packageId.Location;
            if (null != proxyPath)
            {
                this.PackageDirectory = proxyPath.Combine(packageId.Location);
            }

            var packagePath = this.PackageDirectory.GetLocations()[0].AbsolutePath;

            var isPackageDirAbsolute = Bam.Core.RelativePathUtilities.IsPathAbsolute(packagePath);
            var kind = isPackageDirAbsolute ? System.UriKind.Absolute : System.UriKind.Relative;

            if (packagePath[packagePath.Length - 1] == System.IO.Path.DirectorySeparatorChar)
            {
                this.PackageUri = new System.Uri(packagePath, kind);
            }
            else
            {
                this.PackageUri = new System.Uri(packagePath + System.IO.Path.DirectorySeparatorChar, kind);
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

        public Bam.Core.DirectoryLocation PackageDirectory
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

        Bam.Core.UniqueList<string> IProject.References
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

        void
        IProject.Serialize()
        {
            System.Xml.XmlDocument xmlDocument = null;
            try
            {
                var projectLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                xmlDocument = new System.Xml.XmlDocument();

                xmlDocument.AppendChild(xmlDocument.CreateComment("Automatically generated by BuildAMation v" + Bam.Core.State.VersionString));

                // TODO: this needs to be from the Toolset
                var versionString = DotNetFramework.DotNet.VersionString;
                var project = new MSBuildProject(xmlDocument, versionString);

                var generalGroup = project.CreatePropertyGroup();
                generalGroup.CreateProperty("ProjectGuid", this.ProjectGuid.ToString("B").ToUpper());
                // default configuration and platform
                {
                    var defaultConfiguration = generalGroup.CreateProperty("Configuration", "Debug");
                    defaultConfiguration.Condition = " '$(Configuration)' == '' ";
                }
                {
                    var defaultPlatform = generalGroup.CreateProperty("Platform", "AnyCPU");
                    defaultPlatform.Condition = " '$(Platform)' == '' ";
                }
                generalGroup.CreateProperty("TargetFrameworkVersion", "v" + versionString);

                // configurations
                // TODO: convert to var
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
                    var applicationDefinitionGroup = project.CreateItemGroup();

                    // application definition
                    if (this.ApplicationDefinitionFile != null)
                    {
                        var xamlRelativePath = Bam.Core.RelativePathUtilities.GetPath(this.ApplicationDefinitionFile.RelativePath, projectLocationUri);

                        var applicationDefinition = applicationDefinitionGroup.CreateItem("ApplicationDefinition", xamlRelativePath);
                        applicationDefinition.CreateMetaData("Generator", "MSBuild:Compile");
                        applicationDefinition.CreateMetaData("SubType", "Designer");

                        var sourcePathname = xamlRelativePath + ".cs";
                        var associatedSource = applicationDefinitionGroup.CreateItem("Compile", sourcePathname);
                        associatedSource.CreateMetaData("DependentUpon", System.IO.Path.GetFileName(xamlRelativePath));
                        associatedSource.CreateMetaData("SubType", "Code");
                    }

                    // page files
                    // TODO: convert to var
                    foreach (ProjectFile pageFile in this.PageFiles)
                    {
                        var xamlRelativePath = Bam.Core.RelativePathUtilities.GetPath(pageFile.RelativePath, projectLocationUri);

                        var applicationDefinition = applicationDefinitionGroup.CreateItem("Page", xamlRelativePath);
                        applicationDefinition.CreateMetaData("Generator", "MSBuild:Compile");
                        applicationDefinition.CreateMetaData("SubType", "Designer");

                        var sourcePathname = xamlRelativePath + ".cs";
                        var associatedSource = applicationDefinitionGroup.CreateItem("Compile", sourcePathname);
                        associatedSource.CreateMetaData("DependentUpon", System.IO.Path.GetFileName(xamlRelativePath));
                        associatedSource.CreateMetaData("SubType", "Code");
                    }
                }

                // project dependencies
                if (this.DependentProjectList.Count > 0)
                {
                    var dependencyItemGroup = project.CreateItemGroup();
                    foreach (var dependentProject in this.DependentProjectList)
                    {
                        var relativePath = Bam.Core.RelativePathUtilities.GetPath(dependentProject.PathName, this.PathName);
                        var projectReference = dependencyItemGroup.CreateItem("ProjectReference", relativePath);
                        projectReference.CreateMetaData("Project", dependentProject.Guid.ToString("B").ToUpper());
                        projectReference.CreateMetaData("Name", dependentProject.Name);
                    }
                }

                // project references
                if (this.ReferencesList.Count > 0)
                {
                    var referenceItemGroup = project.CreateItemGroup();
                    foreach (var reference in this.ReferencesList)
                    {
                        var noExtReference = reference;
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
            catch (Bam.Core.Exception exception)
            {
                var message = System.String.Format("Xml construction error from project '{0}'", this.PathName);
                throw new Bam.Core.Exception(message, exception);
            }

            // write XML to disk
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.PathName));

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = false;

            try
            {
                using (var xmlWriter = System.Xml.XmlWriter.Create(this.PathName, xmlWriterSettings))
                {
                    xmlDocument.Save(xmlWriter);
                    xmlWriter.WriteWhitespace(xmlWriterSettings.NewLineChars);
                }
            }
            catch (Bam.Core.Exception exception)
            {
                var message = System.String.Format("Serialization error from project '{0}'", this.PathName);
                throw new Bam.Core.Exception(message, exception);
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

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
    public class VCXBuildProject :
        ICProject
    {
        private string ProjectName = null;
        private string PathName = null;
        private System.Uri PackageUri = null;
        private System.Guid ProjectGuid;
        private System.Collections.Generic.List<string> PlatformList = new System.Collections.Generic.List<string>();
        private ProjectConfigurationCollection ProjectConfigurations = new ProjectConfigurationCollection();
        private ProjectFileCollection SourceFileCollection = new ProjectFileCollection();
        private ProjectFileCollection HeaderFileCollection = new ProjectFileCollection();
        private ProjectFileCollection ResourceFileCollection = new ProjectFileCollection();
        private System.Collections.Generic.List<IProject> DependentProjectList = new System.Collections.Generic.List<IProject>();
        private Bam.Core.UniqueList<string> ReferencesList = new Bam.Core.UniqueList<string>();

        public
        VCXBuildProject(
            string moduleName,
            string projectPathName,
            Bam.Core.PackageIdentifier packageId,
            Bam.Core.ProxyModulePath proxyPath)
        {
            this.ProjectName = moduleName;
            this.PathName = projectPathName;
            this.PackageDirectory = packageId.Path;
            if (null != proxyPath)
            {
                this.PackageDirectory = proxyPath.Combine(packageId.Location).AbsolutePath;
            }

            var isPackageDirAbsolute = Bam.Core.RelativePathUtilities.IsPathAbsolute(this.PackageDirectory);
            var kind = isPackageDirAbsolute ? System.UriKind.Absolute : System.UriKind.Relative;

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

        ProjectFileCollection ICProject.HeaderFiles
        {
            get
            {
                return this.HeaderFileCollection;
            }
        }

        ProjectFileCollection ICProject.ResourceFiles
        {
            get
            {
                return this.ResourceFileCollection;
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

        private void
        SerializeVCXProj()
        {
            System.Xml.XmlDocument xmlDocument = null;
            try
            {
                var projectLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                xmlDocument = new System.Xml.XmlDocument();

                xmlDocument.AppendChild(xmlDocument.CreateComment("Automatically generated by BuildAMation v" + Bam.Core.State.OpusVersionString));

                var project = new MSBuildProject(xmlDocument, "4.0", "Build");

                // project globals (guid, etc)
                {
                    var globalPropertyGroup = project.CreatePropertyGroup();
                    globalPropertyGroup.Label = "Globals";
                    globalPropertyGroup.CreateProperty("ProjectGuid", this.ProjectGuid.ToString("B").ToUpper());
                }

                // import default project props
                project.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props");

                // configurations
                this.ProjectConfigurations.SerializeMSBuild(project, projectLocationUri);

                // source files
                if (this.SourceFileCollection.Count > 0)
                {
                    this.SourceFileCollection.SerializeMSBuild(project, "ClCompile", projectLocationUri, this.PackageUri);
                }
                if (this.HeaderFileCollection.Count > 0)
                {
                    this.HeaderFileCollection.SerializeMSBuild(project, "ClInclude", projectLocationUri, this.PackageUri);
                }
                if (this.ResourceFileCollection.Count > 0)
                {
                    this.ResourceFileCollection.SerializeMSBuild(project, "ResourceCompile", projectLocationUri, this.PackageUri);
                }

                // project dependencies
                // these were in the .sln file pre MSBuild
                if (this.DependentProjectList.Count > 0)
                {
                    var dependencyItemGroup = project.CreateItemGroup();
                    foreach (var dependentProject in this.DependentProjectList)
                    {
                        var relativePath = Bam.Core.RelativePathUtilities.GetPath(dependentProject.PathName, this.PathName);
                        var projectReference = dependencyItemGroup.CreateItem("ProjectReference", relativePath);
                        projectReference.CreateMetaData("Project", dependentProject.Guid.ToString("B").ToUpper());
                        projectReference.CreateMetaData("ReferenceOutputAssembly", "false");
                    }
                }

                // import targets
                project.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.targets");
            }
            catch (Bam.Core.Exception exception)
            {
                var message = System.String.Format("Xml construction error from project '{0}'", this.PathName);
                throw new Bam.Core.Exception(exception, message);
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
                }
            }
            catch (Bam.Core.Exception exception)
            {
                var message = System.String.Format("Serialization error from project '{0}'", this.PathName);
                throw new Bam.Core.Exception(exception, message);
            }
        }

        private void
        SerializeVCXProjFilters()
        {
            var filtersPath = this.PathName + ".filters";

            System.Xml.XmlDocument xmlDocument = null;
            try
            {
                var projectLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                xmlDocument = new System.Xml.XmlDocument();

                xmlDocument.AppendChild(xmlDocument.CreateComment("Automatically generated by BuildAMation v" + Bam.Core.State.OpusVersionString));

                var project = new MSBuildProject(xmlDocument, "4.0", null);

                // create new filters
                var filtersGroup = project.CreateItemGroup();
                var sourceSubDirectories = new Bam.Core.StringArray();
                var headerSubDirectories = new Bam.Core.StringArray();
                var resourceSubDirectories = new Bam.Core.StringArray();

                if ((0 == this.SourceFileCollection.Count) &&
                    (0 == this.HeaderFileCollection.Count) &&
                    (0 == this.ResourceFileCollection.Count))
                {
                    throw new Bam.Core.Exception("There are no source, header or resource files for the project '{0}'", this.ProjectName);
                }

                if (this.SourceFileCollection.Count > 0)
                {
                    {
                        var sourceFilesItem = filtersGroup.CreateItem("Filter", "Source Files");
                        // TODO: does this have to be a unique Guid?
                        sourceFilesItem.CreateMetaData("UniqueIdentifier", System.Guid.NewGuid().ToString("B").ToUpper());
                    }

                    // TODO: change to var
                    foreach (ProjectFile file in this.SourceFileCollection)
                    {
                        var subdir = System.IO.Path.GetDirectoryName(file.RelativePath);
                        var relativeSubDirFull = Bam.Core.RelativePathUtilities.GetPath(subdir, this.PackageUri);
                        var relativeSubDirs = relativeSubDirFull.Split(System.IO.Path.DirectorySeparatorChar);
                        string currentBase = null;
                        foreach (var subd in relativeSubDirs)
                        {
                            if (null != currentBase)
                            {
                                currentBase = System.IO.Path.Combine(currentBase, subd);
                            }
                            else
                            {
                                currentBase = subd;
                            }

                            if (!sourceSubDirectories.Contains(currentBase))
                            {
                                sourceSubDirectories.Add(currentBase);
                            }
                        }
                    }

                    foreach (var sourceSubDir in sourceSubDirectories)
                    {
                        var sourceFilesItem = filtersGroup.CreateItem("Filter", System.IO.Path.Combine("Source Files", sourceSubDir));
                        // TODO: does this have to be a unique Guid?
                        sourceFilesItem.CreateMetaData("UniqueIdentifier", System.Guid.NewGuid().ToString("B").ToUpper());
                    }
                }
                if (this.HeaderFileCollection.Count > 0)
                {
                    {
                        var headerFilesItem = filtersGroup.CreateItem("Filter", "Header Files");
                        // TODO: does this have to be a unique Guid?
                        headerFilesItem.CreateMetaData("UniqueIdentifier", System.Guid.NewGuid().ToString("B").ToUpper());
                    }

                    foreach (ProjectFile file in this.HeaderFileCollection)
                    {
                        var subdir = System.IO.Path.GetDirectoryName(file.RelativePath);
                        var relativeSubDirFull = Bam.Core.RelativePathUtilities.GetPath(subdir, this.PackageUri);
                        var relativeSubDirs = relativeSubDirFull.Split(System.IO.Path.DirectorySeparatorChar);
                        string currentBase = null;
                        foreach (var subd in relativeSubDirs)
                        {
                            if (null != currentBase)
                            {
                                currentBase = System.IO.Path.Combine(currentBase, subd);
                            }
                            else
                            {
                                currentBase = subd;
                            }

                            if (!headerSubDirectories.Contains(currentBase))
                            {
                                headerSubDirectories.Add(currentBase);
                            }
                        }
                    }

                    foreach (var headerSubDir in headerSubDirectories)
                    {
                        var headerFilesItem = filtersGroup.CreateItem("Filter", System.IO.Path.Combine("Header Files", headerSubDir));
                        // TODO: does this have to be a unique Guid?
                        headerFilesItem.CreateMetaData("UniqueIdentifier", System.Guid.NewGuid().ToString("B").ToUpper());
                    }
                }
                if (this.ResourceFileCollection.Count > 0)
                {
                    {
                        var resourceFilesItem = filtersGroup.CreateItem("Filter", "Resource Files");
                        // TODO: does this have to be a unique Guid?
                        resourceFilesItem.CreateMetaData("UniqueIdentifier", System.Guid.NewGuid().ToString("B").ToUpper());
                    }

                    // TODO: change to var
                    foreach (ProjectFile file in this.ResourceFileCollection)
                    {
                        var subdir = System.IO.Path.GetDirectoryName(file.RelativePath);
                        var relativeSubDirFull = Bam.Core.RelativePathUtilities.GetPath(subdir, this.PackageUri);
                        var relativeSubDirs = relativeSubDirFull.Split(System.IO.Path.DirectorySeparatorChar);
                        string currentBase = null;
                        foreach (var subd in relativeSubDirs)
                        {
                            if (null != currentBase)
                            {
                                currentBase = System.IO.Path.Combine(currentBase, subd);
                            }
                            else
                            {
                                currentBase = subd;
                            }

                            if (!resourceSubDirectories.Contains(currentBase))
                            {
                                resourceSubDirectories.Add(currentBase);
                            }
                        }
                    }

                    foreach (var resourceSubDir in resourceSubDirectories)
                    {
                        var resourceFilesItem = filtersGroup.CreateItem("Filter", System.IO.Path.Combine("Resource Files", resourceSubDir));
                        // TODO: does this have to be a unique Guid?
                        resourceFilesItem.CreateMetaData("UniqueIdentifier", System.Guid.NewGuid().ToString("B").ToUpper());
                    }
                }

                // use the filters
                if (this.SourceFileCollection.Count > 0)
                {
                    var sourceFilesGroup = project.CreateItemGroup();
                    // TODO: change to var
                    foreach (ProjectFile file in this.SourceFileCollection)
                    {
                        var subdir = System.IO.Path.GetDirectoryName(file.RelativePath);
                        var relativeSubDir = Bam.Core.RelativePathUtilities.GetPath(subdir, this.PackageUri);

                        var toolName = "ClCompile";
                        // TODO: change to var
                        foreach (ProjectFileConfiguration config in file.FileConfigurations)
                        {
                            if (config.Tool.Name == "VCCustomBuildTool")
                            {
                                toolName = "CustomBuild";
                                break;
                            }
                        }
                        var item = sourceFilesGroup.CreateItem(toolName, Bam.Core.RelativePathUtilities.GetPath(file.RelativePath, projectLocationUri));
                        item.CreateMetaData("Filter", System.IO.Path.Combine("Source Files", relativeSubDir));
                    }
                }
                if (this.HeaderFileCollection.Count > 0)
                {
                    var headerFilesGroup = project.CreateItemGroup();
                    // TODO: change to var
                    foreach (ProjectFile file in this.HeaderFileCollection)
                    {
                        var subdir = System.IO.Path.GetDirectoryName(file.RelativePath);
                        var relativeSubDir = Bam.Core.RelativePathUtilities.GetPath(subdir, this.PackageUri);

                        string elementName;
                        if ((null == file.FileConfigurations) ||
                            (0 == file.FileConfigurations.Count))
                        {
                            // no configuration - the header is just included
                            elementName = "ClInclude";
                        }
                        else
                        {
                            // a header file has a configuration setting, so must be a custom build step
                            elementName = "CustomBuild";
                        }

                        var item = headerFilesGroup.CreateItem(elementName, Bam.Core.RelativePathUtilities.GetPath(file.RelativePath, projectLocationUri));
                        item.CreateMetaData("Filter", System.IO.Path.Combine("Header Files", relativeSubDir));
                    }
                }
                if (this.ResourceFileCollection.Count > 0)
                {
                    var resourceFilesGroup = project.CreateItemGroup();
                    // TODO: change to var
                    foreach (ProjectFile file in this.ResourceFileCollection)
                    {
                        var subdir = System.IO.Path.GetDirectoryName(file.RelativePath);
                        var relativeSubDir = Bam.Core.RelativePathUtilities.GetPath(subdir, this.PackageUri);

                        var item = resourceFilesGroup.CreateItem("ResourceCompile", Bam.Core.RelativePathUtilities.GetPath(file.RelativePath, projectLocationUri));
                        item.CreateMetaData("Filter", System.IO.Path.Combine("Resource Files", relativeSubDir));
                    }
                }
            }
            catch (Bam.Core.Exception exception)
            {
                var message = System.String.Format("Xml construction error from project '{0}'", filtersPath);
                throw new Bam.Core.Exception(exception, message);
            }

            // write XML to disk
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filtersPath));

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.NewLineOnAttributes = false;

            try
            {
                using (var xmlWriter = System.Xml.XmlWriter.Create(filtersPath, xmlWriterSettings))
                {
                    xmlDocument.Save(xmlWriter);
                }
            }
            catch (Bam.Core.Exception exception)
            {
                var message = System.String.Format("Serialization error from project '{0}'", filtersPath);
                throw new Bam.Core.Exception(exception, message);
            }
        }

        void IProject.Serialize()
        {
            this.SerializeVCXProj();
            this.SerializeVCXProjFilters();
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

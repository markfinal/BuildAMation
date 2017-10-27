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
    public sealed class Project :
        Object
    {
        public Project(
            Bam.Core.Module module,
            string name)
            :
            base(null, name, "PBXProject")
        {
            this.ProjectDir = module.CreateTokenizedString("$(buildroot)/$(packagename).xcodeproj");
            module.Macros.Add("xcodeprojectdir", this.ProjectDir);

            var projectPath = module.CreateTokenizedString("$(xcodeprojectdir)/project.pbxproj");
            projectPath.Parse();
            this.ProjectPath = projectPath.ToString();

            var sourceRoot = module.CreateTokenizedString("$(packagedir)/");
            sourceRoot.Parse();
            this.SourceRoot = sourceRoot.ToString();
            var buildRoot = module.CreateTokenizedString("$(buildroot)");
            buildRoot.Parse();
            this.BuildRoot = buildRoot.ToString();

            this.Module = module;
            this.Targets = new System.Collections.Generic.Dictionary<System.Type, Target>();
            this.FileReferences = new System.Collections.Generic.List<FileReference>();
            this.BuildFiles = new System.Collections.Generic.List<BuildFile>();
            this.Groups = new System.Collections.Generic.List<Group>();
            this.GroupMap = new System.Collections.Generic.Dictionary<string, Group>();
            this.AllConfigurations = new System.Collections.Generic.List<Configuration>();
            this.ProjectConfigurations = new System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, Configuration>();
            this.ConfigurationLists = new System.Collections.Generic.List<ConfigurationList>();
            this.SourcesBuildPhases = new System.Collections.Generic.List<SourcesBuildPhase>();
            this.FrameworksBuildPhases = new System.Collections.Generic.List<FrameworksBuildPhase>();
            this.ShellScriptsBuildPhases = new Bam.Core.Array<ShellScriptBuildPhase>();
            this.CopyFilesBuildPhases = new Bam.Core.Array<CopyFilesBuildPhase>();
            this.ContainerItemProxies = new Bam.Core.Array<ContainerItemProxy>();
            this.ReferenceProxies = new Bam.Core.Array<ReferenceProxy>();
            this.TargetDependencies = new Bam.Core.Array<TargetDependency>();
            this.ProjectReferences = new System.Collections.Generic.Dictionary<Group, FileReference>();

            this.Groups.Add(new Group(this, null)); // main group
            this.Groups.Add(new Group(this, "Products")); // product ref group
            this.MainGroup.AddChild(this.ProductRefGroup);

            var configList = new ConfigurationList(this);
            this.ConfigurationLists.Add(configList);
        }

        private System.Collections.Generic.Dictionary<string, Object> ExistingGUIDs = new System.Collections.Generic.Dictionary<string, Object>();

        public void
        AddGUID(
            string guid,
            Object objectForGuid)
        {
            if (this.ExistingGUIDs.ContainsKey(guid))
            {
                // enable the log code path to view all clashes, rather than aborting on the first
#if true
                throw new Bam.Core.Exception("GUID collision {6} between\n\t{0}({1})[in {2}]\n\t{3}({4})[in {5}]",
                                             objectForGuid.Name,
                                             objectForGuid.IsA,
                                             objectForGuid.Project.Name,
                                             this.ExistingGUIDs[guid].Name,
                                             this.ExistingGUIDs[guid].IsA,
                                             this.ExistingGUIDs[guid].Project.Name,
                                             guid);
#else
                Bam.Core.Log.MessageAll("GUID collision {6} between\n\t{0}({1})[in {2}]\n\t{3}({4})[in {5}]",
                                             objectForGuid.Name,
                                             objectForGuid.IsA,
                                             objectForGuid.Project.Name,
                                             this.ExistingGUIDs[guid].Name,
                                             this.ExistingGUIDs[guid].IsA,
                                             this.ExistingGUIDs[guid].Project.Name,
                                             guid);
                return;
                #endif
            }
            this.ExistingGUIDs.Add(guid, objectForGuid);
        }

        public string SourceRoot
        {
            get;
            private set;
        }

        public string BuildRoot
        {
            get;
            private set;
        }

        public Bam.Core.TokenizedString ProjectDir
        {
            get;
            private set;
        }

        public string ProjectPath
        {
            get;
            private set;
        }

        public string BuiltProductsDir
        {
            get
            {
                return this.Module.PackageDefinition.GetBuildDirectory() + "/";
            }
        }

        public Bam.Core.Module Module
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<System.Type, Target> Targets
        {
            get;
            private set;
        }

        private System.Collections.Generic.List<FileReference> FileReferences
        {
            get;
            set;
        }

        private System.Collections.Generic.List<BuildFile> BuildFiles
        {
            get;
            set;
        }

        public System.Collections.Generic.List<Group> Groups
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, Group> GroupMap
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<Configuration> AllConfigurations
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, Configuration> ProjectConfigurations
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<ConfigurationList> ConfigurationLists
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<SourcesBuildPhase> SourcesBuildPhases
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<FrameworksBuildPhase> FrameworksBuildPhases
        {
            get;
            private set;
        }

        public Bam.Core.Array<ShellScriptBuildPhase> ShellScriptsBuildPhases
        {
            get;
            private set;
        }

        public Bam.Core.Array<CopyFilesBuildPhase> CopyFilesBuildPhases
        {
            get;
            private set;
        }

        public Bam.Core.Array<ContainerItemProxy> ContainerItemProxies
        {
            get;
            private set;
        }

        public Bam.Core.Array<ReferenceProxy> ReferenceProxies
        {
            get;
            private set;
        }

        public Bam.Core.Array<TargetDependency> TargetDependencies
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<Group, FileReference> ProjectReferences
        {
            get;
            private set;
        }

        public Group MainGroup
        {
            get
            {
                return this.Groups[0];
            }
        }

        public Group ProductRefGroup
        {
            get
            {
                return this.Groups[1];
            }
        }

        public FileReference
        EnsureFileReferenceExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            bool explicitType = true,
            FileReference.ESourceTree sourceTree = FileReference.ESourceTree.NA)
        {
            lock (this.FileReferences)
            {
                var existingFileRef = this.FileReferences.FirstOrDefault(item => item.Path.ToString().Equals(path.ToString()));
                if (null != existingFileRef)
                {
                    return existingFileRef;
                }
                var newFileRef = new FileReference(path, type, this, explicitType, sourceTree);
                this.FileReferences.Add(newFileRef);
                return newFileRef;
            }
        }

        public FileReference
        EnsureFileReferenceExists(
            Bam.Core.TokenizedString path,
            string relativePath,
            FileReference.EFileType type,
            bool explicitType = true,
            FileReference.ESourceTree sourceTree = FileReference.ESourceTree.NA)
        {
            lock (this.FileReferences)
            {
                var existingFileRef = this.FileReferences.FirstOrDefault(item => item.Path.ToString().Equals(path.ToString()));
                if (null != existingFileRef)
                {
                    return existingFileRef;
                }
                var newFileRef = new FileReference(path, type, this, explicitType, sourceTree, relativePath: relativePath);
                this.FileReferences.Add(newFileRef);
                return newFileRef;
            }
        }

        public BuildFile
        EnsureBuildFileExists(
            FileReference fileRef,
            Target target)
        {
            lock (this.BuildFiles)
            {
                var existingBuildFile = this.BuildFiles.FirstOrDefault(item => item.FileRef == fileRef && item.OwningTarget == target);
                if (null != existingBuildFile)
                {
                    return existingBuildFile;
                }
                var newBuildFile = new BuildFile(fileRef, target);
                this.BuildFiles.Add(newBuildFile);
                return newBuildFile;
            }
        }

        public void
        EnsureProjectConfigurationExists(
            Bam.Core.Module module)
        {
            lock (this.ProjectConfigurations)
            {
                var config = module.BuildEnvironment.Configuration;
                if (this.ProjectConfigurations.ContainsKey(config))
                {
                    return;
                }

                // add configuration to project
                var projectConfig = new Configuration(config, this, null);
                projectConfig["USE_HEADERMAP"] = new UniqueConfigurationValue("NO");
                projectConfig["COMBINE_HIDPI_IMAGES"] = new UniqueConfigurationValue("NO"); // TODO: needed to quieten Xcode 4 verification

                // reset SRCROOT, or it is taken to be where the workspace is
                var pkgdir = this.Module.Macros["packagedir"].ToString() + "/";
                var relativeSourcePath = Bam.Core.RelativePathUtilities.GetPath(pkgdir, this.ProjectDir.ToString());
                projectConfig["SRCROOT"] = new UniqueConfigurationValue(relativeSourcePath);

                // all 'products' are relative to SYMROOT in the IDE, regardless of the project settings
                // needed so that built products are no longer 'red' in the IDE
                var relativeSymRoot = Bam.Core.RelativePathUtilities.GetPath(this.BuiltProductsDir, this.SourceRoot);
                projectConfig["SYMROOT"] = new UniqueConfigurationValue("$(SRCROOT)/" + relativeSymRoot);

                // all intermediate files generated are relative to this
                projectConfig["OBJROOT"] = new UniqueConfigurationValue("$(SYMROOT)/intermediates");

                // would like to be able to set this to '$(SYMROOT)/$(TARGET_NAME)/$(CONFIGURATION)'
                // but TARGET_NAME is not defined in the Project configuration settings, and will end up collapsing
                // to an empty value
                // 'products' use the Project configuration value of CONFIGURATION_BUILD_DIR for their path, while
                // written target files use the Target configuration value of CONFIGURATION_BUILD_DIR
                // if these are inconsistent the IDE shows the product in red
                projectConfig["CONFIGURATION_BUILD_DIR"] = new UniqueConfigurationValue("$(SYMROOT)/$(CONFIGURATION)");

                this.ConfigurationLists[0].AddConfiguration(projectConfig);
                this.AllConfigurations.Add(projectConfig);
                this.ProjectConfigurations.Add(config, projectConfig);
            }
        }

        public Configuration
        EnsureTargetConfigurationExists(
            Bam.Core.Module module,
            Target target)
        {
            var configList = target.ConfigurationList;
            lock (configList)
            {
                var config = module.BuildEnvironment.Configuration;
                var existingConfig = configList.FirstOrDefault(item => item.Config == config);
                if (null != existingConfig)
                {
                    return existingConfig;
                }

                // if a new target config is needed, then a new project config is needed too
                this.EnsureProjectConfigurationExists(module);

                var newConfig = new Configuration(module.BuildEnvironment.Configuration, this, target);
                this.AllConfigurations.Add(newConfig);
                configList.AddConfiguration(newConfig);

                var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");

                // set which SDK to build against
                newConfig["SDKROOT"] = new UniqueConfigurationValue(clangMeta.SDK);

                // set the minimum version of OSX/iPhone to run against
                var minVersionRegEx = new System.Text.RegularExpressions.Regex("^(?<Type>[a-z]+)(?<Version>[0-9.]+)$");
                var match = minVersionRegEx.Match(clangMeta.MinimumVersionSupported);
                if (!match.Groups["Type"].Success)
                {
                    throw new Bam.Core.Exception("Unable to extract SDK type from: '{0}'", clangMeta.MinimumVersionSupported);
                }
                if (!match.Groups["Version"].Success)
                {
                    throw new Bam.Core.Exception("Unable to extract SDK version from: '{0}'", clangMeta.MinimumVersionSupported);
                }

                var optionName = System.String.Format("{0}_DEPLOYMENT_TARGET", match.Groups["Type"].Value.ToUpper());
                newConfig[optionName] = new UniqueConfigurationValue(match.Groups["Version"].Value);

                return newConfig;
            }
        }

        public void
        ResolveDeferredSetup()
        {
            // any per-configuration files excluded from the build
            foreach (var target in this.Targets.Values)
            {
                if (null == target.SourcesBuildPhase)
                {
                    continue;
                }
                foreach (var config in target.ConfigurationList)
                {
                    var diff = target.SourcesBuildPhase.BuildFiles.Complement(config.BuildFiles);
                    if (diff.Count > 0)
                    {
                        var excluded = new MultiConfigurationValue();
                        foreach (var file in diff)
                        {
                            var fullPath = file.FileRef.Path.ToString();
                            var filename = System.IO.Path.GetFileName(fullPath);
                            excluded.Add(filename);
                        }
                        config["EXCLUDED_SOURCE_FILE_NAMES"] = excluded;
                    }
                }
            }
            // any target dependencies (now that all projects have been filled out)
            foreach (var target in this.Targets.Values)
            {
                target.ResolveTargetDependencies();
            }
        }

        private void
        InternalSerialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            var indent4 = new string('\t', indentLevel + 3);
            text.AppendLine();
            text.AppendFormat("/* Begin PBXProject section */");
            text.AppendLine();

            text.AppendFormat("{0}{1} /* Project object */ = {{", indent, this.GUID);
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            text.AppendFormat("{0}attributes = {{", indent2);
            text.AppendLine();

            var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");

            text.AppendFormat("{0}LastUpgradeCheck = {1};", indent3, clangMeta.LastUpgradeCheck);
            text.AppendLine();
            text.AppendFormat("{0}}};", indent2);
            text.AppendLine();
            // project configuration list is always the first
            var projectConfigurationList = this.ConfigurationLists[0];
            text.AppendFormat("{0}buildConfigurationList = {1} /* Build configuration list for {2} \"{3}\" */;", indent2, projectConfigurationList.GUID, projectConfigurationList.Parent.IsA, projectConfigurationList.Parent.Name);
            text.AppendLine();
            text.AppendFormat("{0}compatibilityVersion = \"{1}\";", indent2, "Xcode 3.2"); // TODO
            text.AppendLine();
            text.AppendFormat("{0}mainGroup = {1};", indent2, this.MainGroup.GUID);
            text.AppendLine();
            text.AppendFormat("{0}productRefGroup = {1} /* {2} */;", indent2, this.ProductRefGroup.GUID, this.ProductRefGroup.Name);
            text.AppendLine();
            text.AppendFormat("{0}projectDirPath = \"\";", indent2);
            text.AppendLine();
            if (this.ProjectReferences.Count > 0)
            {
                text.AppendFormat("{0}projectReferences = (", indent2);
                text.AppendLine();
                foreach (var projectRef in this.ProjectReferences)
                {
                    text.AppendFormat("{0}{{", indent3);
                    text.AppendLine();
                    text.AppendFormat("{0}ProductGroup = {1} /* {2} */;", indent4, projectRef.Key.GUID, projectRef.Key.Name);
                    text.AppendLine();
                    text.AppendFormat("{0}ProjectRef = {1} /* {2} */;", indent4, projectRef.Value.GUID, projectRef.Value.Name);
                    text.AppendLine();
                    text.AppendFormat("{0}}},", indent3);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}targets = (", indent2);
            text.AppendLine();
            foreach (var target in this.Targets.Values)
            {
                text.AppendFormat("{0}{1} /* {2} */,", indent3, target.GUID, target.Name);
                text.AppendLine();
            }
            text.AppendFormat("{0});", indent2);
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();

            text.AppendFormat("/* End PBXProject section */");
            text.AppendLine();
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            if (this.BuildFiles.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXBuildFile section */");
                text.AppendLine();
                foreach (var buildFile in this.BuildFiles.OrderBy(key => key.GUID))
                {
                    buildFile.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXBuildFile section */");
                text.AppendLine();
            }
            if (this.ContainerItemProxies.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXContainerItemProxy section */");
                text.AppendLine();
                foreach (var proxy in this.ContainerItemProxies.OrderBy(key => key.GUID))
                {
                    proxy.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXContainerItemProxy section */");
                text.AppendLine();
            }
            if (this.CopyFilesBuildPhases.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXCopyFilesBuildPhase section */");
                text.AppendLine();
                foreach (var phase in this.CopyFilesBuildPhases.OrderBy(key => key.GUID))
                {
                    phase.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXCopyFilesBuildPhase section */");
                text.AppendLine();
            }
            if (this.FileReferences.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXFileReference section */");
                text.AppendLine();
                foreach (var fileRef in this.FileReferences.OrderBy(key => key.GUID))
                {
                    fileRef.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXFileReference section */");
                text.AppendLine();
            }
            if (this.FrameworksBuildPhases.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXFrameworksBuildPhase section */");
                text.AppendLine();
                foreach (var phase in this.FrameworksBuildPhases.OrderBy(key => key.GUID))
                {
                    phase.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXFrameworksBuildPhase section */");
                text.AppendLine();
            }
            if (this.Groups.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXGroup section */");
                text.AppendLine();
                foreach (var group in this.Groups.OrderBy(key => key.GUID))
                {
                    group.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXGroup section */");
                text.AppendLine();
            }
            if (this.Targets.Count > 0) // NativeTargets
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXNativeTarget section */");
                text.AppendLine();
                foreach (var target in this.Targets.Values.OrderBy(key => key.GUID))
                {
                    target.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXNativeTarget section */");
                text.AppendLine();
            }
            this.InternalSerialize(text, indentLevel); //this is the PBXProject :)
            if (this.ReferenceProxies.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXReferenceProxy section */");
                text.AppendLine();
                foreach (var proxy in this.ReferenceProxies.OrderBy(key => key.GUID))
                {
                    proxy.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXReferenceProxy section */");
                text.AppendLine();
            }
            if (this.ShellScriptsBuildPhases.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXShellScriptBuildPhase section */");
                text.AppendLine();
                foreach (var phase in this.ShellScriptsBuildPhases.OrderBy(key => key.GUID))
                {
                    phase.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXShellScriptBuildPhase section */");
                text.AppendLine();
            }
            if (this.SourcesBuildPhases.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXSourcesBuildPhase section */");
                text.AppendLine();
                foreach (var phase in this.SourcesBuildPhases.OrderBy(key => key.GUID))
                {
                    phase.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXSourcesBuildPhase section */");
                text.AppendLine();
            }
            if (this.TargetDependencies.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXTargetDependency section */");
                text.AppendLine();
                foreach (var dependency in this.TargetDependencies.OrderBy(key => key.GUID))
                {
                    dependency.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXTargetDependency section */");
                text.AppendLine();
            }
            if (this.AllConfigurations.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin XCBuildConfiguration section */");
                text.AppendLine();
                foreach (var config in this.AllConfigurations.OrderBy(key => key.GUID))
                {
                    config.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End XCBuildConfiguration section */");
                text.AppendLine();
            }
            if (this.ConfigurationLists.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin XCConfigurationList section */");
                text.AppendLine();
                foreach (var configList in this.ConfigurationLists.OrderBy(key => key.GUID))
                {
                    configList.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End XCConfigurationList section */");
                text.AppendLine();
            }
        }

        public string
        GetRelativePathToProject(
            Bam.Core.TokenizedString inputPath)
        {
            var relPath = Bam.Core.RelativePathUtilities.GetPath(inputPath.ToString(), this.ProjectDir.ToString());
            if (Bam.Core.RelativePathUtilities.IsPathAbsolute(relPath))
            {
                return null;
            }
            return relPath;
        }
    }
}

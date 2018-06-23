#region License
// Copyright (c) 2010-2018, Mark Final
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
            this.FileReferences = new Bam.Core.Array<FileReference>();
            this.BuildFiles = new Bam.Core.Array<BuildFile>();
            this.Groups = new Bam.Core.Array<Group>();
            this.GroupMap = new System.Collections.Generic.Dictionary<string, Group>();
            this.AllConfigurations = new Bam.Core.Array<Configuration>();
            this.ProjectConfigurations = new System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, Configuration>();
            this.ConfigurationLists = new Bam.Core.Array<ConfigurationList>();
            this.SourcesBuildPhases = new Bam.Core.Array<SourcesBuildPhase>();
            this.FrameworksBuildPhases = new Bam.Core.Array<FrameworksBuildPhase>();
            this.ShellScriptsBuildPhases = new Bam.Core.Array<ShellScriptBuildPhase>();
            this.CopyFilesBuildPhases = new Bam.Core.Array<CopyFilesBuildPhase>();
            this.ContainerItemProxies = new Bam.Core.Array<ContainerItemProxy>();
            this.ReferenceProxies = new Bam.Core.Array<ReferenceProxy>();
            this.TargetDependencies = new Bam.Core.Array<TargetDependency>();
            this.ProjectReferences = new System.Collections.Generic.Dictionary<Group, FileReference>();

            this.appendGroup(new Group(this, null)); // main group
            this.appendGroup(new Group(this, "Products")); // product ref group
            this.MainGroup.AddChild(this.ProductRefGroup);

            // add the project's configuration list first
            this.appendConfigurationList(new ConfigurationList(this));
        }

        private System.Collections.Generic.Dictionary<string, Object> ExistingGUIDs = new System.Collections.Generic.Dictionary<string, Object>();

        public void
        AddGUID(
            string guid,
            Object objectForGuid)
        {
            lock (this.ExistingGUIDs)
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
        }

        public string SourceRoot
        {
            get;
            private set;
        }

        private string BuildRoot
        {
            get;
            set;
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

        private string BuiltProductsDir
        {
            get
            {
                return this.Module.PackageDefinition.GetBuildDirectory() + "/";
            }
        }

        private Bam.Core.Module Module
        {
            get;
            set;
        }

        private System.Collections.Generic.Dictionary<System.Type, Target> Targets
        {
            get;
            set;
        }

        public void
        appendTarget(
            Target target)
        {
            // Note: this lock probably not required, as it's only invoked from within another lock
            lock (this.Targets)
            {
                this.Targets.Add(target.Module.GetType(), target);
            }
        }

        public System.Collections.Generic.IReadOnlyList<Target>
        getTargetList()
        {
            return this.Targets.Values.ToList();
        }

        private Bam.Core.Array<FileReference> FileReferences
        {
            get;
            set;
        }

        private Bam.Core.Array<BuildFile> BuildFiles
        {
            get;
            set;
        }

        private Bam.Core.Array<Group> Groups
        {
            get;
            set;
        }

        public void
        appendGroup(
            Group group)
        {
            lock (this.Groups)
            {
                this.Groups.Add(group);
            }
        }

        public Group
        groupWithChild(
            ReferenceProxy proxy)
        {
            return this.Groups.FirstOrDefault(item => item.Children.Contains(proxy));
        }

        private System.Collections.Generic.Dictionary<string, Group> GroupMap
        {
            get;
            set;
        }

        public void
        assignGroupToPath(
            Bam.Core.TokenizedString path,
            Group group)
        {
            lock (this.GroupMap)
            {
                this.GroupMap.Add(path.ToString(), group);
            }
        }

        public Group
        getGroupForPath(
            Bam.Core.TokenizedString path)
        {
            var match = this.GroupMap.FirstOrDefault(item => item.Key == path.ToString());
            if (match.Equals(default(System.Collections.Generic.KeyValuePair<string, Group>)))
            {
                return null;
            }
            return match.Value;
        }

        private Bam.Core.Array<Configuration> AllConfigurations
        {
            get;
            set;
        }

        public void
        appendAllConfigurations(
            Configuration config)
        {
            lock (this.AllConfigurations)
            {
                this.AllConfigurations.Add(config);
            }
        }

        private System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, Configuration> ProjectConfigurations
        {
            get;
            set;
        }

        private Bam.Core.Array<ConfigurationList> ConfigurationLists
        {
            get;
            set;
        }

        public void
        appendConfigurationList(
            ConfigurationList configList)
        {
            lock (this.ConfigurationLists)
            {
                this.ConfigurationLists.Add(configList);
            }
        }

        private ConfigurationList
        getProjectConfiguratonList()
        {
            // order is implied - this is always first
            return this.ConfigurationLists[0];
        }

        private Bam.Core.Array<SourcesBuildPhase> SourcesBuildPhases
        {
            get;
            set;
        }

        public void
        appendSourcesBuildPhase(
            SourcesBuildPhase phase)
        {
            lock (this.SourcesBuildPhases)
            {
                this.SourcesBuildPhases.Add(phase);
            }
        }

        private Bam.Core.Array<FrameworksBuildPhase> FrameworksBuildPhases
        {
            get;
            set;
        }

        public void
        appendFrameworksBuildPhase(
            FrameworksBuildPhase phase)
        {
            lock (this.FrameworksBuildPhases)
            {
                this.FrameworksBuildPhases.Add(phase);
            }
        }

        private Bam.Core.Array<ShellScriptBuildPhase> ShellScriptsBuildPhases
        {
            get;
            set;
        }

        public void
        appendShellScriptsBuildPhase(
            ShellScriptBuildPhase phase)
        {
            lock (this.ShellScriptsBuildPhases)
            {
                this.ShellScriptsBuildPhases.Add(phase);
            }
        }

        private Bam.Core.Array<CopyFilesBuildPhase> CopyFilesBuildPhases
        {
            get;
            set;
        }

        private Bam.Core.Array<ContainerItemProxy> ContainerItemProxies
        {
            get;
            set;
        }

        public void
        appendContainerItemProxy(
            ContainerItemProxy proxy)
        {
            // these are only added in a single thread
            this.ContainerItemProxies.AddUnique(proxy);
        }

        public ContainerItemProxy
        getContainerItemProxy(
            Object remote,
            Object containerPortal)
        {
            return this.ContainerItemProxies.FirstOrDefault(item =>
                (item.ContainerPortal == containerPortal) && (item.Remote == remote));
        }

        private Bam.Core.Array<ReferenceProxy> ReferenceProxies
        {
            get;
            set;
        }

        public void
        appendReferenceProxy(
            ReferenceProxy proxy)
        {
            // no lock required, added in serial code
            this.ReferenceProxies.Add(proxy);
        }

        public ReferenceProxy
        getReferenceProxyForRemoteRef(
            ContainerItemProxy remoteRef)
        {
            return this.ReferenceProxies.FirstOrDefault(item => item.RemoteRef == remoteRef);
        }

        private Bam.Core.Array<TargetDependency> TargetDependencies
        {
            get;
            set;
        }

        public void
        appendTargetDependency(
            TargetDependency dep)
        {
            // no lock required, as this is added in serial code
            this.TargetDependencies.Add(dep);
        }

        public TargetDependency
        getTargetDependency(
            Target target,
            ContainerItemProxy proxy)
        {
            return this.TargetDependencies.FirstOrDefault(item => item.Dependency == target && item.Proxy == proxy);
        }

        public TargetDependency
        getTargetDependency(
            string name,
            ContainerItemProxy proxy)
        {
            return this.TargetDependencies.FirstOrDefault(item => item.Dependency == null && item.Name == name && item.Proxy == proxy);
        }

        private System.Collections.Generic.Dictionary<Group, FileReference> ProjectReferences
        {
            get;
            set;
        }

        public void
        EnsureProjectReferenceExists(
            Group group,
            FileReference fileRef)
        {
            var existing = this.ProjectReferences.FirstOrDefault(item => item.Key == group);
            if (null == existing.Key)
            {
                // only ever executed serially, so no lock required
                this.ProjectReferences.Add(group, fileRef);
            }
        }

        public Group MainGroup
        {
            get
            {
                // order is assumed - added in the constructor
                return this.Groups[0];
            }
        }

        public Group ProductRefGroup
        {
            get
            {
                // order is assumed - added in the constructor
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
            return this.EnsureFileReferenceExists(path, null, type, explicitType, sourceTree);
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
                projectConfig["SYMROOT"] = new UniqueConfigurationValue("$(SRCROOT)/" + relativeSymRoot.TrimEnd('/'));

                // all intermediate files generated are relative to this
                projectConfig["OBJROOT"] = new UniqueConfigurationValue("$(SYMROOT)/intermediates");

                // would like to be able to set this to '$(SYMROOT)/$(TARGET_NAME)/$(CONFIGURATION)'
                // but TARGET_NAME is not defined in the Project configuration settings, and will end up collapsing
                // to an empty value
                // 'products' use the Project configuration value of CONFIGURATION_BUILD_DIR for their path, while
                // written target files use the Target configuration value of CONFIGURATION_BUILD_DIR
                // if these are inconsistent the IDE shows the product in red
                projectConfig["CONFIGURATION_BUILD_DIR"] = new UniqueConfigurationValue("$(SYMROOT)/$(CONFIGURATION)");

                this.getProjectConfiguratonList().AddConfiguration(projectConfig);
                this.appendAllConfigurations(projectConfig);
                this.ProjectConfigurations.Add(config, projectConfig);
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
                    if (!target.SourcesBuildPhase.IsValueCreated)
                    {
                        continue;
                    }
                    var diff = target.SourcesBuildPhase.Value.BuildFiles.Complement(config.BuildFiles);
                    if (diff.Any())
                    {
                        var excluded = new MultiConfigurationValue();
                        foreach (var file in diff)
                        {
                            var fullPath = file.FileRef.Path.ToString();
                            var package_build_dir = this.Module.Macros["packagebuilddir"].ToString();
                            var srcRoot = this.Module.Macros["packagedir"].ToString();
                            if (fullPath.StartsWith(package_build_dir))
                            {
                                var excluded_path = "$(SYMROOT)" + fullPath.Replace(package_build_dir, "");
                                excluded.Add(excluded_path);
                            }
                            else if (fullPath.StartsWith(srcRoot))
                            {
                                var excluded_path = "$(SRCROOT)" + fullPath.Replace(srcRoot, "");
                                excluded.Add(excluded_path);
                            }
                            else
                            {
                                var excluded_path = System.IO.Path.GetFileName(fullPath);
                                excluded.Add(excluded_path);
                            }
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
            var projectConfigurationList = this.getProjectConfiguratonList();
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
            if (this.ProjectReferences.Any())
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
            if (this.BuildFiles.Any())
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
            if (this.ContainerItemProxies.Any())
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
            if (this.CopyFilesBuildPhases.Any())
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
            if (this.FileReferences.Any())
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
            if (this.FrameworksBuildPhases.Any())
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
            if (this.Groups.Any())
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
            if (this.Targets.Any()) // NativeTargets
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
            if (this.ReferenceProxies.Any())
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
            if (this.ShellScriptsBuildPhases.Any())
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
            if (this.SourcesBuildPhases.Any())
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
            if (this.TargetDependencies.Any())
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
            if (this.AllConfigurations.Any())
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
            if (this.ConfigurationLists.Any())
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

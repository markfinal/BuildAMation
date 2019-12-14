#region License
// Copyright (c) 2010-2019, Mark Final
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
    /// <summary>
    /// Class representing a PBXProject in an Xcode project
    /// </summary>
    sealed class Project :
        Object
    {
        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="module">Module associated with the Project.</param>
        /// <param name="name">Name of the project.</param>
        public Project(
            Bam.Core.Module module,
            string name)
            :
            base(null, name, "PBXProject")
        {
            this.ProjectDir = module.CreateTokenizedString($"$(buildroot)/{name}.xcodeproj");
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

            this.AppendGroup(new Group(this, null)); // main group
            this.AppendGroup(new Group(this, "Products")); // product ref group
            this.AppendGroup(new Group(this, "Frameworks")); // all frameworks
            this.MainGroup.AddChild(this.ProductRefGroup);
            this.MainGroup.AddChild(this.Frameworks);

            // add the project's configuration list first
            this.AppendConfigurationList(new ConfigurationList(this));

            if (null == this.Module.PrimaryOutputPathKey)
            {
                throw new Bam.Core.Exception(
                    $"Cannot create Xcode project for Module {this.Module.ToString()} as it has no registered outputs"
                );
            }
        }

        private readonly System.Collections.Generic.Dictionary<string, Object> ExistingGUIDs = new System.Collections.Generic.Dictionary<string, Object>();

        /// <summary>
        /// Add a Guid to the Project.
        /// </summary>
        /// <param name="guid">GUID to add.</param>
        /// <param name="objectForGuid">Object associated with the GUID.</param>
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
                    var message = new System.Text.StringBuilder();
                    message.AppendLine($"GUID collision {guid} between");
                    message.AppendLine($"\t{objectForGuid.Name}({objectForGuid.IsA})[in {objectForGuid.Project.Name}]");
                    message.AppendLine($"\t{this.ExistingGUIDs[guid].Name}({this.ExistingGUIDs[guid].IsA})[in {this.ExistingGUIDs[guid].Project.Name}]");
#if true
                    throw new Bam.Core.Exception(message.ToString());
#else
                    Bam.Core.Log.MessageAll(message.ToString());
                    return;
#endif
                }
                this.ExistingGUIDs.Add(guid, objectForGuid);
            }
        }

        /// <summary>
        /// Get the source root of the Project
        /// </summary>
        public string SourceRoot { get; private set; }
        private string BuildRoot { get; set; }

        /// <summary>
        /// Get the project directory
        /// </summary>
        public Bam.Core.TokenizedString ProjectDir { get; private set; }

        /// <summary>
        /// Get the path to the project file
        /// </summary>
        public string ProjectPath { get; private set; }
        private Bam.Core.Module Module { get; set; }
        private System.Collections.Generic.Dictionary<System.Type, Target> Targets
        {
            get;
            set;
        }

        /// <summary>
        /// Append a Target to the Project.
        /// </summary>
        /// <param name="target">Target to append.</param>
        public void
        AppendTarget(
            Target target)
        {
            // Note: this lock probably not required, as it's only invoked from within another lock
            lock (this.Targets)
            {
                this.Targets.Add(target.Module.GetType(), target);
            }
        }

        /// <summary>
        /// Get the list of Targets
        /// </summary>
        /// <returns></returns>
        public System.Collections.Generic.IEnumerable<Target> TargetList => this.Targets.Values;

        private Bam.Core.Array<FileReference> FileReferences { get; set; }
        private Bam.Core.Array<BuildFile> BuildFiles { get; set; }
        private Bam.Core.Array<Group> Groups { get; set; }

        /// <summary>
        /// Append a Group to the Project
        /// </summary>
        /// <param name="group"></param>
        public void
        AppendGroup(
            Group group)
        {
            lock (this.Groups)
            {
                this.Groups.Add(group);
            }
        }

        /// <summary>
        /// Get the group associated with the reference proxy.
        /// </summary>
        /// <param name="proxy">Proxy that is a child of the group, or null if none is found.</param>
        /// <returns></returns>
        public Group
        GroupWithChild(
            ReferenceProxy proxy)
        {
            return this.Groups.FirstOrDefault(item => item.Children.Contains(proxy));
        }

        private System.Collections.Generic.Dictionary<string, Group> GroupMap { get; set; }

        /// <summary>
        /// Associate a Group to a path.
        /// </summary>
        /// <param name="path">TokenizedString to associated</param>
        /// <param name="group">With this group</param>
        public void
        AssignGroupToPath(
            Bam.Core.TokenizedString path,
            Group group)
        {
            lock (this.GroupMap)
            {
                this.GroupMap.Add(path.ToString(), group);
            }
        }

        /// <summary>
        /// Find the Group associated with the given path.
        /// </summary>
        /// <param name="path">Path to find a group for.</param>
        /// <returns>The Group, or null if not found.</returns>
        public Group
        GetGroupForPath(
            Bam.Core.TokenizedString path)
        {
            var match = this.GroupMap.FirstOrDefault(item => item.Key.Equals(path.ToString(), System.StringComparison.Ordinal));
            if (match.Equals(default(System.Collections.Generic.KeyValuePair<string, Group>)))
            {
                return null;
            }
            return match.Value;
        }

        private Bam.Core.Array<Configuration> AllConfigurations { get; set; }

        /// <summary>
        /// Append a Configuration to the AllConfigurations list.
        /// </summary>
        /// <param name="config">Configuration to append</param>
        public void
        AppendAllConfigurations(
            Configuration config)
        {
            lock (this.AllConfigurations)
            {
                this.AllConfigurations.Add(config);
            }
        }

        private System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, Configuration> ProjectConfigurations { get; set; }
        private Bam.Core.Array<ConfigurationList> ConfigurationLists { get; set; }

        /// <summary>
        /// Append a ConfigurationList.
        /// </summary>
        /// <param name="configList">ConfigurationList to append.</param>
        public void
        AppendConfigurationList(
            ConfigurationList configList)
        {
            lock (this.ConfigurationLists)
            {
                this.ConfigurationLists.Add(configList);
            }
        }

        private ConfigurationList
        GetProjectConfiguratonList() => this.ConfigurationLists[0]; // order is implied - this is always first

        private Bam.Core.Array<SourcesBuildPhase> SourcesBuildPhases { get; set; }

        /// <summary>
        /// Append a sources build phase to the project.
        /// </summary>
        /// <param name="phase">Phase to add.</param>
        public void
        AppendSourcesBuildPhase(
            SourcesBuildPhase phase)
        {
            lock (this.SourcesBuildPhases)
            {
                this.SourcesBuildPhases.Add(phase);
            }
        }

        private Bam.Core.Array<FrameworksBuildPhase> FrameworksBuildPhases { get; set; }

        /// <summary>
        /// Append a frameworks build phase to the project.
        /// </summary>
        /// <param name="phase">Phase to add.</param>
        public void
        AppendFrameworksBuildPhase(
            FrameworksBuildPhase phase)
        {
            lock (this.FrameworksBuildPhases)
            {
                this.FrameworksBuildPhases.Add(phase);
            }
        }

        private Bam.Core.Array<ShellScriptBuildPhase> ShellScriptsBuildPhases { get; set; }

        /// <summary>
        /// Append a shell scripts build phase to the project.
        /// </summary>
        /// <param name="phase">Phase to add.</param>
        public void
        AppendShellScriptsBuildPhase(
            ShellScriptBuildPhase phase)
        {
            lock (this.ShellScriptsBuildPhases)
            {
                this.ShellScriptsBuildPhases.Add(phase);
            }
        }

        private Bam.Core.Array<CopyFilesBuildPhase> CopyFilesBuildPhases { get; set; }
        private Bam.Core.Array<ContainerItemProxy> ContainerItemProxies { get; set; }

        /// <summary>
        /// Append a container item proxy to the project.
        /// </summary>
        /// <param name="proxy">Proxy to add.</param>
        public void
        AppendContainerItemProxy(
            ContainerItemProxy proxy) => this.ContainerItemProxies.AddUnique(proxy); // these are only added in a single thread

        /// <summary>
        /// Get the container item proxy for the corresponding traits.
        /// </summary>
        /// <param name="remote">Remote associated.</param>
        /// <param name="containerPortal">Container portal associated.</param>
        /// <returns>ContainerItemProxy found, or null</returns>
        public ContainerItemProxy
        GetContainerItemProxy(
            Object remote,
            Object containerPortal)
        {
            return this.ContainerItemProxies.FirstOrDefault(item =>
                (item.ContainerPortal == containerPortal) && (item.Remote == remote));
        }

        private Bam.Core.Array<ReferenceProxy> ReferenceProxies { get; set; }

        /// <summary>
        /// Add a reference proxy.
        /// </summary>
        /// <param name="proxy">Proxy to add.</param>
        public void
        AppendReferenceProxy(
            ReferenceProxy proxy) => this.ReferenceProxies.Add(proxy); // no lock required, added in serial code

        /// <summary>
        /// Get the ReferenceProxy for the associated ContainerItemProxy.
        /// </summary>
        /// <param name="remoteRef">ContainerItemProxy to locate.</param>
        /// <returns>ReferenceProxy, or null if not found.</returns>
        public ReferenceProxy
        GetReferenceProxyForRemoteRef(
            ContainerItemProxy remoteRef) => this.ReferenceProxies.FirstOrDefault(item => item.RemoteRef == remoteRef);

        private Bam.Core.Array<TargetDependency> TargetDependencies { get; set; }

        /// <summary>
        /// Append a target dependency to the project.
        /// </summary>
        /// <param name="dep">TargetDependency to add</param>
        public void
        AppendTargetDependency(
            TargetDependency dep) => this.TargetDependencies.Add(dep); // // no lock required, as this is added in serial code

        /// <summary>
        /// Get the TargetDependency for the Target and ContainerItemProxy
        /// </summary>
        /// <param name="target">Target container</param>
        /// <param name="proxy">Proxy associated</param>
        /// <returns>TargetDependency, or null if not found.</returns>
        public TargetDependency
        GetTargetDependency(
            Target target,
            ContainerItemProxy proxy)
        {
            return this.TargetDependencies.FirstOrDefault(item => item.Dependency == target && item.Proxy == proxy);
        }

        /// <summary>
        /// Get the TargetDependency for the name and proxy.
        /// </summary>
        /// <param name="name">Name of the target dependency.</param>
        /// <param name="proxy">Proxy associated with it.</param>
        /// <returns>TargetDependency or null if not found.</returns>
        public TargetDependency
        GetTargetDependency(
            string name,
            ContainerItemProxy proxy)
        {
            return this.TargetDependencies.FirstOrDefault(item => item.Dependency == null && item.Name.Equals(name, System.StringComparison.Ordinal) && item.Proxy == proxy);
        }

        private System.Collections.Generic.Dictionary<Group, FileReference> ProjectReferences { get; set; }

        /// <summary>
        /// Ensure that the filereference to the project exists.
        /// </summary>
        /// <param name="group">Group in which to add</param>
        /// <param name="fileRef">FileReference to the project.</param>
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

        /// <summary>
        /// Get the main group from the project.
        /// </summary>
        public Group MainGroup => this.Groups[0]; // order is assumed - added in the constructor

        /// <summary>
        /// Get the product reference from the project.
        /// </summary>
        public Group ProductRefGroup => this.Groups[1]; // order is assumed - added in the constructor

        /// <summary>
        /// Get the frameworks from the project.
        /// </summary>
        public Group Frameworks => this.Groups[2]; // order is assumed - added in the constructor

        /// <summary>
        /// Ensure that a file reference exists in the project.
        /// </summary>
        /// <param name="path">TokenizedString for the path.</param>
        /// <param name="type">Type of the file reference</param>
        /// <param name="explicitType">Does te file reference have an explicit type?</param>
        /// <param name="sourceTree">Source tree for the file reference</param>
        /// <returns></returns>
        public FileReference
        EnsureFileReferenceExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            bool explicitType = true,
            FileReference.ESourceTree sourceTree = FileReference.ESourceTree.NA)
        {
            return this.EnsureFileReferenceExists(path, null, type, explicitType, sourceTree);
        }

        /// <summary>
        /// Ensure that the file reference exists in the project.
        /// </summary>
        /// <param name="path">TokenizedString associated with the file reference</param>
        /// <param name="relativePath">Relative path to use</param>
        /// <param name="type">Type of the file reference</param>
        /// <param name="explicitType">Optional explicit type</param>
        /// <param name="sourceTree">Optional source tree</param>
        /// <returns></returns>
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

        /// <summary>
        /// Ensure that a build file exists.
        /// </summary>
        /// <param name="fileRef">FileReference associated with the BuildFile</param>
        /// <param name="target">Target on which to add the build file.</param>
        /// <returns>BuildFile from the Target</returns>
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

        /// <summary>
        /// Ensure that a Configuration exists on the Project.
        /// </summary>
        /// <param name="module">Module for the Project.</param>
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

                var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");

                // add configuration to project
                var projectConfig = new ProjectConfiguration(config, this);

                // headermaps confuse multi-configuration projects, so disable
                projectConfig["USE_HEADERMAP"] = new UniqueConfigurationValue("NO");

                // no distinction between user and system include paths
                projectConfig["ALWAYS_SEARCH_USER_PATHS"] = new UniqueConfigurationValue("YES");

                // reset SRCROOT, or it is taken to be where the workspace is
                var pkgdir = this.Module.Macros[Bam.Core.ModuleMacroNames.PackageDirectory].ToString() + "/";
                var relativeSourcePath = Bam.Core.RelativePathUtilities.GetRelativePathFromRoot(
                    System.IO.Path.GetDirectoryName(this.ProjectDir.ToString()),
                    pkgdir
                );
                projectConfig["SRCROOT"] = new UniqueConfigurationValue(relativeSourcePath);

                var isXcode10 = clangMeta.ToolchainVersion.AtLeast(ClangCommon.ToolchainVersion.Xcode_10);
                // set the SYMROOT (where built products reside for this Module's configuration)
                var builtProductsDir = module.CreateTokenizedString("@dir($(0))", module.GeneratedPaths[module.PrimaryOutputPathKey]);
                if (!builtProductsDir.IsParsed)
                {
                    builtProductsDir.Parse();
                }
                if (isXcode10)
                {
                    // an absolute path is needed, or there are mkdir errors
                    projectConfig["SYMROOT"] = new UniqueConfigurationValue(builtProductsDir.ToString());
                }
                else
                {
                    var relativeSymRoot = Bam.Core.RelativePathUtilities.GetRelativePathFromRoot(
                        Bam.Core.Graph.Instance.BuildRoot,
                        builtProductsDir.ToString()
                    );
                    projectConfig["SYMROOT"] = new UniqueConfigurationValue(relativeSymRoot);
                }

                // match the configuration build dir to where the built products reside (as BAM is handling any kind of configuration)
                projectConfig["CONFIGURATION_BUILD_DIR"] = new UniqueConfigurationValue("$(BUILD_DIR)");

                this.GetProjectConfiguratonList().AddConfiguration(projectConfig);
                this.AppendAllConfigurations(projectConfig);
                this.ProjectConfigurations.Add(config, projectConfig);
            }
        }

        /// <summary>
        /// Perform any deferred setup
        /// </summary>
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
                            var package_build_dir = this.Module.Macros[Bam.Core.ModuleMacroNames.PackageBuildDirectory].ToString();
                            var srcRoot = this.Module.Macros[Bam.Core.ModuleMacroNames.PackageDirectory].ToString();
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
                                excluded.Add(fullPath);
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
            text.AppendLine("/* Begin PBXProject section */");

            text.AppendLine($"{indent}{this.GUID} /* Project object */ = {{");
            text.AppendLine($"{indent2}isa = {this.IsA};");
            text.AppendLine($"{indent2}attributes = {{");

            try
            {
                var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
                text.AppendLine($"{indent3}LastUpgradeCheck = {clangMeta.LastUpgradeCheck};");
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                if (Bam.Core.OSUtilities.IsOSXHosting)
                {
                    throw;
                }

                // otherwise, silently ignore
            }
            text.AppendLine($"{indent2}}};");
            // project configuration list is always the first
            var projectConfigurationList = this.GetProjectConfiguratonList();
            text.AppendLine($"{indent2}buildConfigurationList = {projectConfigurationList.GUID} /* Build configuration list for {projectConfigurationList.Parent.IsA} \"{projectConfigurationList.Parent.Name}\" */;");
            text.AppendLine($"{indent2}compatibilityVersion = \"Xcode 3.2\";");
            text.AppendLine($"{indent2}mainGroup = {this.MainGroup.GUID};");
            text.AppendLine($"{indent2}productRefGroup = {this.ProductRefGroup.GUID} /* {this.ProductRefGroup.Name} */;");
            text.AppendLine($"{indent2}projectDirPath = \"\";");
            if (this.ProjectReferences.Any())
            {
                text.AppendLine($"{indent2}projectReferences = (");
                foreach (var projectRef in this.ProjectReferences)
                {
                    text.AppendLine($"{indent3}{{");
                    text.AppendLine($"{indent4}ProductGroup = {projectRef.Key.GUID} /* {projectRef.Key.Name} */;");
                    text.AppendLine($"{indent4}ProjectRef = {projectRef.Value.GUID} /* {projectRef.Value.Name} */;");
                    text.AppendLine($"{indent3}}},");
                }
                text.AppendLine($"{indent2});");
            }
            text.AppendLine($"{indent2}targets = (");
            foreach (var target in this.Targets.Values)
            {
                text.AppendLine($"{indent3}{target.GUID} /* {target.Name} */,");
            }
            text.AppendLine($"{indent2});");
            text.AppendLine($"{indent}}};");

            text.AppendLine("/* End PBXProject section */");
        }

        /// <summary>
        /// Serialize the entire projet
        /// </summary>
        /// <param name="text">StringBuilder to write to</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            if (this.BuildFiles.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXBuildFile section */");
                foreach (var buildFile in this.BuildFiles.OrderBy(key => key.GUID))
                {
                    buildFile.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXBuildFile section */");
            }
            if (this.ContainerItemProxies.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXContainerItemProxy section */");
                foreach (var proxy in this.ContainerItemProxies.OrderBy(key => key.GUID))
                {
                    proxy.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXContainerItemProxy section */");
            }
            if (this.CopyFilesBuildPhases.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXCopyFilesBuildPhase section */");
                foreach (var phase in this.CopyFilesBuildPhases.OrderBy(key => key.GUID))
                {
                    phase.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXCopyFilesBuildPhase section */");
            }
            if (this.FileReferences.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXFileReference section */");
                foreach (var fileRef in this.FileReferences.OrderBy(key => key.GUID))
                {
                    fileRef.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXFileReference section */");
            }
            if (this.FrameworksBuildPhases.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXFrameworksBuildPhase section */");
                foreach (var phase in this.FrameworksBuildPhases.OrderBy(key => key.GUID))
                {
                    phase.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXFrameworksBuildPhase section */");
            }
            if (this.Groups.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXGroup section */");
                foreach (var group in this.Groups.OrderBy(key => key.GUID))
                {
                    group.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXGroup section */");
            }
            if (this.Targets.Any()) // NativeTargets
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXNativeTarget section */");
                foreach (var target in this.Targets.Values.OrderBy(key => key.GUID))
                {
                    target.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXNativeTarget section */");
            }
            this.InternalSerialize(text, indentLevel); //this is the PBXProject :)
            if (this.ReferenceProxies.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXReferenceProxy section */");
                foreach (var proxy in this.ReferenceProxies.OrderBy(key => key.GUID))
                {
                    proxy.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXReferenceProxy section */");
            }
            if (this.ShellScriptsBuildPhases.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXShellScriptBuildPhase section */");
                foreach (var phase in this.ShellScriptsBuildPhases.OrderBy(key => key.GUID))
                {
                    phase.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXShellScriptBuildPhase section */");
            }
            if (this.SourcesBuildPhases.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXSourcesBuildPhase section */");
                foreach (var phase in this.SourcesBuildPhases.OrderBy(key => key.GUID))
                {
                    phase.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXSourcesBuildPhase section */");
            }
            if (this.TargetDependencies.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin PBXTargetDependency section */");
                foreach (var dependency in this.TargetDependencies.OrderBy(key => key.GUID))
                {
                    dependency.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End PBXTargetDependency section */");
            }
            if (this.AllConfigurations.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin XCBuildConfiguration section */");
                foreach (var config in this.AllConfigurations.OrderBy(key => key.GUID))
                {
                    config.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End XCBuildConfiguration section */");
            }
            if (this.ConfigurationLists.Any())
            {
                text.AppendLine();
                text.AppendLine("/* Begin XCConfigurationList section */");
                foreach (var configList in this.ConfigurationLists.OrderBy(key => key.GUID))
                {
                    configList.Serialize(text, indentLevel);
                }
                text.AppendLine("/* End XCConfigurationList section */");
            }
        }

        /// <summary>
        /// Convert a path to be relative to this project file.
        /// </summary>
        /// <param name="inputPath">Path to convert.</param>
        /// <returns>Relative path</returns>
        public string
        GetRelativePathToProject(
            Bam.Core.TokenizedString inputPath)
        {
            var relPath = Bam.Core.RelativePathUtilities.GetRelativePathFromRoot(
                System.IO.Path.GetDirectoryName(this.ProjectDir.ToString()),
                inputPath.ToString()
            );
            if (Bam.Core.RelativePathUtilities.IsPathAbsolute(relPath))
            {
                return null;
            }
            return relPath;
        }
    }
}

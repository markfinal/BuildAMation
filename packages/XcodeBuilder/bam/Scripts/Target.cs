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
    /// Class representing a PBXNativeTarget in an Xcode project
    /// </summary>
    sealed class Target :
        Object
    {
        /// <summary>
        /// Type of the product of the target
        /// </summary>
        public enum EProductType
        {
            NA,                 //!< Unknown
            StaticLibrary,      //!< Static library
            Executable,         //!< Executable
            DynamicLibrary,     //!< Dylib
            ApplicationBundle,  //!< Application bundle
            ObjFile,            //!< Object file
            Utility             //!< Utility
        }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="module">Module associated with</param>
        /// <param name="project">Project to add the Target to.</param>
        public Target(
            Bam.Core.Module module,
            Project project)
            :
            base(project, module.GetType().Name, "PBXNativeTarget", project.GUID)
        {
            this.Module = module;
            this.Type = EProductType.NA;

            var configList = new ConfigurationList(this);
            this.ConfigurationList = configList;
            project.AppendConfigurationList(configList);

            this.TargetDependencies = new Bam.Core.Array<TargetDependency>();
            this.ProposedTargetDependencies = new Bam.Core.Array<Target>();

            this.BuildPhases = new System.Lazy<Bam.Core.Array<BuildPhase>>(() => new Bam.Core.Array<BuildPhase>());
            this.SourcesBuildPhase = new System.Lazy<SourcesBuildPhase>(() =>
                {
                    var phase = new SourcesBuildPhase(this);
                    this.AppendBuildPhase(phase);
                    this.Project.AppendSourcesBuildPhase(phase);
                    return phase;
                });
            this.FrameworksBuildPhase = new System.Lazy<XcodeBuilder.FrameworksBuildPhase>(() =>
                {
                    var phase = new FrameworksBuildPhase(this);
                    this.Project.AppendFrameworksBuildPhase(phase);
                    this.AppendBuildPhase(phase);
                    return phase;
                });
        }

        /// <summary>
        /// Get the Module
        /// </summary>
        public Bam.Core.Module Module { get; private set; }

        // as value types cannot be used in lock statements, have a separate lock guard
        private static readonly object TypeGuard = new object();
        private EProductType Type { get; set; }

        /// <summary>
        /// Get the ConfigurationList
        /// </summary>
        public ConfigurationList ConfigurationList { get; private set; }

        private FileReference FileReference = null;

        /// <summary>
        /// Get the FileReference to the Target.
        /// </summary>
        /// <returns></returns>
        public FileReference
        GetFileReference()
        {
            return this.FileReference;
        }

        /// <summary>
        /// Get the list of TargetDependencies
        /// </summary>
        public Bam.Core.Array<TargetDependency> TargetDependencies { get; private set; }
        private Bam.Core.Array<Target> ProposedTargetDependencies { get; set; }
        private System.Lazy<Bam.Core.Array<BuildPhase>> BuildPhases { get; set; }

        private void
        AppendBuildPhase(
            BuildPhase phase)
        {
            lock (this.BuildPhases)
            {
                this.BuildPhases.Value.Add(phase);
            }
        }

        /// <summary>
        /// Get the sources build phase
        /// </summary>
        public System.Lazy<SourcesBuildPhase> SourcesBuildPhase { get; private set; }
        private System.Lazy<FrameworksBuildPhase> FrameworksBuildPhase { get; set; }

        /// <summary>
        /// Set the type of the Target
        /// </summary>
        /// <param name="type"></param>
        public void
        SetType(
            EProductType type)
        {
            lock (TypeGuard)
            {
                if (this.Type == type)
                {
                    return;
                }

                if (this.Type != EProductType.NA)
                {
                    // exception: if there is a multi-config build, and collation modules have been executed on one configuration
                    // prior to linking on another
                    if (EProductType.Executable == type && this.Type == EProductType.ApplicationBundle)
                    {
                        return;
                    }

                    throw new Bam.Core.Exception(
                        $"Product type has already been set to {this.Type.ToString()}. Cannot change it to {type.ToString()}"
                    );
                }

                this.Type = type;
            }
        }

        /// <summary>
        /// Is this a utility type Target?
        /// </summary>
        public bool IsUtilityType => this.Type == EProductType.Utility;

        /// <summary>
        /// Get the Configuration for the Target
        /// </summary>
        /// <param name="module">Associated module</param>
        /// <returns></returns>
        public Configuration
        GetConfiguration(
            Bam.Core.Module module)
        {
            lock (this.ConfigurationList)
            {
                var moduleConfig = module.BuildEnvironment.Configuration;
                var existingConfig = this.ConfigurationList.FirstOrDefault(item => item.Config == moduleConfig);
                if (null != existingConfig)
                {
                    return existingConfig;
                }
                var config = this.EnsureTargetConfigurationExists(module, this.Project);
                return config;
            }
        }

        /// <summary>
        /// Ensure that the output file exists on the Target
        /// </summary>
        /// <param name="path">TokenizedString to the output file</param>
        /// <param name="type">Type of the file reference</param>
        /// <param name="productType">Product type of the Target</param>
        public void
        EnsureOutputFileReferenceExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            Target.EProductType productType)
        {
            this.SetType(productType);
            System.Threading.LazyInitializer.EnsureInitialized(ref this.FileReference, () =>
                {
                    var fileRef = this.Project.EnsureFileReferenceExists(path, type, sourceTree: FileReference.ESourceTree.BuiltProductsDir);
                    this.Project.ProductRefGroup.AddChild(fileRef);
                    return fileRef;
                });
        }

        private BuildFile
        EnsureBuildFileExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            FileReference.ESourceTree? requestedSourceTree = null)
        {
            lock (this.Project)
            {
                var relativePath = this.Project.GetRelativePathToProject(path);
                FileReference.ESourceTree sourceTree;
                if (requestedSourceTree.HasValue)
                {
                    sourceTree = requestedSourceTree.Value;
                }
                else
                {
                    // guess
                    if (null == relativePath)
                    {
                        sourceTree = FileReference.ESourceTree.Absolute;
                    }
                    else
                    {
                        sourceTree = FileReference.ESourceTree.SourceRoot;
                    }
                }
                var fileRef = this.Project.EnsureFileReferenceExists(
                    path,
                    relativePath,
                    type,
                    sourceTree: sourceTree);
                var buildFile = this.Project.EnsureBuildFileExists(fileRef, this);
                return buildFile;
            }
        }

        private Group
        CreateGroupHierarchy(
            Bam.Core.TokenizedString path)
        {
            lock (this.Project)
            {
                var existingGroup = this.Project.GetGroupForPath(path);
                if (null != existingGroup)
                {
                    return existingGroup;
                }
                var basenameTS = this.Module.CreateTokenizedString("@basename($(0))", path);
                lock (basenameTS)
                {
                    if (!basenameTS.IsParsed)
                    {
                        basenameTS.Parse();
                    }
                }
                var basename = basenameTS.ToString();
                var group = new Group(this, basename, path);
                this.Project.AppendGroup(group);
                this.Project.AssignGroupToPath(path, group);
                if (path.ToString().Contains(System.IO.Path.DirectorySeparatorChar))
                {
                    var parent = this.Module.CreateTokenizedString("@dir($(0))", path);
                    lock (parent)
                    {
                        if (!parent.IsParsed)
                        {
                            parent.Parse();
                        }
                    }
                    var parentGroup = this.CreateGroupHierarchy(parent);
                    parentGroup.AddChild(group);
                }
                return group;
            }
        }

        private void
        AddFileRefToGroup(
            FileReference fileRef)
        {
            var relDir = this.Module.CreateTokenizedString(
                "@isrelative(@trimstart(@relativeto(@dir($(0)),$(packagedir)),../),@dir($(0)))",
                fileRef.Path
            );
            lock (relDir)
            {
                if (!relDir.IsParsed)
                {
                    relDir.Parse();
                }
            }
            if (relDir.ToString().Equals(".", System.StringComparison.Ordinal))
            {
                // this can happen if the source file is directly in the package directory
                this.Project.MainGroup.AddChild(fileRef);
            }
            else
            {
                var newGroup = this.CreateGroupHierarchy(relDir);
                var parentGroup = newGroup;
                while (parentGroup.Parent != null)
                {
                    parentGroup = parentGroup.Parent;
                    if (parentGroup == this.Project.MainGroup)
                    {
                        break;
                    }
                }
                this.Project.MainGroup.AddChild(parentGroup);
                newGroup.AddChild(fileRef);
            }
        }

        /// <summary>
        /// Ensure that a source build file exists on the Target
        /// </summary>
        /// <param name="path">TokenizedString for the build file</param>
        /// <param name="type">File type of the file reference</param>
        /// <returns>The Build File from the Target</returns>
        public BuildFile
        EnsureSourceBuildFileExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type)
        {
            lock (this.SourcesBuildPhase)
            {
                var buildFile = this.EnsureBuildFileExists(path, type);
                this.AddFileRefToGroup(buildFile.FileRef);
                this.SourcesBuildPhase.Value.AddBuildFile(buildFile);
                return buildFile;
            }
        }

        /// <summary>
        /// Ensure that the Frameworks build file exists
        /// </summary>
        /// <param name="path">TokenizedString for the frameworks build file</param>
        /// <param name="type">File type of the file reference</param>
        /// <param name="sourceTree">Source tree</param>
        /// <returns>The BuildFile created</returns>
        public BuildFile
        EnsureFrameworksBuildFileExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            FileReference.ESourceTree sourceTree)
        {
            lock (this.FrameworksBuildPhase)
            {
                var buildFile = this.EnsureBuildFileExists(path, type, requestedSourceTree: sourceTree);
                this.FrameworksBuildPhase.Value.AddBuildFile(buildFile);
                this.Project.Frameworks.AddChild(buildFile.FileRef);
                return buildFile;
            }
        }

        /// <summary>
        /// Ensure that the header file exists on the Target.
        /// </summary>
        /// <param name="path">TokenizedString for the header</param>
        public void
        EnsureHeaderFileExists(
            Bam.Core.TokenizedString path)
        {
            var relativePath = this.Project.GetRelativePathToProject(path);
            this.EnsureFileOfTypeExists(path, FileReference.EFileType.HeaderFile, relativePath: relativePath);
        }

        /// <summary>
        /// Ensure that a file of the given type exists.
        /// </summary>
        /// <param name="path">TokenizedString for the file</param>
        /// <param name="type">Type of that file</param>
        /// <param name="relativePath">Relative path to use</param>
        /// <param name="explicitType">Should this use an explicit type?</param>
        public void
        EnsureFileOfTypeExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            string relativePath = null,
            bool explicitType = true)
        {
            var sourceTree = (relativePath != null) ? FileReference.ESourceTree.SourceRoot : FileReference.ESourceTree.Absolute;
            var fileRef = this.Project.EnsureFileReferenceExists(
                path,
                relativePath,
                type,
                explicitType: explicitType,
                sourceTree: sourceTree);
            this.AddFileRefToGroup(fileRef);
        }

        /// <summary>
        /// Add a dependency on another Target
        /// </summary>
        /// <param name="other">Target that is a dependency</param>
        public void
        DependsOn(
            Target other)
        {
            if (this.Project == other.Project)
            {
                var linkedBuildFile = this.Project.EnsureBuildFileExists(other.FileReference, this);
                this.FrameworksBuildPhase.Value.AddBuildFile(linkedBuildFile);
            }
            else
            {
                var fileRefAlias = other.FileReference.MakeLinkableAlias(this.Module, this.Project);
                var linkedBuildFile = this.Project.EnsureBuildFileExists(fileRefAlias, this);
                this.FrameworksBuildPhase.Value.AddBuildFile(linkedBuildFile);
            }
        }

        /// <summary>
        /// Add an order only dependency on another Target
        /// </summary>
        /// <param name="other">Target that is a dependency</param>
        public void
        Requires(
            Target other)
        {
            lock (this.ProposedTargetDependencies)
            {
                this.ProposedTargetDependencies.AddUnique(other);
            }
        }

        /// <summary>
        /// Resolve all target dependencies
        /// </summary>
        public void
        ResolveTargetDependencies()
        {
            foreach (var depTarget in this.ProposedTargetDependencies)
            {
                if (this.Project == depTarget.Project)
                {
                    var nativeTargetItemProxy = this.Project.GetContainerItemProxy(depTarget, depTarget.Project);
                    if (null == nativeTargetItemProxy)
                    {
                        nativeTargetItemProxy = new ContainerItemProxy(this.Project, depTarget);
                    }

                    // note that target dependencies can be shared in a project by many Targets
                    // but each Target needs a reference to it
                    var dependency = this.Project.GetTargetDependency(depTarget, nativeTargetItemProxy);
                    if (null == dependency)
                    {
                        dependency = new TargetDependency(this.Project, depTarget, nativeTargetItemProxy);
                    }
                    this.TargetDependencies.AddUnique(dependency);
                }
                else
                {
                    if (null == depTarget.FileReference)
                    {
                        // expect header libraries not to have a build output
                        if (!(depTarget.Module is C.HeaderLibrary))
                        {
                            Bam.Core.Log.ErrorMessage(
                                $"Project {depTarget.Name} cannot be a target dependency as it has no output FileReference"
                            );
                        }
                        continue;
                    }

                    var relativePath = this.Project.GetRelativePathToProject(depTarget.Project.ProjectDir);
                    var sourceTree = FileReference.ESourceTree.NA;
                    if (null == relativePath)
                    {
                        sourceTree = FileReference.ESourceTree.Absolute;
                    }
                    else
                    {
                        sourceTree = FileReference.ESourceTree.Group; // note: not relative to SOURCE
                    }

                    var dependentProjectFileRef = this.Project.EnsureFileReferenceExists(
                        depTarget.Project.ProjectDir,
                        relativePath,
                        FileReference.EFileType.Project,
                        explicitType: false,
                        sourceTree: sourceTree);
                    this.Project.MainGroup.AddChild(dependentProjectFileRef);

                    // need a ContainerItemProxy for the dependent NativeTarget
                    // which is associated with a local PBXTargetDependency
                    var nativeTargetItemProxy = this.Project.GetContainerItemProxy(depTarget, dependentProjectFileRef);
                    if (null == nativeTargetItemProxy)
                    {
                        nativeTargetItemProxy = new ContainerItemProxy(this.Project, dependentProjectFileRef, depTarget);
                    }

                    // note that target dependencies can be shared in a project by many Targets
                    // but each Target needs a reference to it
                    var targetDependency = this.Project.GetTargetDependency(depTarget.Name, nativeTargetItemProxy);
                    if (null == targetDependency)
                    {
                        // no 'target', but does have the name of the dependent
                        targetDependency = new TargetDependency(this.Project, depTarget.Name, nativeTargetItemProxy);
                    }
                    this.TargetDependencies.AddUnique(targetDependency);

                    // need a ContainerItemProxy for the filereference of the dependent NativeTarget
                    // which is associated with a local PBXReferenceProxy
                    var dependentFileRefItemProxy = this.Project.GetContainerItemProxy(depTarget.FileReference, dependentProjectFileRef);
                    if (null == dependentFileRefItemProxy)
                    {
                        // note, uses the name of the Target, not the FileReference
                        dependentFileRefItemProxy = new ContainerItemProxy(this.Project, dependentProjectFileRef, depTarget.FileReference, depTarget.Name);
                    }

                    var refProxy = this.Project.GetReferenceProxyForRemoteRef(dependentFileRefItemProxy);
                    if (null == refProxy)
                    {
                        refProxy = new ReferenceProxy(
                            this.Project,
                            depTarget.FileReference.Type,
                            depTarget.FileReference.Path,
                            dependentFileRefItemProxy,
                            depTarget.FileReference.SourceTree);
                    }

                    // TODO: all PBXReferenceProxies could go into the same group
                    // but at the moment, a group is made for each
                    var productRefGroup = this.Project.GroupWithChild(refProxy);
                    if (null == productRefGroup)
                    {
                        productRefGroup = new Group(this.Project, "Products", refProxy);
                        this.Project.AppendGroup(productRefGroup);
                    }

                    this.Project.EnsureProjectReferenceExists(productRefGroup, dependentProjectFileRef);
                }
            }
        }

        /// <summary>
        /// Make this Target into an application bundle
        /// </summary>
        public void
        MakeApplicationBundle()
        {
            if (this.Type == EProductType.ApplicationBundle)
            {
                return;
            }
            if (this.Type != EProductType.Executable)
            {
                throw new Bam.Core.Exception("Can only change an executable to an application bundle");
            }
            this.Type = EProductType.ApplicationBundle;
            this.FileReference.MakeApplicationBundle();
        }

        private string
        ProductTypeToString()
        {
            switch (this.Type)
            {
            case EProductType.StaticLibrary:
                return "com.apple.product-type.library.static";

            case EProductType.Executable:
                return "com.apple.product-type.tool";

            case EProductType.DynamicLibrary:
                return "com.apple.product-type.library.dynamic";

            case EProductType.ApplicationBundle:
                return "com.apple.product-type.application";

            case EProductType.ObjFile:
                return "com.apple.product-type.objfile";

            case EProductType.Utility:
                // this is analogous to the VisualStudio 'utility' project - nothing needs building, just pre/post build steps
                return "com.apple.product-type.library.static";

            default:
                throw new Bam.Core.Exception(
                    $"Unrecognized product type, {this.Type.ToString()}, for module {this.Module.ToString()}"
                );
            }
        }

        /// <summary>
        /// Serialize the Target
        /// </summary>
        /// <param name="text">StringBuilder to write to</param>
        /// <param name="indentLevel">Number of tabs to indent by.</param>
        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendLine($"{indent}{this.GUID} /* {this.Name} */ = {{");
            text.AppendLine($"{indent2}isa = {this.IsA};");
            text.AppendLine($"{indent2}buildConfigurationList = {this.ConfigurationList.GUID} /* Build configuration list for {this.ConfigurationList.Parent.IsA} \"{this.ConfigurationList.Parent.Name}\" */;");
            text.AppendLine($"{indent2}buildPhases = (");
            foreach (var config in this.ConfigurationList)
            {
                foreach (var buildPhase in config.EnumeratePreBuildBuildPhases)
                {
                    text.AppendLine($"{indent3}{buildPhase.GUID} /* {buildPhase.Name} */,");
                }
            }
            foreach (var buildPhase in this.BuildPhases.Value)
            {
                text.AppendLine($"{indent3}{buildPhase.GUID} /* {buildPhase.Name} */,");
            }
            foreach (var config in this.ConfigurationList)
            {
                foreach (var buildPhase in config.EnumeratePostBuildBuildPhases)
                {
                    text.AppendLine($"{indent3}{buildPhase.GUID} /* {buildPhase.Name} */,");
                }
            }
            text.AppendLine($"{indent2});");
            text.AppendLine($"{indent2}buildRules = (");
            text.AppendLine($"{indent2});");
            text.AppendLine($"{indent2}dependencies = (");
            foreach (var dependency in this.TargetDependencies)
            {
                text.AppendLine($"{indent3}{dependency.GUID} /* {dependency.Name} */,");
            }
            text.AppendLine($"{indent2});");
            text.AppendLine($"{indent2}name = {this.Name};");
            text.AppendLine($"{indent2}productName = {this.Name};");
            if (null != this.FileReference)
            {
                text.AppendLine($"{indent2}productReference = {this.FileReference.GUID} /* {this.FileReference.Name} */;");
            }
            text.AppendLine($"{indent2}productType = \"{this.ProductTypeToString()}\";");
            text.AppendLine($"{indent}}};");
        }

        private Configuration
        EnsureTargetConfigurationExists(
            Bam.Core.Module module,
            Project project)
        {
            var configList = this.ConfigurationList;
            lock (configList)
            {
                var config = module.BuildEnvironment.Configuration;
                var existingConfig = configList.FirstOrDefault(item => item.Config == config);
                if (null != existingConfig)
                {
                    return existingConfig;
                }

                // if a new target config is needed, then a new project config is needed too
                project.EnsureProjectConfigurationExists(module);

                var newConfig = new TargetConfiguration(module.BuildEnvironment.Configuration, this);
                project.AppendAllConfigurations(newConfig);
                configList.AddConfiguration(newConfig);

                try
                {
                    var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");

                    // set which SDK to build against
                    newConfig["SDKROOT"] = new UniqueConfigurationValue(clangMeta.SDK);

                    // set the minimum version of macOS/iOS to run against
                    newConfig["MACOSX_DEPLOYMENT_TARGET"] = new UniqueConfigurationValue(clangMeta.MacOSXMinimumVersionSupported);

                    var isXcode10 = clangMeta.ToolchainVersion.AtLeast(ClangCommon.ToolchainVersion.Xcode_10);
                    // set the CONFIGURATION_BUILD_DIR (where built products reside for this Module's configuration)
                    var builtProductsDir = module.CreateTokenizedString("@dir($(0))", module.GeneratedPaths[module.PrimaryOutputPathKey]);
                    if (!builtProductsDir.IsParsed)
                    {
                        builtProductsDir.Parse();
                    }
                    if (isXcode10)
                    {
                        // an absolute path is needed, or there are mkdir errors
                        newConfig["CONFIGURATION_BUILD_DIR"] = new UniqueConfigurationValue(builtProductsDir.ToString());
                    }
                    else
                    {
                        var relativeSymRoot = Bam.Core.RelativePathUtilities.GetRelativePathFromRoot(
                            Bam.Core.Graph.Instance.BuildRoot,
                            builtProductsDir.ToString()
                        );
                        newConfig["CONFIGURATION_BUILD_DIR"] = new UniqueConfigurationValue(relativeSymRoot);
                    }
                }
                catch (System.Collections.Generic.KeyNotFoundException)
                {
                    if (Bam.Core.OSUtilities.IsOSXHosting)
                    {
                        // try a forced discovery, since it doesn't appear to have happened prior to now
                        // which suggests a project with no source files
                        var clangMeta = Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang");
                        (clangMeta as C.IToolchainDiscovery).Discover(C.EBit.SixtyFour); // arbitrary

                        // set which SDK to build against
                        newConfig["SDKROOT"] = new UniqueConfigurationValue(clangMeta.SDK);
                        // set the minimum version of macOS/iOS to run against
                        newConfig["MACOSX_DEPLOYMENT_TARGET"] = new UniqueConfigurationValue(clangMeta.MacOSXMinimumVersionSupported);
                    }
                    else
                    {
                        // arbitrary choice as we're not on macOS to look for valid SDK versions
                        var sdk_version = "10.13";
                        newConfig["SDKROOT"] = new UniqueConfigurationValue("macosx" + sdk_version);
                        newConfig["MACOSX_DEPLOYMENT_TARGET"] = new UniqueConfigurationValue(sdk_version);
                    }

                    // set the CONFIGURATION_BUILD_DIR (where built products reside for this Module's configuration)
                    var builtProductsDir = module.CreateTokenizedString("@dir($(0))", module.GeneratedPaths[module.PrimaryOutputPathKey]);
                    if (!builtProductsDir.IsParsed)
                    {
                        builtProductsDir.Parse();
                    }
                    // an absolute path is needed, or there are mkdir errors
                    newConfig["CONFIGURATION_BUILD_DIR"] = new UniqueConfigurationValue(builtProductsDir.ToString());
                }

                return newConfig;
            }
        }

        /// <summary>
        /// Override the name of the Target
        /// </summary>
        /// <param name="name">New name</param>
        public void
        OverrideName(
            string name)
        {
            this.Name = name;
        }
    }
}

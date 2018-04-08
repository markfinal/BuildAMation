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
    public sealed class Target :
        Object
    {
        public enum EProductType
        {
            NA,
            StaticLibrary,
            Executable,
            DynamicLibrary,
            ApplicationBundle,
            ObjFile,
            Utility
        }

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
            project.appendConfigurationList(configList);

            this.TargetDependencies = new Bam.Core.Array<TargetDependency>();
            this.ProposedTargetDependencies = new Bam.Core.Array<Target>();

            this.BuildPhases = new System.Lazy<Bam.Core.Array<BuildPhase>>(() => new Bam.Core.Array<BuildPhase>());
            this.SourcesBuildPhase = new System.Lazy<SourcesBuildPhase>(() =>
                {
                    var phase = new SourcesBuildPhase(this);
                    this.appendBuildPhase(phase);
                    this.Project.appendSourcesBuildPhase(phase);
                    return phase;
                });
            this.FrameworksBuildPhase = new System.Lazy<XcodeBuilder.FrameworksBuildPhase>(() =>
                {
                    var phase = new FrameworksBuildPhase(this);
                    this.Project.appendFrameworksBuildPhase(phase);
                    this.appendBuildPhase(phase);
                    return phase;
                });
            this.PreBuildBuildPhase = new System.Lazy<ShellScriptBuildPhase>(() =>
                {
                    return new ShellScriptBuildPhase(this, "Pre Build", (target) =>
                        {
                            var content = new System.Text.StringBuilder();
                            foreach (var config in target.ConfigurationList)
                            {
                                content.AppendFormat("if [ \\\"$CONFIGURATION\\\" = \\\"{0}\\\" ]; then\\n\\n", config.Name);
                                config.SerializePreBuildCommands(content, 1);
                                content.AppendFormat("fi\\n\\n");
                            }
                            return content.ToString();
                        });
                });
            this.PostBuildBuildPhase = new System.Lazy<ShellScriptBuildPhase>(() =>
                {
                    return new ShellScriptBuildPhase(this, "Post Build", (target) =>
                        {
                            var content = new System.Text.StringBuilder();
                            foreach (var config in target.ConfigurationList)
                            {
                                content.AppendFormat("if [ \\\"$CONFIGURATION\\\" = \\\"{0}\\\" ]; then\\n\\n", config.Name);
                                config.SerializePostBuildCommands(content, 1);
                                content.AppendFormat("fi\\n\\n");
                            }
                            return content.ToString();
                        });
                });
        }

        public Bam.Core.Module Module
        {
            get;
            private set;
        }

        // as value types cannot be used in lock statements, have a separate lock guard
        private static readonly object TypeGuard = new object();
        private EProductType Type
        {
            get;
            set;
        }

        public ConfigurationList ConfigurationList
        {
            get;
            private set;
        }

        private FileReference FileReference = null;

        public FileReference
        getFileReference()
        {
            return this.FileReference;
        }

        public Bam.Core.Array<TargetDependency> TargetDependencies
        {
            get;
            private set;
        }

        private Bam.Core.Array<Target> ProposedTargetDependencies
        {
            get;
            set;
        }

        private System.Lazy<Bam.Core.Array<BuildPhase>> BuildPhases
        {
            get;
            set;
        }

        private void
        appendBuildPhase(
            BuildPhase phase)
        {
            lock (this.BuildPhases)
            {
                this.BuildPhases.Value.Add(phase);
            }
        }

        public System.Lazy<SourcesBuildPhase> SourcesBuildPhase
        {
            get;
            private set;
        }

        private System.Lazy<FrameworksBuildPhase> FrameworksBuildPhase
        {
            get;
            set;
        }

        private System.Lazy<ShellScriptBuildPhase> PreBuildBuildPhase
        {
            get;
            set;
        }

        public System.Lazy<ShellScriptBuildPhase> PostBuildBuildPhase
        {
            get;
            set;
        }

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

                    throw new Bam.Core.Exception("Product type has already been set to {0}. Cannot change it to {1}",
                        this.Type.ToString(),
                        type.ToString());
                }

                this.Type = type;
            }
        }

        public bool
        isUtilityType
        {
            get
            {
                return this.Type == EProductType.Utility;
            }
        }

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
            FileReference.EFileType type)
        {
            lock (this.Project)
            {
                var relativePath = this.Project.GetRelativePathToProject(path);
                var sourceTree = FileReference.ESourceTree.NA;
                if (null == relativePath)
                {
                    sourceTree = FileReference.ESourceTree.Absolute;
                }
                else
                {
                    sourceTree = FileReference.ESourceTree.SourceRoot;
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
                var existingGroup = this.Project.getGroupForPath(path);
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
                this.Project.appendGroup(group);
                this.Project.assignGroupToPath(path, group);
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
            var relDir = this.Module.CreateTokenizedString("@trimstart(@relativeto(@dir($(0)),$(packagedir)),../)", fileRef.Path);
            lock (relDir)
            {
                if (!relDir.IsParsed)
                {
                    relDir.Parse();
                }
            }
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

        public BuildFile
        EnsureFrameworksBuildFileExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type)
        {
            lock (this.FrameworksBuildPhase)
            {
                var buildFile = this.EnsureBuildFileExists(path, type);
                this.FrameworksBuildPhase.Value.AddBuildFile(buildFile);
                return buildFile;
            }
        }

        public void
        EnsureHeaderFileExists(
            Bam.Core.TokenizedString path)
        {
            var relativePath = this.Project.GetRelativePathToProject(path);
            this.EnsureFileOfTypeExists(path, FileReference.EFileType.HeaderFile, relativePath: relativePath);
        }

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

        public void
        Requires(
            Target other)
        {
            lock (this.ProposedTargetDependencies)
            {
                this.ProposedTargetDependencies.AddUnique(other);
            }
        }

        public void
        ResolveTargetDependencies()
        {
            foreach (var depTarget in this.ProposedTargetDependencies)
            {
                if (this.Project == depTarget.Project)
                {
                    var nativeTargetItemProxy = this.Project.getContainerItemProxy(depTarget, depTarget.Project);
                    if (null == nativeTargetItemProxy)
                    {
                        nativeTargetItemProxy = new ContainerItemProxy(this.Project, depTarget);
                    }

                    // note that target dependencies can be shared in a project by many Targets
                    // but each Target needs a reference to it
                    var dependency = this.Project.getTargetDependency(depTarget, nativeTargetItemProxy);
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
                            Bam.Core.Log.ErrorMessage("Project {0} cannot be a target dependency as it has no output FileReference", depTarget.Name);
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
                    var nativeTargetItemProxy = this.Project.getContainerItemProxy(depTarget, dependentProjectFileRef);
                    if (null == nativeTargetItemProxy)
                    {
                        nativeTargetItemProxy = new ContainerItemProxy(this.Project, dependentProjectFileRef, depTarget);
                    }

                    // note that target dependencies can be shared in a project by many Targets
                    // but each Target needs a reference to it
                    var targetDependency = this.Project.getTargetDependency(depTarget.Name, nativeTargetItemProxy);
                    if (null == targetDependency)
                    {
                        // no 'target', but does have the name of the dependent
                        targetDependency = new TargetDependency(this.Project, depTarget.Name, nativeTargetItemProxy);
                    }
                    this.TargetDependencies.AddUnique(targetDependency);

                    // need a ContainerItemProxy for the filereference of the dependent NativeTarget
                    // which is associated with a local PBXReferenceProxy
                    var dependentFileRefItemProxy = this.Project.getContainerItemProxy(depTarget.FileReference, dependentProjectFileRef);
                    if (null == dependentFileRefItemProxy)
                    {
                        // note, uses the name of the Target, not the FileReference
                        dependentFileRefItemProxy = new ContainerItemProxy(this.Project, dependentProjectFileRef, depTarget.FileReference, depTarget.Name);
                    }

                    var refProxy = this.Project.getReferenceProxyForRemoteRef(dependentFileRefItemProxy);
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
                    var productRefGroup = this.Project.groupWithChild(refProxy);
                    if (null == productRefGroup)
                    {
                        productRefGroup = new Group(this.Project, "Products", refProxy);
                        this.Project.appendGroup(productRefGroup);
                    }

                    this.Project.EnsureProjectReferenceExists(productRefGroup, dependentProjectFileRef);
                }
            }
        }

        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands,
            Configuration configuration)
        {
            if (!this.PreBuildBuildPhase.IsValueCreated)
            {
                this.Project.appendShellScriptsBuildPhase(this.PreBuildBuildPhase.Value);
                // do not add PreBuildBuildPhase to this.BuildPhases, so that it can be serialized in the right order
            }

            configuration.appendPreBuildCommands(commands);
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands,
            Configuration configuration)
        {
            if (!this.PostBuildBuildPhase.IsValueCreated)
            {
                this.Project.appendShellScriptsBuildPhase(this.PostBuildBuildPhase.Value);
                // do not add PostBuildBuildPhase to this.BuildPhases, so that it can be serialized in the right order
            }

            configuration.appendPostBuildCommands(commands);
        }

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
                throw new Bam.Core.Exception("Unrecognized product type, {0}, for module {1}", this.Type.ToString(), this.Module.ToString());
            }
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            text.AppendFormat("{0}buildConfigurationList = {1} /* Build configuration list for {2} \"{3}\" */;", indent2, this.ConfigurationList.GUID, this.ConfigurationList.Parent.IsA, this.ConfigurationList.Parent.Name);
            text.AppendLine();
            if (this.BuildPhases.IsValueCreated ||
                this.PreBuildBuildPhase.IsValueCreated ||
                (null != this.PostBuildBuildPhase))
            {
                // make sure that pre-build phases appear first
                // then any regular build phases
                // and then post-build phases.
                // any of these can be missing

                text.AppendFormat("{0}buildPhases = (", indent2);
                text.AppendLine();
                System.Action<BuildPhase> dumpPhase = (phase) =>
                {
                    text.AppendFormat("{0}{1} /* {2} */,", indent3, phase.GUID, phase.Name);
                    text.AppendLine();
                };
                if (this.PreBuildBuildPhase.IsValueCreated)
                {
                    dumpPhase(this.PreBuildBuildPhase.Value);
                }
                if (this.BuildPhases.IsValueCreated)
                {
                    foreach (var phase in this.BuildPhases.Value)
                    {
                        dumpPhase(phase);
                    }
                }
                if (this.PostBuildBuildPhase.IsValueCreated)
                {
                    dumpPhase(this.PostBuildBuildPhase.Value);
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}buildRules = (", indent2);
            text.AppendLine();
            text.AppendFormat("{0});", indent2);
            text.AppendLine();
            text.AppendFormat("{0}dependencies = (", indent2);
            text.AppendLine();
            foreach (var dependency in this.TargetDependencies)
            {
                text.AppendFormat("{0}{1} /* {2} */,", indent3, dependency.GUID, dependency.Name);
                text.AppendLine();
            }
            text.AppendFormat("{0});", indent2);
            text.AppendLine();
            text.AppendFormat("{0}name = {1};", indent2, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}productName = {1};", indent2, this.Name);
            text.AppendLine();
            if (null != this.FileReference)
            {
                text.AppendFormat("{0}productReference = {1} /* {2} */;", indent2, this.FileReference.GUID, this.FileReference.Name);
                text.AppendLine();
            }
            text.AppendFormat("{0}productType = \"{1}\";", indent2, this.ProductTypeToString());
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
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

                var newConfig = new Configuration(module.BuildEnvironment.Configuration, project, this);
                project.appendAllConfigurations(newConfig);
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
    }
}

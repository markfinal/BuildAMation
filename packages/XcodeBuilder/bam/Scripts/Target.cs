#region License
// Copyright (c) 2010-2016, Mark Final
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
            project.ConfigurationLists.Add(configList);

            this.TargetDependencies = new Bam.Core.Array<TargetDependency>();
            this.ProposedTargetDependencies = new Bam.Core.Array<Target>();
        }

        public Bam.Core.Module Module
        {
            get;
            private set;
        }

        public EProductType Type
        {
            get;
            set;
        }

        public ConfigurationList ConfigurationList
        {
            get;
            private set;
        }

        public FileReference FileReference
        {
            get;
            private set;
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

        public Bam.Core.Array<BuildPhase> BuildPhases
        {
            get;
            private set;
        }

        public SourcesBuildPhase SourcesBuildPhase
        {
            get;
            private set;
        }

        public FrameworksBuildPhase FrameworksBuildPhase
        {
            get;
            set;
        }

        public ShellScriptBuildPhase PreBuildBuildPhase
        {
            get;
            private set;
        }

        public ShellScriptBuildPhase PostBuildBuildPhase
        {
            get;
            private set;
        }

        private void
        SetProductType(
            EProductType type)
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

        public Configuration
        GetConfiguration(
            Bam.Core.Module module)
        {
            lock (this.ConfigurationList)
            {
                var moduleConfig = module.BuildEnvironment.Configuration;
                var existingConfig = this.ConfigurationList.Where(item => item.Config == moduleConfig).FirstOrDefault();
                if (null != existingConfig)
                {
                    return existingConfig;
                }
                var config = this.Project.EnsureTargetConfigurationExists(module, this);
                return config;
            }
        }

        public void
        EnsureOutputFileReferenceExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            Target.EProductType productType)
        {
            lock (this)
            {
                this.SetProductType(productType);

                var fileRef = this.Project.EnsureFileReferenceExists(path, type, sourceTree: FileReference.ESourceTree.BuiltProductsDir);
                if (null == this.FileReference)
                {
                    this.FileReference = fileRef;
                    this.Project.ProductRefGroup.AddChild(fileRef);
                }
            }
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
                var found = this.Project.GroupMap.Where(item => item.Key == path.Parse()).FirstOrDefault();
                if (!found.Equals(default(System.Collections.Generic.KeyValuePair<string, Group>)))
                {
                    return found.Value;
                }
                var basename = this.Module.CreateTokenizedString("@basename($(0))", path).Parse();
                var group = new Group(this, basename, path);
                this.Project.Groups.Add(group);
                this.Project.GroupMap.Add(path.Parse(), group);
                if (path.Parse().Contains(System.IO.Path.DirectorySeparatorChar))
                {
                    var parent = this.Module.CreateTokenizedString("@dir($(0))", path);
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
            lock (this)
            {
                if (null == this.SourcesBuildPhase)
                {
                    this.SourcesBuildPhase = new SourcesBuildPhase(this);
                    if (null == this.BuildPhases)
                    {
                        this.BuildPhases = new Bam.Core.Array<BuildPhase>();
                    }
                    this.BuildPhases.Add(this.SourcesBuildPhase);
                    this.Project.SourcesBuildPhases.Add(this.SourcesBuildPhase);
                }

                var buildFile = this.EnsureBuildFileExists(path, type);
                this.AddFileRefToGroup(buildFile.FileRef);
                this.SourcesBuildPhase.AddBuildFile(buildFile);
                return buildFile;
            }
        }

        private void
        EnsureFrameworksBuildPhaseExists()
        {
            lock (this)
            {
                if (null != this.FrameworksBuildPhase)
                {
                    return;
                }
                var frameworks = new FrameworksBuildPhase(this);
                this.Project.FrameworksBuildPhases.Add(frameworks);
                this.BuildPhases.Add(frameworks);
                this.FrameworksBuildPhase = frameworks;
            }
        }

        public BuildFile
        EnsureFrameworksBuildFileExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type)
        {
            lock (this)
            {
                this.EnsureFrameworksBuildPhaseExists();

                var buildFile = this.EnsureBuildFileExists(path, type);
                this.FrameworksBuildPhase.AddBuildFile(buildFile);
                return buildFile;
            }
        }

        public void
        EnsureHeaderFileExists(
            Bam.Core.TokenizedString path)
        {
            lock (this)
            {
                var relativePath = this.Project.GetRelativePathToProject(path);
                this.EnsureFileOfTypeExists(path, FileReference.EFileType.HeaderFile, relativePath: relativePath);
            }
        }

        public void
        EnsureFileOfTypeExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            string relativePath = null)
        {
            lock (this)
            {
                var sourceTree = (relativePath != null) ? FileReference.ESourceTree.SourceRoot : FileReference.ESourceTree.Absolute;
                var fileRef = this.Project.EnsureFileReferenceExists(
                    path,
                    relativePath,
                    type,
                    sourceTree: sourceTree);
                this.AddFileRefToGroup(fileRef);
            }
        }

        public void
        DependsOn(
            Target other)
        {
            lock (this)
            {
                this.EnsureFrameworksBuildPhaseExists();
                if (this.Project == other.Project)
                {
                    var linkedBuildFile = this.Project.EnsureBuildFileExists(other.FileReference, this);
                    this.FrameworksBuildPhase.AddBuildFile(linkedBuildFile);
                }
                else
                {
                    var fileRefAlias = other.FileReference.MakeLinkableAlias(this.Module, this.Project);
                    var linkedBuildFile = this.Project.EnsureBuildFileExists(fileRefAlias, this);
                    this.FrameworksBuildPhase.AddBuildFile(linkedBuildFile);
                }
            }
        }

        public void
        Requires(
            Target other)
        {
            lock (this)
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
                    var nativeTargetItemProxy = this.Project.ContainerItemProxies.FirstOrDefault(
                        item => (item.ContainerPortal == this.Project) && (item.Remote == depTarget));
                    if (null == nativeTargetItemProxy)
                    {
                        nativeTargetItemProxy = new ContainerItemProxy(this.Project, depTarget);
                    }

                    // note that target dependencies can be shared in a project by many Targets
                    // but each Target needs a reference to it
                    var dependency = this.Project.TargetDependencies.FirstOrDefault(
                        item => (item.Dependency == depTarget) && (item.Proxy == nativeTargetItemProxy));
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
                        Bam.Core.Log.DebugMessage("Project {0} cannot be a target dependency as it has no output FileReference", depTarget.Name);
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
                    var nativeTargetItemProxy = this.Project.ContainerItemProxies.FirstOrDefault(
                        item => (item.ContainerPortal == dependentProjectFileRef) && (item.Remote == depTarget));
                    if (null == nativeTargetItemProxy)
                    {
                        nativeTargetItemProxy = new ContainerItemProxy(this.Project, dependentProjectFileRef, depTarget);
                    }

                    // note that target dependencies can be shared in a project by many Targets
                    // but each Target needs a reference to it
                    var targetDependency = this.Project.TargetDependencies.FirstOrDefault(
                        item => (item.Dependency == null) && (item.Name == depTarget.Name) && (item.Proxy == nativeTargetItemProxy));
                    if (null == targetDependency)
                    {
                        // no 'target', but does have the name of the dependent
                        targetDependency = new TargetDependency(this.Project, depTarget.Name, nativeTargetItemProxy);
                    }
                    this.TargetDependencies.AddUnique(targetDependency);

                    // need a ContainerItemProxy for the filereference of the dependent NativeTarget
                    // which is associated with a local PBXReferenceProxy
                    var dependentFileRefItemProxy = this.Project.ContainerItemProxies.FirstOrDefault(
                        item => (item.ContainerPortal == dependentProjectFileRef) && (item.Remote == depTarget.FileReference));
                    if (null == dependentFileRefItemProxy)
                    {
                        // note, uses the name of the Target, not the FileReference
                        dependentFileRefItemProxy = new ContainerItemProxy(this.Project, dependentProjectFileRef, depTarget.FileReference, depTarget.Name);
                    }

                    var refProxy = this.Project.ReferenceProxies.FirstOrDefault(
                        item => item.RemoteRef == dependentFileRefItemProxy);
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
                    var productRefGroup = this.Project.Groups.FirstOrDefault(
                        item => item.Children.Contains(refProxy));
                    if (null == productRefGroup)
                    {
                        productRefGroup = new Group(this.Project, "Products", refProxy);
                        this.Project.Groups.Add(productRefGroup);
                    }

                    var productRef = this.Project.ProjectReferences.FirstOrDefault(
                        item => item.Key == productRefGroup);
                    if (null == productRef.Key)
                    {
                        this.Project.ProjectReferences.Add(productRefGroup, dependentProjectFileRef);
                    }
                }
            }
        }

        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands,
            Configuration configuration)
        {
            lock (this)
            {
                if (null == this.PreBuildBuildPhase)
                {
                    var preBuildBuildPhase = new ShellScriptBuildPhase(this, "Pre Build", (target) =>
                    {
                        var content = new System.Text.StringBuilder();
                        foreach (var config in target.ConfigurationList)
                        {
                            content.AppendFormat("if [ \\\"$CONFIGURATION\\\" = \\\"{0}\\\" ]; then\\n\\n", config.Name);
                            foreach (var line in config.PreBuildCommands)
                            {
                                content.AppendFormat("  {0}\\n", line.Replace("\"", "\\\""));
                            }
                            content.AppendFormat("fi\\n\\n");
                        }
                        return content.ToString();
                    });
                    this.Project.ShellScriptsBuildPhases.Add(preBuildBuildPhase);
                    this.PreBuildBuildPhase = preBuildBuildPhase;
                    // do not add PreBuildBuildPhase to this.BuildPhases, so that it can be serialized in the right order
                }

                configuration.PreBuildCommands.AddRange(commands);
            }
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands,
            Configuration configuration)
        {
            lock (this)
            {
                if (null == this.PostBuildBuildPhase)
                {
                    var postBuildBuildPhase = new ShellScriptBuildPhase(this, "Post Build", (target) =>
                    {
                        var content = new System.Text.StringBuilder();
                        foreach (var config in target.ConfigurationList)
                        {
                            content.AppendFormat("if [ \\\"$CONFIGURATION\\\" = \\\"{0}\\\" ]; then\\n\\n", config.Name);
                            foreach (var line in config.PostBuildCommands)
                            {
                                content.AppendFormat("  {0}\\n", line.Replace("\"", "\\\""));
                            }
                            content.AppendFormat("fi\\n\\n");
                        }
                        return content.ToString();
                    });
                    this.Project.ShellScriptsBuildPhases.Add(postBuildBuildPhase);
                    this.PostBuildBuildPhase = postBuildBuildPhase;
                    // do not add PostBuildBuildPhase to this.BuildPhases, so that it can be serialized in the right order
                }

                configuration.PostBuildCommands.AddRange(commands);
            }
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
            if (((null != this.BuildPhases) && (this.BuildPhases.Count > 0)) ||
                (null != this.PreBuildBuildPhase) ||
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
                if (null != this.PreBuildBuildPhase)
                {
                    dumpPhase(this.PreBuildBuildPhase);
                }
                if (null != this.BuildPhases)
                {
                    foreach (var phase in this.BuildPhases)
                    {
                        dumpPhase(phase);
                    }
                }
                if (null != this.PostBuildBuildPhase)
                {
                    dumpPhase(this.PostBuildBuildPhase);
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
    }
}

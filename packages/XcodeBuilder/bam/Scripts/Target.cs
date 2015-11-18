#region License
// Copyright (c) 2010-2015, Mark Final
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
            ObjFile
        }

        public Target(
            Bam.Core.Module module,
            Project project)
        {
            this.IsA = "PBXNativeTarget";
            this.Name = module.GetType().Name;
            this.Module = module;
            this.Project = project;
            this.Type = EProductType.NA;

            var configList = new ConfigurationList(this);
            this.ConfigurationList = configList;
            project.ConfigurationLists.Add(configList);

            this.TargetDependencies = new Bam.Core.Array<TargetDependency>();
        }

        private Bam.Core.Module Module
        {
            get;
            set;
        }

        public Project Project
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
            var moduleConfig = module.BuildEnvironment.Configuration;
            var existingConfig = this.ConfigurationList.Where(item => item.Config == moduleConfig).FirstOrDefault();
            if (null != existingConfig)
            {
                return existingConfig;
            }
            var config = this.Project.EnsureTargetConfigurationExists(module, this.ConfigurationList);
            return config;
        }

        public void
        EnsureOutputFileReferenceExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            Target.EProductType productType)
        {
            this.SetProductType(productType);

            var fileRef = this.Project.EnsureFileReferenceExists(path, type, sourceTree: FileReference.ESourceTree.BuiltProductsDir);
            if (null == this.FileReference)
            {
                this.FileReference = fileRef;
                this.Project.ProductRefGroup.AddChild(fileRef);
            }
        }

        private BuildFile
        EnsureBuildFileExists(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type)
        {
            var fileRef = this.Project.EnsureFileReferenceExists(
                path,
                type,
                sourceTree: FileReference.ESourceTree.Absolute);
            var buildFile = this.Project.EnsureBuildFileExists(fileRef);
            return buildFile;
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
                    this.SourcesBuildPhase = new SourcesBuildPhase();
                    if (null == this.BuildPhases)
                    {
                        this.BuildPhases = new Bam.Core.Array<BuildPhase>();
                    }
                    this.BuildPhases.Add(this.SourcesBuildPhase);
                    this.Project.SourcesBuildPhases.Add(this.SourcesBuildPhase);
                }

                var buildFile = this.EnsureBuildFileExists(path, type);
                this.Project.SourceFilesGroup.AddChild(buildFile.FileRef);
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
                var frameworks = new FrameworksBuildPhase();
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
                var fileRef = this.Project.EnsureFileReferenceExists(
                    path,
                    FileReference.EFileType.HeaderFile,
                    sourceTree: FileReference.ESourceTree.Absolute);
                this.Project.HeaderFilesGroup.AddChild(fileRef);
            }
        }

        public void
        DependsOn(
            Target other)
        {
            this.EnsureFrameworksBuildPhaseExists();
            if (this.Project == other.Project)
            {
                var linkedBuildFile = this.Project.EnsureBuildFileExists(other.FileReference);
                this.FrameworksBuildPhase.AddBuildFile(linkedBuildFile);
            }
            else
            {
                var fileRefAlias = other.FileReference.MakeLinkableAlias(this.Module, this.Project);
                var linkedBuildFile = this.Project.EnsureBuildFileExists(fileRefAlias);
                this.FrameworksBuildPhase.AddBuildFile(linkedBuildFile);
            }
        }

        public void
        Requires(
            Target other)
        {
            var existingTargetDep = this.TargetDependencies.Where(item => item.Dependency == other).FirstOrDefault();
            if (null != existingTargetDep)
            {
                return;
            }
            if (this.Project == other.Project)
            {
                var itemProxy = this.Project.ContainerItemProxies.Where(item => (item.ContainerPortal == this.Project) && (item.Remote == other)).FirstOrDefault();
                if (null == itemProxy)
                {
                    itemProxy = new ContainerItemProxy(this.Project, this.Project, other, false);
                }

                var dependency = this.TargetDependencies.Where(item => (item.Dependency == other) && (item.Proxy == itemProxy)).FirstOrDefault();
                if (null == dependency)
                {
                    dependency = new TargetDependency(this.Project, other, itemProxy);
                    this.TargetDependencies.AddUnique(dependency);
                }
            }
            else
            {
                var fileRef = this.Project.EnsureFileReferenceExists(
                    other.Project.ProjectDir,
                    FileReference.EFileType.Project,
                    explicitType: false,
                    sourceTree: FileReference.ESourceTree.Absolute);
                this.Project.MainGroup.AddChild(fileRef);

                var itemProxy = this.Project.ContainerItemProxies.Where(item => (item.ContainerPortal == fileRef) && (item.Remote == other)).FirstOrDefault();
                if (null == itemProxy)
                {
                    itemProxy = new ContainerItemProxy(this.Project, fileRef, other, false);
                }

                var refProxy = this.Project.ReferenceProxies.Where(item => item.RemoteRef == itemProxy).FirstOrDefault();
                if (null == refProxy)
                {
                    refProxy = new ReferenceProxy(
                        this.Project,
                        other.FileReference.Type,
                        other.FileReference.Path,
                        itemProxy,
                        other.FileReference.SourceTree);
                }

                var productRefGroup = this.Project.Groups.Where(item => item.Children.Contains(refProxy)).FirstOrDefault();
                if (null == productRefGroup)
                {
                    productRefGroup = new Group("Products");
                    productRefGroup.AddChild(refProxy);
                    this.Project.Groups.Add(productRefGroup);
                }

                var productRef = this.Project.ProjectReferences.Where(item => item.Key == productRefGroup).FirstOrDefault();
                if (productRef.Equals(default(System.Collections.Generic.Dictionary<Group, FileReference>)))
                {
                    this.Project.ProjectReferences.Add(productRefGroup, fileRef);
                }

                var dependency = this.TargetDependencies.Where(item => (item.Dependency == null) && (item.Proxy == itemProxy)).FirstOrDefault();
                if (null == dependency)
                {
                    dependency = new TargetDependency(this.Project, null, itemProxy);
                    this.TargetDependencies.AddUnique(dependency);
                }
            }
        }

        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands,
            Configuration configuration)
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
                            content.AppendFormat("  {0}\\n", line);
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

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands,
            Configuration configuration)
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
                            content.AppendFormat("  {0}\\n", line);
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
            if ((null != this.BuildPhases) && (this.BuildPhases.Count > 0))
            {
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
                foreach (var phase in this.BuildPhases)
                {
                    dumpPhase(phase);
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

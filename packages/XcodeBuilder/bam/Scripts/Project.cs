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
    public sealed class Project :
        Object
    {
        public Project(
            Bam.Core.Module module,
            string name)
        {
            this.IsA = "PBXProject";
            this.Name = name;
            var projectDir = module.CreateTokenizedString("$(buildroot)/$(packagename).xcodeproj");
            module.Macros.Add("xcodeprojectdir", projectDir);
            this.ProjectDir = projectDir.Parse();

            var projectPath = module.CreateTokenizedString("$(xcodeprojectdir)/project.pbxproj");
            this.ProjectPath = projectPath.Parse();

            this.SourceRoot = module.CreateTokenizedString("$(packagedir)").Parse();
            this.BuildRoot = module.CreateTokenizedString("$(buildroot)").Parse();

            this.Module = module;
            this.Targets = new System.Collections.Generic.Dictionary<System.Type, Target>();
            this.FileReferences = new System.Collections.Generic.List<FileReference>();
            this.BuildFiles = new System.Collections.Generic.List<BuildFile>();
            this.Groups = new System.Collections.Generic.List<Group>();
            this.AllConfigurations = new System.Collections.Generic.List<Configuration>();
            this.ProjectConfigurations = new System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, Configuration>();
            this.ConfigurationLists = new System.Collections.Generic.List<ConfigurationList>();
            this.SourcesBuildPhases = new System.Collections.Generic.List<SourcesBuildPhase>();
            this.FrameworksBuildPhases = new System.Collections.Generic.List<FrameworksBuildPhase>();
            this.ShellScriptsBuildPhases = new Bam.Core.Array<ShellScriptBuildPhase>();
            this.CopyFilesBuildPhases = new Bam.Core.Array<CopyFilesBuildPhase>();
            this.ContainerItemProxies = new Bam.Core.Array<ContainerItemProxy>();
            this.TargetDependencies = new Bam.Core.Array<TargetDependency>();

            this.Groups.Add(new Group()); // main group
            this.Groups.Add(new Group("Products")); // product ref group
            this.Groups.Add(new Group("Source Files"));
            this.Groups.Add(new Group("Header Files"));

            this.MainGroup.AddReference(this.ProductRefGroup);
            this.MainGroup.AddReference(this.SourceFilesGroup);
            this.MainGroup.AddReference(this.HeaderFilesGroup);

            var configList = new ConfigurationList(this);
            this.ConfigurationLists.Add(configList);

            this.AddNewProjectConfiguration(module);
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

        public string ProjectDir
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
                return this.Module.PackageDefinition.GetBuildDirectory();
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

        public Bam.Core.Array<TargetDependency> TargetDependencies
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

        public Group SourceFilesGroup
        {
            get
            {
                return this.Groups[2];
            }
        }

        public Group HeaderFilesGroup
        {
            get
            {
                return this.Groups[3];
            }
        }

        public FileReference
        FindOrCreateFileReference(
            Bam.Core.TokenizedString path,
            FileReference.EFileType type,
            bool explicitType = false,
            FileReference.ESourceTree sourceTree = FileReference.ESourceTree.NA)
        {
            foreach (var fileRef in this.FileReferences)
            {
                if (fileRef.Path.Equals(path))
                {
                    return fileRef;
                }
            }

            var newFileRef = new FileReference(path, type, this, explicitType, sourceTree);
            this.FileReferences.Add(newFileRef);
            return newFileRef;
        }

        public FileReference
        FindOrCreateFileReference(
            FileReference other)
        {
            foreach (var fileRef in this.FileReferences)
            {
                if (null == fileRef.LinkedTo)
                {
                    continue;
                }
                if (fileRef.LinkedTo.GUID == other.GUID)
                {
                    return fileRef;
                }
            }

            var newFileRef = new FileReference(other, this);
            this.FileReferences.Add(newFileRef);
            return newFileRef;
        }

        public BuildFile
        FindOrCreateBuildFile(
            Bam.Core.TokenizedString path,
            FileReference fileRef)
        {
            foreach (var buildFile in this.BuildFiles)
            {
                if (buildFile.Source == fileRef)
                {
                    return buildFile;
                }
            }

            var newBuildFile = new BuildFile(path, fileRef);
            this.BuildFiles.Add(newBuildFile);
            return newBuildFile;
        }

        public Target
        FindOrCreateTarget(
            Bam.Core.Module module,
            FileReference fileRef,
            Target.EProductType type)
        {
            foreach (var target in this.Targets)
            {
                if (target.Key == module.GetType())
                {
                    return target.Value;
                }
            }

            var newTarget = new Target(module, this, fileRef, type);
            this.Targets[module.GetType()] = newTarget;
            return newTarget;
        }

        public void
        AddNewProjectConfiguration(
            Bam.Core.Module module)
        {
            var config = module.BuildEnvironment.Configuration;
            if (this.ProjectConfigurations.ContainsKey(config))
            {
                return;
            }

            // add configuration to project
            var projectConfig = new Configuration(config.ToString());
            projectConfig["USE_HEADERMAP"] = new UniqueConfigurationValue("NO");
            projectConfig["COMBINE_HIDPI_IMAGES"] = new UniqueConfigurationValue("NO"); // TODO: needed to quieten Xcode 4 verification

            // reset SRCROOT, or it is taken to be where the workspace is
            projectConfig["SRCROOT"] = new UniqueConfigurationValue(this.SourceRoot);

            // all 'products' are relative to SYMROOT in the IDE, regardless of the project settings
            // needed so that built products are no longer 'red' in the IDE
            projectConfig["SYMROOT"] = new UniqueConfigurationValue(this.BuiltProductsDir);

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

        public Configuration
        AddNewTargetConfiguration(
            Bam.Core.Module module,
            Bam.Core.TokenizedString productName,
            Target target)
        {
            // add configuration to target
            var config = new Configuration(module.BuildEnvironment.Configuration.ToString());
            config["PRODUCT_NAME"] = new UniqueConfigurationValue(productName.Parse());

            target.ConfigurationList.AddConfiguration(config);
            this.AllConfigurations.Add(config);

            return config;
        }

        public void
        FixupPerConfigurationData()
        {
            foreach (var targetPair in this.Targets)
            {
                var target = targetPair.Value;
                foreach (var config in target.ConfigurationList)
                {
                    var diff = target.SourcesBuildPhase.BuildFiles.Complement(config.BuildFiles);
                    if (diff.Count > 0)
                    {
                        var excluded = new Bam.Core.StringArray();
                        foreach (var file in diff)
                        {
                            excluded.AddUnique(file.Source.Path.Parse());
                        }
                        config["EXCLUDED_SOURCE_FILE_NAMES"] = new UniqueConfigurationValue(excluded.ToString(" "));
                    }
                }
            }
        }

        private void InternalSerialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendLine();
            text.AppendFormat("/* Begin PBXProject section */");
            text.AppendLine();

            text.AppendFormat("{0}{1} /* Project object */ = {{", indent, this.GUID);
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            text.AppendFormat("{0}attributes = {{", indent2);
            text.AppendLine();
            text.AppendFormat("{0}LastUpgradeCheck = {1};", indent3, "0460"); // TODO: this is for Xcode 4
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

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
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
            if (this.Targets.Count > 0)
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
    }
}

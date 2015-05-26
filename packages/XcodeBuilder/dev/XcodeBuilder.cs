#region License
// Copyright 2010-2015 Mark Final
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
#endregion // License

[assembly: Bam.Core.DeclareBuilder("Xcode", typeof(XcodeBuilder.XcodeBuilder))]

namespace XcodeBuilder
{
namespace V2
{
    public abstract class Object
    {
        protected Object()
        {
            this.GUID = MakeGUID();
        }

        protected static string MakeGUID()
        {
            var guid = System.Guid.NewGuid();
            var toString = guid.ToString("N").ToUpper(); // this is 32 characters long

            // cannot create a 24-char (96 bit) GUID, so just chop off the front and rear 4 characters
            // this ought to be random enough
            var toString24Chars = toString.Substring(4, 24);
            return toString24Chars;
        }

        public abstract string GUID
        {
            get;
            protected set;
        }

        public abstract void Serialize(System.Text.StringBuilder text, int indentLevel);
    }

    public sealed class FileReference :
        Object
    {
        public enum EFileType
        {
            SourceCodeC,
            Archive,
            Executable
        }

        public enum ESourceTree
        {
            NA,       /* maps to <unknown> */
            Absolute, /* absolute path */
            Group,    /* group of things? (which group, where?) */
            SourceRoot, /* relative to project */
            DeveloperDir, /* relative to developer directory */
            BuiltProductsDir, /* relative to where products are built in project */
            SDKRoot /* relative to SDK root */
        }

        public FileReference(
            Bam.Core.V2.TokenizedString path,
            EFileType type,
            Project project,
            bool explicitType = false,
            ESourceTree sourceTree = ESourceTree.NA)
        {
            this.Path = path;
            this.Type = type;
            this.Project = project;
            this.SourceTree = sourceTree;
            this.ExplicitType = explicitType;
        }

        public FileReference(string path, FileReference other)
        {
            this.Path = Bam.Core.V2.TokenizedString.Create(path, null);
            this.Type = other.Type;
            this.Project = other.Project;
            this.SourceTree = other.SourceTree;
            this.ExplicitType = other.ExplicitType;
        }

        public static FileReference
        MakeLinkedClone(
            Project project,
            Bam.Core.EConfiguration configuration,
            FileReference other)
        {
            // need to constructed a path that is the original, relative to the current project's built products dir
            var originalPath = other.Path.ToString();
            var thisProjectPath = System.String.Format("{0}/{1}/", project.BuiltProductsDir, configuration.ToString());
            var relativePath = Bam.Core.RelativePathUtilities.GetPath(originalPath, thisProjectPath);
            var clone = project.FindOrCreateFileReference(relativePath, other);
            return clone;
        }

        public Bam.Core.V2.TokenizedString Path
        {
            get;
            private set;
        }

        public EFileType Type
        {
            get;
            private set;
        }

        private Project Project
        {
            get;
            set;
        }

        private bool ExplicitType
        {
            get;
            set;
        }

        private ESourceTree SourceTree
        {
            get;
            set;
        }

        public override string GUID
        {
            get;
            protected set;
        }

        private string FileTypeAsString()
        {
            switch (this.Type)
            {
                case EFileType.SourceCodeC:
                    return "sourcecode.c.c";

                case EFileType.Archive:
                    return "archive.ar";

                case EFileType.Executable:
                    return "compiled.mach-o.executable";
            }

            throw new Bam.Core.Exception("Unrecognized file type");
        }

        private string SourceTreeAsString()
        {
            switch (this.SourceTree)
            {
                case ESourceTree.NA:
                    return "\"<unknown>\"";

                case ESourceTree.Absolute:
                    return "\"<absolute>\"";

                case ESourceTree.Group:
                    return "\"<group>\"";

                case ESourceTree.SourceRoot:
                    return "SOURCE_ROOT";

                case ESourceTree.DeveloperDir:
                    return "DEVELOPER_DIR";

                case ESourceTree.BuiltProductsDir:
                    return "BUILT_PRODUCTS_DIR";

                case ESourceTree.SDKRoot:
                    return "SDKROOT";

                default:
                    throw new Bam.Core.Exception("Unknown source tree");
            }
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            text.AppendFormat("{0}{1} /* FILLMEIN */ = {{isa = PBXFileReference; ", indent, this.GUID);
            if (this.ExplicitType)
            {
                text.AppendFormat("explicitFileType = {0}; ", this.FileTypeAsString());
            }
            else
            {
                text.AppendFormat("lastKnownFileType = {0}; ", this.FileTypeAsString());
            }

            var name = System.IO.Path.GetFileName(this.Path.ToString());
            text.AppendFormat("name = \"{0}\"; ", name);

            string path = null;
            switch (this.SourceTree)
            {
                case ESourceTree.NA:
                case ESourceTree.Absolute:
                    path = this.Path.ToString();
                    break;

                case ESourceTree.BuiltProductsDir:
                    {
                        var fully = this.Path.ToString();
                        var builtProductsDir = this.Project.BuiltProductsDir;
                        path = "./" + Bam.Core.RelativePathUtilities.GetPath(fully, builtProductsDir + "/");
                    }
                    break;

                default:
                    throw new Bam.Core.Exception("Other source trees not handled yet");
            }
            text.AppendFormat("path = \"{0}\" /* FILLEMEIN */; ", path);

            text.AppendFormat("sourceTree = {0}; ", this.SourceTreeAsString());
            text.AppendFormat("}};");
            text.AppendLine();
        }
    }

    public sealed class BuildFile :
        Object
    {
        public BuildFile(
            Bam.Core.V2.TokenizedString path,
            FileReference source)
        {
            this.Path = path;
            this.Source = source;
        }

        public Bam.Core.V2.TokenizedString Path
        {
            get;
            private set;
        }

        public FileReference Source
        {
            get;
            private set;
        }

        public Bam.Core.StringArray Settings
        {
            get;
            set;
        }

        public override string GUID
        {
            get;
            protected set;
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            text.AppendFormat("{0}{1} /* FILLMEIN */ = {{isa = PBXBuildFile; fileRef = {2} /* FILLEMEIN */; ", indent, this.GUID, this.Source.GUID);
            if (this.Settings != null)
            {
                text.AppendFormat("settings = {{COMPILER_FLAGS = \"{0}\"; }}; ", this.Settings.ToString(' '));
            }
            text.AppendFormat("}};", indent, this.GUID, this.Source.GUID);
            text.AppendLine();
        }
    }

    public sealed class Group :
        Object
    {
        public Group()
            : this("<group>")
        {}

        public Group(string sourceTree)
        {
            this.Children = new System.Collections.Generic.List<Object>();
            this.SourceTree = sourceTree;
        }

        public string Name
        {
            get;
            set;
        }

        public string SourceTree
        {
            get;
            private set;
        }

        private System.Collections.Generic.List<Object> Children
        {
            get;
            set;
        }

        public void
        AddReference(Object other)
        {
            foreach (var child in this.Children)
            {
                if (child.GUID == other.GUID)
                {
                    return;
                }
            }
            this.Children.Add(other);
        }

        public override string GUID
        {
            get;
            protected set;
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            if (null != this.Name)
            {
                text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
            }
            else
            {
                text.AppendFormat("{0}{1} = {{", indent, this.GUID);
            }
            text.AppendLine();
            text.AppendFormat("{0}isa = PBXGroup;", indent2);
            text.AppendLine();
            if (this.Children.Count > 0)
            {
                text.AppendFormat("{0}children = (", indent2);
                text.AppendLine();
                foreach (var child in this.Children)
                {
                    text.AppendFormat("{0}{1} /* FILLMEIN */,", indent3, child.GUID);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            if (null != this.Name)
            {
                text.AppendFormat("{0}name = {1};", indent2, this.Name);
                text.AppendLine();
            }
            text.AppendFormat("{0}sourceTree = \"{1}\";", indent2, this.SourceTree);
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }

    public abstract class BuildPhase :
        Object
    {
        protected BuildPhase()
        {
            this.BuildFiles = new System.Collections.Generic.List<BuildFile>();
        }

        public void
        AddBuildFile(BuildFile other)
        {
            foreach (var build in this.BuildFiles)
            {
                if (build.GUID == other.GUID)
                {
                    return;
                }
            }
            this.BuildFiles.Add(other);
        }

        public override string GUID
        {
            get;
            protected set;
        }

        protected abstract string IsA
        {
            get;
        }

        protected abstract string BuildActionMask
        {
            get;
        }

        protected abstract bool RunOnlyForDeploymentPostprocessing
        {
            get;
        }

        private System.Collections.Generic.List<BuildFile> BuildFiles
        {
            get;
            set;
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* FILLMEIN */ = {{", indent, this.GUID);
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            text.AppendFormat("{0}buildActionMask = {1};", indent2, this.BuildActionMask);
            text.AppendLine();
            if (this.BuildFiles.Count > 0)
            {
                text.AppendFormat("{0}files = (", indent2);
                text.AppendLine();
                foreach (var file in this.BuildFiles)
                {
                    text.AppendFormat("{0}{1} /* FILLMEIN */,", indent3, file.GUID);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}runOnlyForDeploymentPostprocessing = {1};", indent2, this.RunOnlyForDeploymentPostprocessing ? "1" : "0");
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }

    public sealed class SourcesBuildPhase :
        BuildPhase
    {
        protected override string IsA
        {
            get
            {
                return "PBXSourcesBuildPhase";
            }
        }

        protected override string BuildActionMask
        {
            get
            {
                return "0";
            }
        }

        protected override bool RunOnlyForDeploymentPostprocessing
        {
            get
            {
                return false;
            }
        }
    }

    public sealed class FrameworksBuildPhase :
        BuildPhase
    {
        protected override string IsA
        {
            get
            {
                return "PBXFrameworksBuildPhase";
            }
        }

        protected override string BuildActionMask
        {
            get
            {
                return "2147483647";
            }
        }

        protected override bool RunOnlyForDeploymentPostprocessing
        {
            get
            {
                return false;
            }
        }
    }

    public sealed class Project :
        Object
    {
        public Project(Bam.Core.V2.Module module) :
            base()
        {
            this.Module = module;
            this.Targets = new System.Collections.Generic.Dictionary<System.Type, Target>();
            this.FileReferences = new System.Collections.Generic.List<FileReference>();
            this.BuildFiles = new System.Collections.Generic.List<BuildFile>();
            this.Groups = new System.Collections.Generic.List<Group>();
            this.Configurations = new System.Collections.Generic.List<Configuration>();
            this.ConfigurationLists = new System.Collections.Generic.List<ConfigurationList>();
            this.SourcesBuildPhases = new System.Collections.Generic.List<SourcesBuildPhase>();
            this.FrameworksBuildPhases = new System.Collections.Generic.List<FrameworksBuildPhase>();

            this.Groups.Add(new Group()); // main group
            this.Groups.Add(new Group()); // product ref group

            this.ProductRefGroup.Name = "Products";
            this.MainGroup.AddReference(this.ProductRefGroup);

            var configList = new ConfigurationList(this);
            this.ConfigurationLists.Add(configList);
        }

        public string BuiltProductsDir
        {
            get
            {
                return this.Module.Package.BuildDirectory;
            }
        }

        public Bam.Core.V2.Module Module
        {
            get;
            private set;
        }

        private System.Collections.Generic.Dictionary<System.Type, Target> Targets
        {
            get;
            set;
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

        public System.Collections.Generic.List<Configuration> Configurations
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

        public override string GUID
        {
            get;
            protected set;
        }

        public FileReference
        FindOrCreateFileReference(
            Bam.Core.V2.TokenizedString path,
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
            string path,
            FileReference other)
        {
            foreach (var fileRef in this.FileReferences)
            {
                if (fileRef.Path.ToString() == path)
                {
                    return fileRef;
                }
            }

            var newFileRef = new FileReference(path, other);
            this.FileReferences.Add(newFileRef);
            return newFileRef;
        }

        public BuildFile
        FindOrCreateBuildFile(
            Bam.Core.V2.TokenizedString path,
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
            Bam.Core.V2.Module module,
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

        public Configuration
        AddNewTargetConfiguration(
            Bam.Core.V2.Module module,
            FileReference fileRef,
            Target target)
        {
            // add configuration to project
            var projectConfig = new Configuration(module.BuildEnvironment.Configuration.ToString());
            projectConfig["USE_HEADERMAP"] = new UniqueConfigurationValue("NO");
            projectConfig["COMBINE_HIDPI_IMAGES"] = new UniqueConfigurationValue("NO"); // TODO: needed to quieten Xcode 4 verification

            // all 'products' are relative to this in the IDE, regardless of the project settings
            // needed so that built products are no longer 'red' in the IDE
            projectConfig["SYMROOT"] = new UniqueConfigurationValue(this.BuiltProductsDir);

            //config["PROJECT_TEMP_DIR"] = new UniqueConfigurationValue("$SYMROOT");
            this.ConfigurationLists[0].AddConfiguration(projectConfig);
            this.Configurations.Add(projectConfig);


            // add configuration to target
            var config = new Configuration(module.BuildEnvironment.Configuration.ToString());
            config["PRODUCT_NAME"] = new UniqueConfigurationValue("$(TARGET_NAME)");

            // reset SRCROOT, or it is taken to be where the workspace is
            config["SRCROOT"] = new UniqueConfigurationValue(Bam.Core.V2.TokenizedString.Create("$(pkgroot)", module).Parse());

            // let's now override the SYMROOT for this configuration
            var absLibraryDir = System.IO.Path.GetDirectoryName(fileRef.Path.ToString());
            config["SYMROOT"] = new UniqueConfigurationValue(absLibraryDir);

            config["CONFIGURATION_BUILD_DIR"] = new UniqueConfigurationValue("$SYMROOT");

            target.ConfigurationList.AddConfiguration(config);
            this.Configurations.Add(config);

            return config;
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
            text.AppendFormat("{0}isa = PBXProject;", indent2);
            text.AppendLine();
            text.AppendFormat("{0}attributes = {{", indent2);
            text.AppendLine();
            text.AppendFormat("{0}LastUpgradeCheck = {1};", indent3, "0460"); // TODO: this is for Xcode 4
            text.AppendLine();
            text.AppendFormat("{0}}};", indent2);
            text.AppendLine();
            text.AppendFormat("{0}compatibilityVersion = \"{1}\";", indent2, "Xcode 3.2"); // TODO
            text.AppendLine();
            // project configuration list is always the first
            text.AppendFormat("{0}buildConfigurationList = {1} /* Build configuration list for FILLEMIN */;", indent2, this.ConfigurationLists[0].GUID);
            text.AppendLine();
            text.AppendFormat("{0}mainGroup = {1};", indent2, this.MainGroup.GUID);
            text.AppendLine();
            text.AppendFormat("{0}productRefGroup = {1} /* FILLMEIN */;", indent2, this.ProductRefGroup.GUID);
            text.AppendLine();
            text.AppendFormat("{0}targets = (", indent2);
            text.AppendLine();
            foreach (var target in this.Targets.Values)
            {
                text.AppendFormat("{0}{1} /* {2} */", indent3, target.GUID, "REPLACENAMEHERE");
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
                foreach (var buildFile in this.BuildFiles)
                {
                    buildFile.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXBuildFile section */");
                text.AppendLine();
            }
            if (this.FileReferences.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXFileReference section */");
                text.AppendLine();
                foreach (var fileRef in this.FileReferences)
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
                foreach (var phase in this.FrameworksBuildPhases)
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
                foreach (var group in this.Groups)
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
                foreach (var target in this.Targets.Values)
                {
                    target.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXNativeTarget section */");
                text.AppendLine();
            }
            this.InternalSerialize(text, indentLevel);
            if (this.SourcesBuildPhases.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin PBXSourcesBuildPhase section */");
                text.AppendLine();
                foreach (var phase in this.SourcesBuildPhases)
                {
                    phase.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End PBXSourcesBuildPhase section */");
                text.AppendLine();
            }
            if (this.Configurations.Count > 0)
            {
                text.AppendLine();
                text.AppendFormat("/* Begin XCBuildConfiguration section */");
                text.AppendLine();
                foreach (var config in this.Configurations)
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
                foreach (var configList in this.ConfigurationLists)
                {
                    configList.Serialize(text, indentLevel);
                }
                text.AppendFormat("/* End XCConfigurationList section */");
                text.AppendLine();
            }
        }
    }

    public sealed class Target :
        Object
    {
        public enum EProductType
        {
            StaticLibrary,
            Executable
        }

        public Target(
            Bam.Core.V2.Module module,
            Project project,
            FileReference fileRef,
            EProductType type) :
            base()
        {
            this.Name = module.GetType().Name;
            this.FileReference = fileRef;
            this.Type = type;

            var configList = new ConfigurationList(this);
            project.ConfigurationLists.Add(configList);
            this.ConfigurationList = configList;

            this.BuildPhases = new System.Collections.Generic.List<BuildPhase>();
            this.SourcesBuildPhase = new V2.SourcesBuildPhase();
            this.BuildPhases.Add(this.SourcesBuildPhase);

            this.Project = project;
            this.Project.SourcesBuildPhases.Add(this.SourcesBuildPhase);
        }

        public string Name
        {
            get;
            private set;
        }

        public SourcesBuildPhase SourcesBuildPhase
        {
            get;
            private set;
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

        public EProductType Type
        {
            get;
            private set;
        }

        public Project Project
        {
            get;
            set;
        }

        public System.Collections.Generic.List<BuildPhase> BuildPhases
        {
            get;
            private set;
        }

        public override string GUID
        {
            get;
            protected set;
        }

        public void
        SetCommonCompilationOptions(
            Bam.Core.V2.Module module,
            Configuration configuration,
            Bam.Core.V2.Settings settings)
        {
            (settings as XcodeProjectProcessor.V2.IConvertToProject).Convert(module, configuration);
        }

        private string ProductTypeToString()
        {
            switch (this.Type)
            {
                case EProductType.StaticLibrary:
                    return "com.apple.product-type.library.static";

                case EProductType.Executable:
                    return "com.apple.product-type.tool";
            }

            throw new Bam.Core.Exception("Unrecognized product type");
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* FILLMEIN */ = {{", indent, this.GUID);
            text.AppendLine();
            text.AppendFormat("{0}isa = PBXNativeTarget;", indent2);
            text.AppendLine();
            text.AppendFormat("{0}buildConfigurationList = {1} /* Build configuration list for FILLMEIN */;", indent2, this.ConfigurationList.GUID);
            text.AppendLine();
            if (this.BuildPhases.Count > 0)
            {
                text.AppendFormat("{0}buildPhases = (", indent2);
                text.AppendLine();
                foreach (var phase in this.BuildPhases)
                {
                    text.AppendFormat("{0}{1} /* FILLMEIN */,", indent3, phase.GUID);
                    text.AppendLine();
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
            text.AppendFormat("{0});", indent2);
            text.AppendLine();
            text.AppendFormat("{0}name = {1};", indent2, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}productName = {1};", indent2, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}productReference = {1} /* FILLMEIN */;", indent2, this.FileReference.GUID);
            text.AppendLine();
            text.AppendFormat("{0}productType = \"{1}\";", indent2, this.ProductTypeToString());
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }

    public sealed class ConfigurationList :
        Object
    {
        public ConfigurationList(Object parent)
        {
            this.Parent = parent;
            this.Configurations = new System.Collections.Generic.List<Configuration>();
        }

        public Configuration this[int index]
        {
            get
            {
                return this.Configurations[index];
            }
        }

        public Object Parent
        {
            get;
            private set;
        }

        private System.Collections.Generic.List<Configuration> Configurations
        {
            get;
            set;
        }

        public void
        AddConfiguration(Configuration config)
        {
            foreach (var conf in this.Configurations)
            {
                if (config.GUID == conf.GUID)
                {
                    return;
                }
            }
            this.Configurations.Add(config);
        }

        public override string GUID
        {
            get;
            protected set;
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* Build configuration list for FILLMEIN */ = {{", indent, this.GUID);
            text.AppendLine();
            text.AppendFormat("{0}isa = XCConfigurationList;", indent2);
            text.AppendLine();
            if (this.Configurations.Count > 0)
            {
                text.AppendFormat("{0}buildConfigurations = (", indent2);
                text.AppendLine();
                foreach (var config in this.Configurations)
                {
                    text.AppendFormat("{0}{1} /* FILLMEIN */,", indent3, config.GUID);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }

    public abstract class ConfigurationValue
    {
        public abstract void Merge(ConfigurationValue value);
    }

    public sealed class UniqueConfigurationValue :
        ConfigurationValue
    {
        public UniqueConfigurationValue(string value)
        {
            this.Value = value;
        }

        public override void Merge(ConfigurationValue value)
        {
            this.Value = (value as UniqueConfigurationValue).Value;
        }

        private string Value
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }

    public sealed class MultiConfigurationValue :
        ConfigurationValue
    {
        public MultiConfigurationValue()
        {
            this.Value = new Bam.Core.StringArray();
        }

        public MultiConfigurationValue(string value)
            : this()
        {
            this.Value.AddUnique(value);
        }

        public override void Merge(ConfigurationValue value)
        {
            this.Value.AddRangeUnique((value as MultiConfigurationValue).Value);
        }

        private Bam.Core.StringArray Value
        {
            get;
            set;
        }

        public void Add(string value)
        {
            this.Value.AddUnique(value);
        }

        public override string ToString()
        {
            return this.Value.ToString(' ');
        }
    }

    public sealed class Configuration :
        Object
    {
        public Configuration(string name)
            : base()
        {
            this.Name = name;
        }

        public string Name
        {
            get;
            private set;
        }

        public override string GUID
        {
            get;
            protected set;
        }

        public ConfigurationValue this[string key]
        {
            get
            {
                return this.Settings[key];
            }

            set
            {
                if (!this.Settings.ContainsKey(key))
                {
                    this.Settings[key] = value;
                }
                else
                {
                    this.Settings[key].Merge(value);
                }
            }
        }

        private System.Collections.Generic.Dictionary<string, ConfigurationValue> Settings = new System.Collections.Generic.Dictionary<string, ConfigurationValue>();

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}isa = XCBuildConfiguration;", indent2);
            text.AppendLine();
            text.AppendFormat("{0}buildSettings = {{", indent2);
            text.AppendLine();
            foreach (var setting in this.Settings)
            {
                text.AppendFormat("{0}{1} = \"{2}\";", indent3, setting.Key, setting.Value);
                text.AppendLine();
            }
            text.AppendFormat("{0}}};", indent2);
            text.AppendLine();
            text.AppendFormat("{0}name = {1};", indent2, this.Name);
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }

    public sealed class WorkspaceMeta :
        System.Collections.Generic.IEnumerable<Project>
    {
        public WorkspaceMeta()
        {
            this.Projects = new System.Collections.Generic.Dictionary<System.Type, Project>();
        }

        private System.Collections.Generic.Dictionary<System.Type, Project> Projects
        {
            get;
            set;
        }

        public Project
        FindOrCreateProject(
            Bam.Core.V2.Module module,
            XcodeMeta.Type projectType)
        {
            var moduleType = module.GetType();
            if (this.Projects.ContainsKey(moduleType))
            {
                return this.Projects[moduleType];
            }
            else
            {
                var project = new Project(module);
                this.Projects[moduleType] = project;
                return project;
            }
        }


        public System.Collections.Generic.IEnumerator<Project> GetEnumerator()
        {
            foreach (var project in this.Projects)
            {
                yield return project.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public abstract class XcodeMeta
    {
        public enum Type
        {
            NA,
            StaticLibrary,
            Application
        }

        protected XcodeMeta(Bam.Core.V2.Module module, Type type)
        {
            var graph = Bam.Core.V2.Graph.Instance;
            var isReferenced = graph.IsReferencedModule(module);
            this.IsProjectModule = isReferenced;

            var workspace = graph.MetaData as WorkspaceMeta;
            this.ProjectModule = isReferenced ? module : module.GetEncapsulatingReferencedModule();
            this.Project = workspace.FindOrCreateProject(this.ProjectModule, type);

            module.MetaData = this;
        }

        public bool IsProjectModule
        {
            get;
            private set;
        }

        public Bam.Core.V2.Module ProjectModule
        {
            get;
            private set;
        }

        public Project Project
        {
            get;
            set;
        }

        public Target Target
        {
            get;
            protected set;
        }

        public Configuration Configuration
        {
            get;
            protected set;
        }

        public static void PreExecution()
        {
            Bam.Core.V2.Graph.Instance.MetaData = new WorkspaceMeta();
        }

        public static void PostExecution()
        {
            // TODO: some alternatives
            // all modules in the same namespace -> targets in the .xcodeproj
            // one .xcodeproj for all modules -> each a target
            // one project per module, each with one target

            // TODO:
            // create folder <name>.xcodeproj
            // write file project.pbxproj
            // create folder project.xcworkspace
            // create folder xcuserdata

            var workspaceMeta = Bam.Core.V2.Graph.Instance.MetaData as WorkspaceMeta;

            var workspaceContents = new System.Xml.XmlDocument();
            var workspace = workspaceContents.CreateElement("Workspace");
            workspace.Attributes.Append(workspaceContents.CreateAttribute("version")).Value = "1.0";
            workspaceContents.AppendChild(workspace);

            foreach (var project in workspaceMeta)
            {
                var text = new System.Text.StringBuilder();
                text.AppendLine("// !$*UTF8*$!");
                text.AppendLine("{");
                var indentLevel = 1;
                var indent = new string('\t', indentLevel);
                text.AppendFormat("{0}archiveVersion = 1;", indent);
                text.AppendLine();
                text.AppendFormat("{0}classes = {{", indent);
                text.AppendLine();
                text.AppendFormat("{0}}};", indent);
                text.AppendLine();
                text.AppendFormat("{0}objectVersion = 46;", indent);
                text.AppendLine();
                text.AppendFormat("{0}objects = {{", indent);
                text.AppendLine();
                project.Serialize(text, indentLevel + 1);
                text.AppendFormat("{0}}};", indent);
                text.AppendLine();
                text.AppendFormat("{0}rootObject = {1} /* Project object */;", indent, project.GUID);
                text.AppendLine();
                text.AppendLine("}");

                var projectPath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(modulename).xcodeproj/project.pbxproj", project.Module);
                projectPath.Parse();

                var projectDir = System.IO.Path.GetDirectoryName(projectPath.ToString());
                if (!System.IO.Directory.Exists(projectDir))
                {
                    System.IO.Directory.CreateDirectory(projectDir);
                }

                //Bam.Core.Log.DebugMessage(text.ToString());
                using (var writer = new System.IO.StreamWriter(projectPath.ToString()))
                {
                    writer.Write(text.ToString());
                }

                var workspaceFileRef = workspaceContents.CreateElement("FileRef");
                workspaceFileRef.Attributes.Append(workspaceContents.CreateAttribute("location")).Value = System.String.Format("group:{0}", projectDir);
                workspace.AppendChild(workspaceFileRef);
            }

            var workspacePath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/workspace.xcworkspace/contents.xcworkspacedata", null);
            workspacePath.Parse();

            var workspaceDir = System.IO.Path.GetDirectoryName(workspacePath.ToString());
            if (!System.IO.Directory.Exists(workspaceDir))
            {
                System.IO.Directory.CreateDirectory(workspaceDir);
            }

            var settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Encoding = new System.Text.UTF8Encoding(false); // no BOM
            settings.NewLineChars = System.Environment.NewLine;
            settings.Indent = true;
            settings.ConformanceLevel = System.Xml.ConformanceLevel.Document;

            using (var xmlwriter = System.Xml.XmlWriter.Create(workspacePath.ToString(), settings))
            {
                workspaceContents.WriteTo(xmlwriter);
            }

            var workspaceSettings = new WorkspaceSettings(workspaceDir);
            workspaceSettings.Serialize();
        }
    }

    public sealed class XcodeObjectFile :
        XcodeMeta
    {
        public XcodeObjectFile(Bam.Core.V2.Module module) :
            base(module, Type.NA)
        {
        }

        public FileReference Source
        {
            get;
            set;
        }

        public BuildFile Output
        {
            get;
            set;
        }
    }

    public sealed class XcodeStaticLibrary :
        XcodeMeta
    {
        public XcodeStaticLibrary(
            Bam.Core.V2.Module module,
            Bam.Core.V2.TokenizedString libraryPath) :
            base(module, Type.StaticLibrary)
        {
            var library = this.Project.FindOrCreateFileReference(
                libraryPath,
                FileReference.EFileType.Archive,
                explicitType:true,
                sourceTree:FileReference.ESourceTree.BuiltProductsDir);
            this.Output = library;
            this.Project.ProductRefGroup.AddReference(library);
            this.Target = this.Project.FindOrCreateTarget(module, library, V2.Target.EProductType.StaticLibrary);
            this.Configuration = this.Project.AddNewTargetConfiguration(module, library, this.Target);
        }

        public void AddSource(Bam.Core.V2.Module module, FileReference source, BuildFile output, Bam.Core.V2.Settings patchSettings)
        {
            if (null != patchSettings)
            {
                var commandLine = new Bam.Core.StringArray();
                (patchSettings as CommandLineProcessor.V2.IConvertToCommandLine).Convert(module, commandLine);
                output.Settings = commandLine;
            }
            this.Target.SourcesBuildPhase.AddBuildFile(output);
            this.Project.MainGroup.AddReference(source); // TODO: will do proper grouping later
        }

        public void SetCommonCompilationOptions(Bam.Core.V2.Module module, Bam.Core.V2.Settings settings)
        {
            this.Target.SetCommonCompilationOptions(module, this.Configuration, settings);
        }

        public FileReference Output
        {
            get;
            private set;
        }
    }

    public sealed class XcodeProgram :
        XcodeMeta
    {
        public XcodeProgram(
            Bam.Core.V2.Module module,
            Bam.Core.V2.TokenizedString executablePath) :
            base(module, Type.Application)
        {
            var application = this.Project.FindOrCreateFileReference(
                executablePath,
                FileReference.EFileType.Executable,
                explicitType:true,
                sourceTree:FileReference.ESourceTree.BuiltProductsDir);
            this.Output = application;
            this.Project.ProductRefGroup.AddReference(application);
            this.Target = this.Project.FindOrCreateTarget(module, application, V2.Target.EProductType.Executable);
            this.Configuration = this.Project.AddNewTargetConfiguration(module, application, this.Target);
        }

        public void AddSource(Bam.Core.V2.Module module, FileReference source, BuildFile output, Bam.Core.V2.Settings patchSettings)
        {
            if (null != patchSettings)
            {
                var commandLine = new Bam.Core.StringArray();
                (patchSettings as CommandLineProcessor.V2.IConvertToCommandLine).Convert(module, commandLine);
                output.Settings = commandLine;
            }
            this.Target.SourcesBuildPhase.AddBuildFile(output);
            this.Project.MainGroup.AddReference(source); // TODO: will do proper grouping later
        }

        public void
        SetCommonCompilationOptions(
            Bam.Core.V2.Module module,
            Bam.Core.V2.Settings settings)
        {
            this.Target.SetCommonCompilationOptions(module, this.Configuration, settings);
        }

        public FileReference Output
        {
            get;
            private set;
        }

        private FrameworksBuildPhase Frameworks = null;

        public void AddStaticLibrary(XcodeStaticLibrary library)
        {
            if (null == this.Frameworks)
            {
                this.Frameworks = new FrameworksBuildPhase();
                this.Project.FrameworksBuildPhases.Add(this.Frameworks);
                this.Target.BuildPhases.Add(this.Frameworks);
            }
            // this generates a new GUID
            var copyOfLibFileRef = FileReference.MakeLinkedClone(this.Project, this.ProjectModule.BuildEnvironment.Configuration, library.Output);
            var libraryBuildFile = this.Project.FindOrCreateBuildFile(library.Output.Path, copyOfLibFileRef);
            this.Project.MainGroup.AddReference(copyOfLibFileRef); // TODO: structure later
            this.Frameworks.AddBuildFile(libraryBuildFile);
        }
    }

    public class WorkspaceSettings
    {
        public
        WorkspaceSettings(
            string workspaceDir)
        {
            this.Path = workspaceDir;
            this.Path = System.IO.Path.Combine(this.Path, "xcuserdata");
            this.Path = System.IO.Path.Combine(this.Path, System.Environment.GetEnvironmentVariable("USER") + ".xcuserdatad");
            this.Path = System.IO.Path.Combine(this.Path, "WorkspaceSettings.xcsettings");
            this.CreatePlist();
        }

        private void
        CreateKeyValuePair(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement parent,
            string key,
            string value)
        {
            var keyEl = doc.CreateElement("key");
            keyEl.InnerText = key;
            var valueEl = doc.CreateElement("string");
            valueEl.InnerText = value;
            parent.AppendChild(keyEl);
            parent.AppendChild(valueEl);
        }

        private void
        CreatePlist()
        {
            var doc = new System.Xml.XmlDocument();
            // don't resolve any URLs, or if there is no internet, the process will pause for some time
            doc.XmlResolver = null;

            {
                var type = doc.CreateDocumentType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
                doc.AppendChild(type);
            }
            var plistEl = doc.CreateElement("plist");
            {
                var versionAttr = doc.CreateAttribute("version");
                versionAttr.Value = "1.0";
                plistEl.Attributes.Append(versionAttr);
            }

            var dictEl = doc.CreateElement("dict");
            plistEl.AppendChild(dictEl);
            doc.AppendChild(plistEl);

#if true
            // TODO: this seems to be the only way to get the target settings working
            CreateKeyValuePair(doc, dictEl, "BuildLocationStyle", "UseTargetSettings");
#else
            // build and intermediate file locations
            CreateKeyValuePair(doc, dictEl, "BuildLocationStyle", "CustomLocation");
            CreateKeyValuePair(doc, dictEl, "CustomBuildIntermediatesPath", "XcodeIntermediates"); // where xxx.build folders are stored
            CreateKeyValuePair(doc, dictEl, "CustomBuildLocationType", "RelativeToWorkspace");
            CreateKeyValuePair(doc, dictEl, "CustomBuildProductsPath", "."); // has to be the workspace folder, in order to write files to expected locations

            // derived data
            CreateKeyValuePair(doc, dictEl, "DerivedDataCustomLocation", "XcodeDerivedData");
            CreateKeyValuePair(doc, dictEl, "DerivedDataLocationStyle", "WorkspaceRelativePath");
#endif

            this.Document = doc;
        }

        private string Path
        {
            get;
            set;
        }

        private System.Xml.XmlDocument Document
        {
            get;
            set;
        }

        public void
        Serialize()
        {
            // do not write a Byte-Ordering-Mark (BOM)
            var encoding = new System.Text.UTF8Encoding(false);

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.Path));
            using (var writer = new System.IO.StreamWriter(this.Path, false, encoding))
            {
                var settings = new System.Xml.XmlWriterSettings();
                settings.OmitXmlDeclaration = false;
                settings.NewLineChars = "\n";
                settings.Indent = true;
                using (var xmlWriter = System.Xml.XmlWriter.Create(writer, settings))
                {
                    this.Document.WriteTo(xmlWriter);
                    xmlWriter.WriteWhitespace(settings.NewLineChars);
                }
            }
        }
    }
}
}

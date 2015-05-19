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
            Archive
        }

        public FileReference(string path, EFileType type)
        {
            this.Path = path;
            this.Type = type;
        }

        public string Path
        {
            get;
            private set;
        }

        public EFileType Type
        {
            get;
            private set;
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
            }

            throw new Bam.Core.Exception("Unrecognized file type");
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            text.AppendFormat("{0}{1} /* FILLMEIN */ = {{isa = PBXFileReference; lastKnownFileType = {2}; path = \"{3}\" /* FILLEMEIN */; }};", indent, this.GUID, this.FileTypeAsString(), this.Path);
            text.AppendLine();
        }
    }

    public sealed class BuildFile :
        Object
    {
        public BuildFile(string path, FileReference source)
        {
            this.Path = path;
            this.Source = source;
        }

        public string Path
        {
            get;
            private set;
        }

        public FileReference Source
        {
            get;
            private set;
        }

        public override string GUID
        {
            get;
            protected set;
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            text.AppendFormat("{0}{1} /* FILLMEIN */ = {{isa = PBXBuildFile; fileRef = {2} /* FILLEMEIN */; }};", indent, this.GUID, this.Source.GUID);
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

        public System.Collections.Generic.List<Object> Children
        {
            get;
            private set;
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

        public System.Collections.Generic.List<BuildFile> BuildFiles
        {
            get;
            private set;
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

    public sealed class Project :
        Object
    {
        public Project() :
            base()
        {
            this.Targets = new System.Collections.Generic.List<Target>();
            this.FileReferences = new System.Collections.Generic.List<FileReference>();
            this.BuildFiles = new System.Collections.Generic.List<BuildFile>();
            this.Groups = new System.Collections.Generic.List<Group>();
            this.Configurations = new System.Collections.Generic.List<Configuration>();
            this.ConfigurationLists = new System.Collections.Generic.List<ConfigurationList>();
            this.SourcesBuildPhases = new System.Collections.Generic.List<SourcesBuildPhase>();

            this.Groups.Add(new Group()); // main group
            this.Groups.Add(new Group()); // product ref group

            this.ProductRefGroup.Name = "Products";
            this.MainGroup.Children.Add(this.ProductRefGroup);

            var config = new Configuration("Debug"); // TODO: is debug?
            config["USE_HEADERMAP"] = "NO";
            config["COMBINE_HIDPI_IMAGES"] = "NO"; // TODO: needed to quieten Xcode 4 verification
            config["SYMROOT"] = Bam.Core.State.BuildRoot;
            config["PROJECT_TEMP_DIR"] = "$SYMROOT";
            var configList = new ConfigurationList(this);
            configList.Configurations.Add(config);
            this.Configurations.Add(config);
            this.ConfigurationLists.Add(configList);
        }

        public System.Collections.Generic.List<Target> Targets
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<FileReference> FileReferences
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<BuildFile> BuildFiles
        {
            get;
            private set;
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
            text.AppendFormat("{0}buildConfigurationList = {1} /* Build configuration list for FILLEMIN */;", indent2, this.ConfigurationLists[0].GUID);
            text.AppendLine();
            text.AppendFormat("{0}mainGroup = {1};", indent2, this.MainGroup.GUID);
            text.AppendLine();
            text.AppendFormat("{0}productRefGroup = {1} /* FILLMEIN */;", indent2, this.ProductRefGroup.GUID);
            text.AppendLine();
            text.AppendFormat("{0}targets = (", indent2);
            text.AppendLine();
            foreach (var target in this.Targets)
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
                foreach (var target in this.Targets)
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
            StaticLibrary
        }

        public Target(Bam.Core.V2.Module module, Project project, FileReference fileRef, EProductType type) :
            base()
        {
            this.Name = module.GetType().Name;
            this.FileReference = fileRef;
            this.Type = type;

            var config = new Configuration("Debug"); // TODO: is debug?
            config["PRODUCT_NAME"] = "$(TARGET_NAME)";
            config["CONFIGURATION_TEMP_DIR"] = "$PROJECT_TEMP_DIR";
            var configList = new ConfigurationList(this);
            configList.Configurations.Add(config);
            project.Configurations.Add(config);
            project.ConfigurationLists.Add(configList);
            this.ConfigurationList = configList;

            this.BuildPhases = new System.Collections.Generic.List<BuildPhase>();
            this.SourcesBuildPhase = new V2.SourcesBuildPhase();
            this.BuildPhases.Add(this.SourcesBuildPhase);

            project.Targets.Add(this);
            this.Project = project;
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

        private string ProductTypeToString()
        {
            switch (this.Type)
            {
                case EProductType.StaticLibrary:
                    return "com.apple.product-type.library.static";
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

        public Object Parent
        {
            get;
            private set;
        }

        public System.Collections.Generic.List<Configuration> Configurations
        {
            get;
            private set;
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

        public string this[string key]
        {
            get
            {
                return this.Settings[key];
            }

            set
            {
                this.Settings[key] = value;
            }
        }

        private System.Collections.Generic.Dictionary<string, string> Settings = new System.Collections.Generic.Dictionary<string, string>();

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

    public sealed class WorkspaceMeta
    {
        public WorkspaceMeta()
        {
            this.Projects = new System.Collections.Generic.List<Project>();
        }

        public System.Collections.Generic.List<Project> Projects
        {
            get;
            private set;
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

            if (isReferenced)
            {
                this.ProjectModule = module;
                this.Project = new Project();
                (graph.MetaData as WorkspaceMeta).Projects.Add(this.Project);
            }
            else
            {
                this.ProjectModule = module.GetEncapsulatingReferencedModule();
            }

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

        public Object Target
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

            var projectPath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/Test.xcodeproj/project.pbxproj", null);
            projectPath.Parse();

            var projectDir = System.IO.Path.GetDirectoryName(projectPath.ToString());
            if (!System.IO.Directory.Exists(projectDir))
            {
                System.IO.Directory.CreateDirectory(projectDir);
            }

            var workspaceMeta = Bam.Core.V2.Graph.Instance.MetaData as WorkspaceMeta;

            foreach (var project in workspaceMeta.Projects)
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

                //Bam.Core.Log.DebugMessage(text.ToString());
                using (var writer = new System.IO.StreamWriter(projectPath.ToString()))
                {
                    writer.Write(text.ToString());
                }
            }
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
        public XcodeStaticLibrary(Bam.Core.V2.Module module, string libraryPath) :
            base(module, Type.StaticLibrary)
        {
            var library = new FileReference(libraryPath, FileReference.EFileType.Archive);
            this.Output = library;
            this.Project.FileReferences.Add(library);
            this.Project.ProductRefGroup.Children.Add(library);

            var target = new Target(module, this.Project, library, V2.Target.EProductType.StaticLibrary);
            this.Target = target;
            this.Project.SourcesBuildPhases.Add(target.SourcesBuildPhase);
        }

        public void AddSource(FileReference source, BuildFile output)
        {
            var target = this.Target as Target;
            target.SourcesBuildPhase.BuildFiles.Add(output);

            this.Project.FileReferences.Add(source);
            this.Project.MainGroup.Children.Add(source); // TODO: will do proper grouping later
            this.Project.BuildFiles.Add(output);
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
        public XcodeProgram(Bam.Core.V2.Module module) :
            base(module, Type.Application)
        { }
    }
}
}

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
namespace V2
{
    public abstract class Object
    {
        protected Object()
        {
            this.GUID = MakeGUID();
            this.Name = "Please Provide Name";
            this.IsA = "Undefined Xcode Object Type";
        }

        private static string
        MakeGUID()
        {
            var guid = System.Guid.NewGuid();
            var toString = guid.ToString("N").ToUpper(); // this is 32 characters long

            // cannot create a 24-char (96 bit) GUID, so just chop off the front and rear 4 characters
            // this ought to be random enough
            var toString24Chars = toString.Substring(4, 24);
            return toString24Chars;
        }

        public string GUID
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            protected set;
        }

        public string IsA
        {
            get;
            protected set;
        }

        public abstract void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel);
    }

    public sealed class ContainerItemProxy :
        Object
    {
        public ContainerItemProxy(
            Project project,
            Object reference)
        {
            this.IsA = "PBXContainerItemProxy";
            this.Name = "PBXContainerItemProxy";
            this.ContainerPortal = project;
            this.ProxyType = 1; // TODO: what does this mean?
            this.Remote = reference;

            project.ContainerItemProxies.AddUnique(this);
        }

        private Object ContainerPortal
        {
            get;
            set;
        }

        private int ProxyType
        {
            get;
            set;
        }

        private Object Remote
        {
            get;
            set;
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            if (null != this.Name)
            {
                text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
            }
            else
            {
                text.AppendFormat("{0}{1} = {{", indent, this.GUID);
            }
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            var typeWithoutPBX = this.ContainerPortal.IsA.Substring(3);
            text.AppendFormat("{0}containerPortal = {1} /* {2} object */;", indent2, this.ContainerPortal.GUID, typeWithoutPBX);
            text.AppendLine();
            text.AppendFormat("{0}proxyType = {1};", indent2, this.ProxyType);
            text.AppendLine();
            text.AppendFormat("{0}remoteGlobalIDString = {1};", indent2, this.Remote.GUID);
            text.AppendLine();
            text.AppendFormat("{0}remoteInfo = {1};", indent2, this.Remote.Name);
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }

    public sealed class TargetDependency :
        Object
    {
        public TargetDependency(
            Target dependency,
            ContainerItemProxy proxy)
        {
            this.IsA = "PBXTargetDependency";
            this.Name = "PBXTargetDependency";
            this.Dependency = dependency;
            this.Proxy = proxy;

            dependency.Project.TargetDependencies.AddUnique(this);
        }

        private Target Dependency
        {
            get;
            set;
        }

        private ContainerItemProxy Proxy
        {
            get;
            set;
        }

        public override void
        Serialize(
            System.Text.StringBuilder text,
            int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            if (null != this.Name)
            {
                text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
            }
            else
            {
                text.AppendFormat("{0}{1} = {{", indent, this.GUID);
            }
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            text.AppendFormat("{0}target = {1} /* {2} */;", indent2, this.Dependency.GUID, this.Dependency.Name);
            text.AppendLine();
            text.AppendFormat("{0}targetProxy = {1} /* {2} */;", indent2, this.Proxy.GUID, this.Proxy.Name);
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }

    public sealed class FileReference :
        Object
    {
        public enum EFileType
        {
            SourceCodeC,
            HeaderFile,
            Archive,
            Executable,
            DynamicLibrary,
            WrapperFramework,
            ApplicationBundle
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

        private FileReference()
        {
            this.IsA = "PBXFileReference";
        }

        public FileReference(
            Bam.Core.TokenizedString path,
            EFileType type,
            Project project,
            bool explicitType = false,
            ESourceTree sourceTree = ESourceTree.NA)
            :
            this()
        {
            this.Path = path;
            this.Type = type;
            this.Project = project;
            this.SourceTree = sourceTree;
            this.ExplicitType = explicitType;
            this.LinkedTo = null;
        }

        public FileReference(
            FileReference other,
            Project owningProject)
            :
            this()
        {
            this.Path = other.Path;
            this.Type = other.Type;
            this.Project = owningProject;
            // TODO: Linked FileReferences should be <group> and non-explicit
            this.SourceTree = other.SourceTree;
            this.ExplicitType = other.ExplicitType;
            this.LinkedTo = other;
        }

        /// <summary>
        /// This generates a new GUID
        /// or returns null if the target is in the current project
        /// </summary>
        /// <param name="project"></param>
        /// <param name="configuration"></param>
        /// <param name="originalFileRef"></param>
        /// <returns></returns>
        public static FileReference
        MakeLinkedClone(
            Project project,
            Bam.Core.EConfiguration configuration,
            FileReference originalFileRef)
        {
            if (project == originalFileRef.Project)
            {
                return null;
            }
            var clone = project.FindOrCreateFileReference(originalFileRef);
            return clone;
        }

        private Bam.Core.TokenizedString ThePath;
        public Bam.Core.TokenizedString Path
        {
            get
            {
                return this.ThePath;
            }
            private set
            {
                this.ThePath = value;
                this.Name = System.IO.Path.GetFileName(value.Parse());
            }
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

        public FileReference LinkedTo
        {
            get;
            private set;
        }

        public void
        MakeApplicationBundle()
        {
            if (this.Type == EFileType.ApplicationBundle)
            {
                return;
            }
            if (this.Type != EFileType.Executable)
            {
                throw new Bam.Core.Exception("Can only make executables into application bundles");
            }
            this.Type = EFileType.ApplicationBundle;
        }

        private string FileTypeAsString()
        {
            switch (this.Type)
            {
                case EFileType.SourceCodeC:
                    return "sourcecode.c.c";

                case EFileType.HeaderFile:
                    return "sourcecode.c.h";

                case EFileType.Archive:
                    return "archive.ar";

                case EFileType.Executable:
                    return "compiled.mach-o.executable";

                case EFileType.DynamicLibrary:
                    return "compiled.mach-o.dylib";

                case EFileType.WrapperFramework:
                    return "wrapper.framework";

                case EFileType.ApplicationBundle:
                    return "wrapper.application";
            }

            throw new Bam.Core.Exception("Unrecognized file type {0}", this.Type.ToString());
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
            var leafname = System.IO.Path.GetFileName(this.Path.ToString());

            var indent = new string('\t', indentLevel);
            text.AppendFormat("{0}{1} /* {2} */ = {{isa = {3}; ", indent, this.GUID, leafname, this.IsA);
            if (this.ExplicitType)
            {
                text.AppendFormat("explicitFileType = {0}; ", this.FileTypeAsString());
            }
            else
            {
                text.AppendFormat("lastKnownFileType = {0}; ", this.FileTypeAsString());
            }

            text.AppendFormat("name = \"{0}\"; ", leafname);

            string path = null;
            switch (this.SourceTree)
            {
                case ESourceTree.NA:
                case ESourceTree.Absolute:
                    path = this.Path.ToString();
                    break;

                case ESourceTree.BuiltProductsDir:
                    {
                        if ((null != this.LinkedTo) &&
                            (this.LinkedTo.Project.GUID != this.Project.GUID) &&
                            (this.LinkedTo.Project.BuiltProductsDir != this.Project.BuiltProductsDir))
                        {
                            // product is in a different BUILT_PRODUCTS_DIR - make a relative path
                            // but must use the definition of CONFIGURATION_BUILD_DIR to denote where it will end up
                            // cannot use the real path
                            var configName = this.Project.Module.BuildEnvironment.Configuration.ToString();
                            var macros = new Bam.Core.MacroList();
                            macros.Add("moduleoutputdir", Bam.Core.TokenizedString.Create(configName, null));
                            var fully = this.Path.Parse(macros);
                            var configurationBuildDir = this.Project.BuiltProductsDir + "/" + configName + "/";
                            path = Bam.Core.RelativePathUtilities.GetPath(fully, configurationBuildDir);
                        }
                        else
                        {
                            path = System.IO.Path.GetFileName(this.Path.ToString());
                        }
                    }
                    break;

                default:
                    throw new Bam.Core.Exception("Other source trees not handled yet");
            }
            text.AppendFormat("path = \"{0}\"; ", path);

            text.AppendFormat("sourceTree = {0}; ", this.SourceTreeAsString());
            text.AppendFormat("}};");
            text.AppendLine();
        }
    }

    public sealed class BuildFile :
        Object
    {
        public BuildFile(
            Bam.Core.TokenizedString path,
            FileReference source)
        {
            this.IsA = "PBXBuildFile";
            this.Path = path;
            this.Source = source;
        }

        public Bam.Core.TokenizedString Path
        {
            get;
            private set;
        }

        private FileReference TheSource;
        public FileReference Source
        {
            get
            {
                return this.TheSource;
            }
            private set
            {
                this.TheSource = value;
                this.Name = System.IO.Path.GetFileName(value.Path.ToString());
            }
        }

        public Bam.Core.StringArray Settings
        {
            get;
            set;
        }

        public Object Parent
        {
            get;
            set;
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            text.AppendFormat("{0}{1} /* {3} in {4} */ = {{isa = {5}; fileRef = {2} /* {3} */; ",
                indent, this.GUID, this.Source.GUID,
                this.Name,
                (null != this.Parent) ? this.Parent.Name : "Unknown",
                this.IsA);
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
        public Group(
            string name = null)
            : this(name, "<group>")
        {}

        private Group(
            string name,
            string sourceTree)
        {
            this.IsA = "PBXGroup";
            this.Name = name;
            this.Children = new System.Collections.Generic.List<Object>();
            this.SourceTree = sourceTree;
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
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            if (this.Children.Count > 0)
            {
                text.AppendFormat("{0}children = (", indent2);
                text.AppendLine();
                foreach (var child in this.Children)
                {
                    text.AppendFormat("{0}{1} /* {2} */,", indent3, child.GUID, child.Name);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            if (null != this.Name)
            {
                text.AppendFormat("{0}name = \"{1}\";", indent2, this.Name);
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
            this.BuildFiles = new Bam.Core.Array<BuildFile>();
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
            other.Parent = this;
        }

        protected abstract string BuildActionMask
        {
            get;
        }

        protected abstract bool RunOnlyForDeploymentPostprocessing
        {
            get;
        }

        public Bam.Core.Array<BuildFile> BuildFiles
        {
            get;
            protected set;
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
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
                    text.AppendFormat("{0}{1} /* {2} in {3} */,", indent3, file.GUID, file.Name, this.Name);
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
        public SourcesBuildPhase()
        {
            this.Name = "Sources";
            this.IsA = "PBXSourcesBuildPhase";
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
        public FrameworksBuildPhase()
        {
            this.Name = "Frameworks";
            this.IsA = "PBXFrameworksBuildPhase";
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

    public sealed class ShellScriptBuildPhase :
        BuildPhase
    {
        public delegate string GenerateScriptDelegate(Target target);

        public ShellScriptBuildPhase(
            Target target,
            string name,
            GenerateScriptDelegate generateScript)
        {
            this.Name = name;
            this.IsA = "PBXShellScriptBuildPhase";
            this.ShellPath = "/bin/sh";
            this.ShowEnvironmentInLog = true;
            this.InputPaths = new Bam.Core.StringArray();
            this.OutputPaths = new Bam.Core.StringArray();
            this.AssociatedTarget = target;
            this.GenerateScript = generateScript;
        }

        private GenerateScriptDelegate GenerateScript;

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

        public string ShellPath
        {
            get;
            private set;
        }

        public bool ShowEnvironmentInLog
        {
            get;
            set;
        }

        private Bam.Core.StringArray InputPaths
        {
            get;
            set;
        }

        private Bam.Core.StringArray OutputPaths
        {
            get;
            set;
        }

        public Target AssociatedTarget
        {
            get;
            private set;
        }

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* {2} */ = {{", indent, this.GUID, this.Name);
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
            if (this.InputPaths.Count > 0)
            {
                text.AppendFormat("{0}inputPaths = (", indent2);
                text.AppendLine();
                foreach (var path in this.InputPaths)
                {
                    text.AppendFormat("{0}\"{1}\",", indent3, path);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}name = \"{1}\";", indent2, this.Name);
            text.AppendLine();
            if (this.OutputPaths.Count > 0)
            {
                text.AppendFormat("{0}outputPaths = (", indent2);
                text.AppendLine();
                foreach (var path in this.OutputPaths)
                {
                    text.AppendFormat("{0}\"{1}\",", indent3, path);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}runOnlyForDeploymentPostprocessing = {1};", indent2, this.RunOnlyForDeploymentPostprocessing ? "1" : "0");
            text.AppendLine();
            text.AppendFormat("{0}shellPath = {1};", indent2, this.ShellPath);
            text.AppendLine();
            var scriptContent = this.GenerateScript(this.AssociatedTarget);
            text.AppendFormat("{0}shellScript = \"{1}\";", indent2, scriptContent);
            text.AppendLine();
            if (!this.ShowEnvironmentInLog)
            {
                text.AppendFormat("{0}showEnvVarsInLog = 0;", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }

    public sealed class CopyFilesBuildPhase :
        BuildPhase
    {
        public enum ESubFolderSpec
        {
            AbsolutePath = 0,
            Wrapper = 1,
            Executables = 6,
            Resources = 7,
            Frameworks = 10,
            SharedFrameworks = 11,
            SharedSupport = 12,
            Plugins = 13,
            JavaResources = 15,
            ProductsDirectory = 16
        }

        public CopyFilesBuildPhase(
            string name)
        {
            this.Name = name;
            this.IsA = "PBXCopyFilesBuildPhase";
            this.DestinationPath = string.Empty;
            this.SubFolderSpec = ESubFolderSpec.AbsolutePath;
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

        public string DestinationPath
        {
            get;
            private set;
        }

        public ESubFolderSpec SubFolderSpec
        {
            get;
            set;
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
            text.AppendFormat("{0}buildActionMask = {1};", indent2, this.BuildActionMask);
            text.AppendLine();
            text.AppendFormat("{0}dstPath = \"{1}\";", indent2, this.DestinationPath);
            text.AppendLine();
            text.AppendFormat("{0}dstSubfolderSpec = \"{1}\";", indent2, (int)this.SubFolderSpec);
            text.AppendLine();
            if (this.BuildFiles.Count > 0)
            {
                text.AppendFormat("{0}files = (", indent2);
                text.AppendLine();
                foreach (var file in this.BuildFiles)
                {
                    text.AppendFormat("{0}{1} /* in {2} */,", indent3, file.GUID, this.Name);
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

    public sealed class Project :
        Object
    {
        public Project(
            Bam.Core.Module module,
            string name)
        {
            this.IsA = "PBXProject";
            this.Name = name;
            var projectDir = Bam.Core.TokenizedString.Create("$(buildroot)/$(packagename).xcodeproj", module);
            module.Macros.Add("xcodeprojectdir", projectDir);
            this.ProjectDir = projectDir.Parse();

            var projectPath = Bam.Core.TokenizedString.Create("$(xcodeprojectdir)/project.pbxproj", module);
            this.ProjectPath = projectPath.Parse();

            this.SourceRoot = Bam.Core.TokenizedString.Create("$(pkgroot)", module).Parse();
            this.BuildRoot = Bam.Core.TokenizedString.Create("$(buildroot)", module).Parse();

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
            Target target)
        {
            // add configuration to target
            var config = new Configuration(module.BuildEnvironment.Configuration.ToString());
            config["PRODUCT_NAME"] = new UniqueConfigurationValue("$(TARGET_NAME)");

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

    public sealed class Target :
        Object
    {
        public enum EProductType
        {
            StaticLibrary,
            Executable,
            DynamicLibrary,
            ApplicationBundle
        }

        public Target(
            Bam.Core.Module module,
            Project project,
            FileReference fileRef,
            EProductType type)
        {
            this.IsA = "PBXNativeTarget";
            this.Name = module.GetType().Name;
            this.FileReference = fileRef;
            this.Type = type;

            var configList = new ConfigurationList(this);
            project.ConfigurationLists.Add(configList);
            this.ConfigurationList = configList;

            this.BuildPhases = new System.Collections.Generic.List<BuildPhase>();
            this.SourcesBuildPhase = new SourcesBuildPhase();
            this.BuildPhases.Add(this.SourcesBuildPhase);

            this.TargetDependencies = new Bam.Core.Array<TargetDependency>();

            this.Project = project;
            this.Project.SourcesBuildPhases.Add(this.SourcesBuildPhase);
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
            set;
        }

        public ShellScriptBuildPhase PostBuildBuildPhase
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

        public Bam.Core.Array<TargetDependency> TargetDependencies
        {
            get;
            private set;
        }

        public void
        SetCommonCompilationOptions(
            Bam.Core.Module module,
            Configuration configuration,
            Bam.Core.Settings settings)
        {
            (settings as XcodeProjectProcessor.V2.IConvertToProject).Convert(module, configuration);
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
            }

            throw new Bam.Core.Exception("Unrecognized product type");
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

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
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
            if (this.BuildPhases.Count > 0)
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
            text.AppendFormat("{0}productReference = {1} /* {2} */;", indent2, this.FileReference.GUID, this.FileReference.Name);
            text.AppendLine();
            text.AppendFormat("{0}productType = \"{1}\";", indent2, this.ProductTypeToString());
            text.AppendLine();
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }
    }

    public sealed class ConfigurationList :
        Object,
        System.Collections.Generic.IEnumerable<Configuration>
    {
        public ConfigurationList(Object parent)
        {
            this.IsA = "XCConfigurationList";
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

        public override void Serialize(System.Text.StringBuilder text, int indentLevel)
        {
            var indent = new string('\t', indentLevel);
            var indent2 = new string('\t', indentLevel + 1);
            var indent3 = new string('\t', indentLevel + 2);
            text.AppendFormat("{0}{1} /* Build configuration list for {2} \"{3}\" */ = {{", indent, this.GUID, this.Parent.IsA, this.Parent.Name);
            text.AppendLine();
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            if (this.Configurations.Count > 0)
            {
                text.AppendFormat("{0}buildConfigurations = (", indent2);
                text.AppendLine();
                foreach (var config in this.Configurations)
                {
                    text.AppendFormat("{0}{1} /* {2} */,", indent3, config.GUID, config.Name);
                    text.AppendLine();
                }
                text.AppendFormat("{0});", indent2);
                text.AppendLine();
            }
            text.AppendFormat("{0}}};", indent);
            text.AppendLine();
        }

        public System.Collections.Generic.IEnumerator<Configuration> GetEnumerator()
        {
            foreach (var config in this.Configurations)
            {
                yield return config;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return this.GetEnumerator();
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
            this.IsA = "XCBuildConfiguration";
            this.Name = name;
            this.PreBuildCommands = new Bam.Core.StringArray();
            this.PostBuildCommands = new Bam.Core.StringArray();
            this.BuildFiles = new Bam.Core.Array<BuildFile>();
        }

        public Bam.Core.StringArray PreBuildCommands
        {
            get;
            private set;
        }

        public Bam.Core.StringArray PostBuildCommands
        {
            get;
            private set;
        }

        public Bam.Core.Array<BuildFile> BuildFiles
        {
            get;
            private set;
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
            text.AppendFormat("{0}isa = {1};", indent2, this.IsA);
            text.AppendLine();
            text.AppendFormat("{0}buildSettings = {{", indent2);
            text.AppendLine();
            foreach (var setting in this.Settings.OrderBy(key => key.Key))
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
            this.Projects = new System.Collections.Generic.Dictionary<Bam.Core.PackageDefinitionFile, Project>();
        }

        private System.Collections.Generic.Dictionary<Bam.Core.PackageDefinitionFile, Project> Projects
        {
            get;
            set;
        }

        public Project
        FindOrCreateProject(
            Bam.Core.Module module,
            XcodeMeta.Type projectType)
        {
            lock(this)
            {
                // Note: if you want a Xcode project per module, change this from keying off of the package
                // to the module type
                var package = module.PackageDefinition;
                if (this.Projects.ContainsKey(package))
                {
                    var project = this.Projects[package];
                    project.AddNewProjectConfiguration(module);
                    return project;
                }
                else
                {
                    var project = new Project(module, package.Name);
                    this.Projects[package] = project;
                    return project;
                }
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

    public sealed class GenerateXcodeSchemes :
        Bam.Core.IBooleanCommandLineArgument
    {
        string Bam.Core.ICommandLineArgument.ShortName
        {
            get
            {
                return null;
            }
        }

        string Bam.Core.ICommandLineArgument.LongName
        {
            get
            {
                return "--Xcode.generateSchemes";
            }
        }

        string Bam.Core.ICommandLineArgument.ContextHelp
        {
            get
            {
                return "Enable the writing of user-specific scheme files, so that xcodebuild can be used on generated Xcode workspaces. Not needed if just the IDE is being used.";
            }
        }
    }

    public abstract class XcodeMeta
    {
        public enum Type
        {
            NA,
            StaticLibrary,
            Application,
            DynamicLibrary
        }

        protected XcodeMeta(Bam.Core.Module module, Type type)
        {
            var graph = Bam.Core.Graph.Instance;
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

        public Bam.Core.Module ProjectModule
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
            Bam.Core.Graph.Instance.MetaData = new WorkspaceMeta();
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

            var workspaceMeta = Bam.Core.Graph.Instance.MetaData as WorkspaceMeta;

            var workspaceContents = new System.Xml.XmlDocument();
            var workspace = workspaceContents.CreateElement("Workspace");
            workspace.Attributes.Append(workspaceContents.CreateAttribute("version")).Value = "1.0";
            workspaceContents.AppendChild(workspace);

            var generateProjectSchemes = Bam.Core.CommandLineProcessor.Evaluate(new GenerateXcodeSchemes());

            foreach (var project in workspaceMeta)
            {
                project.FixupPerConfigurationData();

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

                var projectDir = project.ProjectDir;
                if (!System.IO.Directory.Exists(projectDir))
                {
                    System.IO.Directory.CreateDirectory(projectDir);
                }

                //Bam.Core.Log.DebugMessage(text.ToString());
                using (var writer = new System.IO.StreamWriter(project.ProjectPath))
                {
                    writer.Write(text.ToString());
                }

                if (generateProjectSchemes)
                {
                    var projectSchemeCache = new ProjectSchemeCache(project);
                    projectSchemeCache.Serialize();
                }

                var workspaceFileRef = workspaceContents.CreateElement("FileRef");
                workspaceFileRef.Attributes.Append(workspaceContents.CreateAttribute("location")).Value = System.String.Format("group:{0}", projectDir);
                workspace.AppendChild(workspaceFileRef);
            }

            var workspacePath = Bam.Core.TokenizedString.Create("$(buildroot)/$(masterpackagename).xcworkspace/contents.xcworkspacedata", null);
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

            Bam.Core.Log.Info("Successfully created Xcode workspace for package '{0}'\n\t{1}", Bam.Core.Graph.Instance.MasterPackage.Name, workspaceDir);
        }
    }

    public sealed class XcodeHeaderFile :
        XcodeMeta
    {
        public XcodeHeaderFile(Bam.Core.Module module) :
            base(module, Type.NA)
        {
        }

        public FileReference Source
        {
            get;
            set;
        }
    }

    public sealed class XcodeObjectFile :
        XcodeMeta
    {
        public XcodeObjectFile(Bam.Core.Module module) :
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

    public abstract class XcodeCommonProject :
        XcodeMeta
    {
        public XcodeCommonProject(
            Bam.Core.Module module,
            Bam.Core.TokenizedString libraryPath,
            XcodeMeta.Type type) :
            base(module, type)
        {
        }

        protected void
        PullInProjectPreOrPostBuildSteps()
        {
            var postBuildCommands = this.Project.ProjectConfigurations[this.ProjectModule.BuildEnvironment.Configuration].PostBuildCommands;
            if (postBuildCommands.Count > 0)
            {
                this.AddPostBuildCommands(postBuildCommands);
            }
            var preBuildCommands = this.Project.ProjectConfigurations[this.ProjectModule.BuildEnvironment.Configuration].PreBuildCommands;
            if (preBuildCommands.Count > 0)
            {
                this.AddPreBuildCommands(preBuildCommands);
            }
        }

        public void
        AddHeader(
            FileReference header)
        {
            this.Project.HeaderFilesGroup.AddReference(header);
        }

        public void AddSource(Bam.Core.Module module, FileReference source, BuildFile output, Bam.Core.Settings patchSettings)
        {
            if (null != patchSettings)
            {
                var commandLine = new Bam.Core.StringArray();
                (patchSettings as CommandLineProcessor.V2.IConvertToCommandLine).Convert(module, commandLine);
                output.Settings = commandLine;
            }
            this.Target.SourcesBuildPhase.AddBuildFile(output); // this is shared among configurations
            this.Project.SourceFilesGroup.AddReference(source);
            this.Configuration.BuildFiles.Add(output);
        }

        public void SetCommonCompilationOptions(Bam.Core.Module module, Bam.Core.Settings settings)
        {
            this.Target.SetCommonCompilationOptions(module, this.Configuration, settings);
        }

        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands)
        {
            if (null == this.Target.PreBuildBuildPhase)
            {
                var preBuildBuildPhase = new ShellScriptBuildPhase(this.Target, "Pre Build", (target) =>
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
                this.Target.PreBuildBuildPhase = preBuildBuildPhase;
                // do not add PreBuildBuildPhase to this.Target.BuildPhases, so that it can be serialized in the right order
            }

            this.Configuration.PreBuildCommands.AddRange(commands);
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands)
        {
            if (null == this.Target.PostBuildBuildPhase)
            {
                var postBuildBuildPhase = new ShellScriptBuildPhase(this.Target, "Post Build", (target) =>
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
                this.Target.PostBuildBuildPhase = postBuildBuildPhase;
                // do not add PostBuildBuildPhase to this.Target.BuildPhases, so that it can be serialized in the right order
            }

            this.Configuration.PostBuildCommands.AddRange(commands);
        }

        public FileReference Output
        {
            get;
            protected set;
        }

        public void
        RequiresProject(
            XcodeCommonProject dependent)
        {
            var proxy = new ContainerItemProxy(this.Project, dependent.Target);
            var dependency = new TargetDependency(dependent.Target, proxy);
            this.Target.TargetDependencies.AddUnique(dependency);
        }
    }

    public sealed class XcodeHeaderLibrary :
        XcodeCommonProject
    {
        public XcodeHeaderLibrary(
            Bam.Core.Module module) :
            base(module, null, Type.NA)
        {
            this.PullInProjectPreOrPostBuildSteps();
        }
    }

    public sealed class XcodeStaticLibrary :
        XcodeCommonProject
    {
        public XcodeStaticLibrary(
            Bam.Core.Module module,
            Bam.Core.TokenizedString libraryPath) :
            base(module, libraryPath, Type.StaticLibrary)
        {
            var library = this.Project.FindOrCreateFileReference(
                libraryPath,
                FileReference.EFileType.Archive,
                explicitType:true,
                sourceTree:FileReference.ESourceTree.BuiltProductsDir);
            this.Output = library;
            this.Project.ProductRefGroup.AddReference(library);
            this.Target = this.Project.FindOrCreateTarget(module, library, V2.Target.EProductType.StaticLibrary);
            this.Configuration = this.Project.AddNewTargetConfiguration(module, this.Target);
            this.PullInProjectPreOrPostBuildSteps();
        }
    }

    public abstract class XcodeCommonLinkable :
        XcodeCommonProject
    {
        public XcodeCommonLinkable(
            Bam.Core.Module module,
            Bam.Core.TokenizedString libraryPath,
            XcodeMeta.Type type) :
            base(module, libraryPath, type)
        {
        }

        public void
        EnsureFrameworksBuildPhaseExists()
        {
            if (null == this.Target.FrameworksBuildPhase)
            {
                var frameworks = new FrameworksBuildPhase();
                this.Project.FrameworksBuildPhases.Add(frameworks);
                this.Target.BuildPhases.Add(frameworks);
                this.Target.FrameworksBuildPhase = frameworks;
            }
        }

        public void
        AddStaticLibrary(
            XcodeStaticLibrary library)
        {
            this.EnsureFrameworksBuildPhaseExists();
            var copyOfLibFileRef = FileReference.MakeLinkedClone(this.Project, this.ProjectModule.BuildEnvironment.Configuration, library.Output);
            if (null != copyOfLibFileRef)
            {
                this.Project.MainGroup.AddReference(copyOfLibFileRef); // TODO: structure later
            }
            else
            {
                copyOfLibFileRef = library.Output;
            }
            var libraryBuildFile = this.Project.FindOrCreateBuildFile(library.Output.Path, copyOfLibFileRef);
            this.Target.FrameworksBuildPhase.AddBuildFile(libraryBuildFile);
        }

        public void
        AddDynamicLibrary(
            XcodeDynamicLibrary library)
        {
            this.EnsureFrameworksBuildPhaseExists();
            var copyOfLibFileRef = FileReference.MakeLinkedClone(this.Project, this.ProjectModule.BuildEnvironment.Configuration, library.Output);
            if (null != copyOfLibFileRef)
            {
                this.Project.MainGroup.AddReference(copyOfLibFileRef); // TODO: structure later
            }
            else
            {
                copyOfLibFileRef = library.Output;
            }
            var libraryBuildFile = this.Project.FindOrCreateBuildFile(library.Output.Path, copyOfLibFileRef);
            this.Target.FrameworksBuildPhase.AddBuildFile(libraryBuildFile);
        }
    }

    public sealed class XcodeProgram :
        XcodeCommonLinkable
    {
        public XcodeProgram(
            Bam.Core.Module module,
            Bam.Core.TokenizedString executablePath) :
            base(module, executablePath, Type.Application)
        {
            var application = this.Project.FindOrCreateFileReference(
                executablePath,
                FileReference.EFileType.Executable,
                explicitType:true,
                sourceTree:FileReference.ESourceTree.BuiltProductsDir);
            this.Output = application;
            this.Project.ProductRefGroup.AddReference(application);
            this.Target = this.Project.FindOrCreateTarget(module, application, V2.Target.EProductType.Executable);
            this.Configuration = this.Project.AddNewTargetConfiguration(module, this.Target);
            this.PullInProjectPreOrPostBuildSteps();
        }
    }

    public sealed class XcodeDynamicLibrary :
        XcodeCommonLinkable
    {
        public XcodeDynamicLibrary(
            Bam.Core.Module module,
            Bam.Core.TokenizedString libraryPath) :
            base(module, libraryPath, Type.DynamicLibrary)
        {
            var dynamicLibrary = this.Project.FindOrCreateFileReference(
                libraryPath,
                FileReference.EFileType.DynamicLibrary,
                explicitType:true,
                sourceTree:FileReference.ESourceTree.BuiltProductsDir);
            this.Output = dynamicLibrary;
            this.Project.ProductRefGroup.AddReference(dynamicLibrary);
            this.Target = this.Project.FindOrCreateTarget(module, dynamicLibrary, V2.Target.EProductType.DynamicLibrary);
            this.Configuration = this.Project.AddNewTargetConfiguration(module, this.Target);
            this.PullInProjectPreOrPostBuildSteps();
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

    public sealed class ProjectSchemeCache
    {
        public
        ProjectSchemeCache(
            Project project)
        {
            this.Project = project;
            this.SchemeDocuments = new System.Collections.Generic.Dictionary<string, System.Xml.XmlDocument>();

            foreach (var target in project.Targets)
            {
                var schemeFilename = System.String.Format("{0}.xcscheme", target.Value.Name);
                var schemePathname = System.String.Format("{0}/xcuserdata/{1}.xcuserdatad/xcschemes/{2}",
                        project.ProjectDir,
                        System.Environment.GetEnvironmentVariable("USER"),
                        schemeFilename);
                var doc = this.CreateSchemePlist(target.Value);
                this.SchemeDocuments.Add(schemePathname, doc);
            }
            this.CreateManagementPlist();
        }

        private void
        CreateBuildActionEntry(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement buildActionEntriesEl,
            Target target,
            Bam.Core.Array<Target> buildActionsCreated)
        {
#if true
#else
            // add all required dependencies in first (order matters)
            foreach (var required in target.TargetDependencies)
            {
                this.CreateBuildActionEntry(doc, buildActionEntriesEl, required., buildActionsCreated);
            }
#endif

            // the same target might appear again while iterating through the required targets of dependencies
            // only add it once (first one is important for ordering)
            if (buildActionsCreated.Contains(target))
            {
                return;
            }

            var buildActionEntry = doc.CreateElement("BuildActionEntry");
            buildActionEntriesEl.AppendChild(buildActionEntry);
            var buildableReference = doc.CreateElement("BuildableReference");
            buildActionEntry.AppendChild(buildableReference);
            {
                var buildable = doc.CreateAttribute("BuildableIdentifier");
                buildable.Value = "primary";
                var blueprint = doc.CreateAttribute("BlueprintIdentifier");
                blueprint.Value = target.GUID;
                var buildableName = doc.CreateAttribute("BuildableName");
                buildableName.Value = target.FileReference.Name;
                var blueprintName = doc.CreateAttribute("BlueprintName");
                blueprintName.Value = target.Name;
                var refContainer = doc.CreateAttribute("ReferencedContainer");
                if (target.Project.ProjectDir == this.Project.ProjectDir)
                {
                    refContainer.Value = "container:" + System.IO.Path.GetFileName(this.Project.ProjectDir);
                }
                else
                {
                    var relative = Bam.Core.RelativePathUtilities.GetPath(target.Project.ProjectDir, this.Project.ProjectDir);
                    refContainer.Value = "container:" + relative;
                }
                buildableReference.Attributes.Append(buildable);
                buildableReference.Attributes.Append(blueprint);
                buildableReference.Attributes.Append(buildableName);
                buildableReference.Attributes.Append(blueprintName);
                buildableReference.Attributes.Append(refContainer);
            }

            buildActionsCreated.Add(target);
        }

        private void
        CreateBuildActions(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement schemeElement,
            Target target)
        {
            var buildActionEl = doc.CreateElement("BuildAction");
            schemeElement.AppendChild(buildActionEl);
            {
                var parallelBuildsAttr = doc.CreateAttribute("parallelizeBuildables");
                parallelBuildsAttr.Value = "YES";
                buildActionEl.Attributes.Append(parallelBuildsAttr);

                var buildImplicitDepsAttr = doc.CreateAttribute("buildImplicitDependencies");
                buildImplicitDepsAttr.Value = "YES";
                buildActionEl.Attributes.Append(buildImplicitDepsAttr);

                var buildActionEntries = doc.CreateElement("BuildActionEntries");
                buildActionEl.AppendChild(buildActionEntries);

                var buildActionsCreated = new Bam.Core.Array<Target>();
                this.CreateBuildActionEntry(doc, buildActionEntries, target, buildActionsCreated);
            }
        }

        private void
        CreateTestActions(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement schemeElement,
            Target target)
        {
            var testActionEl = doc.CreateElement("TestAction");
            schemeElement.AppendChild(testActionEl);
            {
                var selectedDebuggerAttr = doc.CreateAttribute("selectedDebuggerIdentifier");
                selectedDebuggerAttr.Value = "Xcode.DebuggerFoundation.Debugger.LLDB";
                testActionEl.Attributes.Append(selectedDebuggerAttr);

                var selectedLauncherAttr = doc.CreateAttribute("selectedLauncherIdentifier");
                selectedLauncherAttr.Value = "Xcode.DebuggerFoundation.Launcher.LLDB";
                testActionEl.Attributes.Append(selectedLauncherAttr);

                var useLaunchSchemeArgsAttr = doc.CreateAttribute("shouldUseLaunchSchemeArgsEnv");
                useLaunchSchemeArgsAttr.Value = "YES";
                testActionEl.Attributes.Append(useLaunchSchemeArgsAttr);

                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.ConfigurationList.ElementAt(0).Name;
                testActionEl.Attributes.Append(buildConfigurationAttr);
            }
        }

        private void
        CreateLaunchActions(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement schemeElement,
            Target target)
        {
            var launchActionEl = doc.CreateElement("LaunchAction");
            schemeElement.AppendChild(launchActionEl);
            {
                var selectedDebuggerAttr = doc.CreateAttribute("selectedDebuggerIdentifier");
                selectedDebuggerAttr.Value = "Xcode.DebuggerFoundation.Debugger.LLDB";
                launchActionEl.Attributes.Append(selectedDebuggerAttr);

                var selectedLauncherAttr = doc.CreateAttribute("selectedLauncherIdentifier");
                selectedLauncherAttr.Value = "Xcode.DebuggerFoundation.Launcher.LLDB";
                launchActionEl.Attributes.Append(selectedLauncherAttr);

                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.ConfigurationList.ElementAt(0).Name;
                launchActionEl.Attributes.Append(buildConfigurationAttr);

                var productRunnableEl = doc.CreateElement("BuildableProductRunnable");
                launchActionEl.AppendChild(productRunnableEl);

                var buildableRefEl = doc.CreateElement("BuildableReference");
                productRunnableEl.AppendChild(buildableRefEl);

                var buildableIdAttr = doc.CreateAttribute("BuildableIdentifier");
                buildableIdAttr.Value = "primary";
                buildableRefEl.Attributes.Append(buildableIdAttr);

                var blueprintIdAttr = doc.CreateAttribute("BlueprintIdentifier");
                blueprintIdAttr.Value = target.FileReference.GUID;
                buildableRefEl.Attributes.Append(blueprintIdAttr);

                var buildableNameAttr = doc.CreateAttribute("BuildableName");
                buildableNameAttr.Value = target.FileReference.Name;
                buildableRefEl.Attributes.Append(buildableNameAttr);

                var blueprintNameAttr = doc.CreateAttribute("BlueprintName");
                blueprintNameAttr.Value = target.FileReference.Name;
                buildableRefEl.Attributes.Append(blueprintNameAttr);

                var refContainerAttr = doc.CreateAttribute("ReferencedContainer");
                if (target.Project.ProjectDir == this.Project.ProjectDir)
                {
                    refContainerAttr.Value = "container:" + System.IO.Path.GetFileName(target.Project.ProjectDir);
                }
                else
                {
                    var relative = Bam.Core.RelativePathUtilities.GetPath(target.Project.ProjectDir, this.Project.ProjectDir);
                    refContainerAttr.Value = "container:" + relative;
                }
                buildableRefEl.Attributes.Append(refContainerAttr);
            }
        }

        private System.Xml.XmlDocument
        CreateSchemePlist(
            Target target)
        {
            var doc = new System.Xml.XmlDocument();

            var schemeEl = doc.CreateElement("Scheme");
            doc.AppendChild(schemeEl);

            this.CreateBuildActions(doc, schemeEl, target);
            this.CreateTestActions(doc, schemeEl, target);
            this.CreateLaunchActions(doc, schemeEl, target);

            var profileActionEl = doc.CreateElement("ProfileAction");
            schemeEl.AppendChild(profileActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.ConfigurationList.ElementAt(0).Name;
                profileActionEl.Attributes.Append(buildConfigurationAttr);
            }

            var analyzeActionEl = doc.CreateElement("AnalyzeAction");
            schemeEl.AppendChild(analyzeActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.ConfigurationList.ElementAt(0).Name;
                analyzeActionEl.Attributes.Append(buildConfigurationAttr);
            }

            var archiveActionEl = doc.CreateElement("ArchiveAction");
            schemeEl.AppendChild(archiveActionEl);
            {
                var buildConfigurationAttr = doc.CreateAttribute("buildConfiguration");
                buildConfigurationAttr.Value = target.ConfigurationList.ElementAt(0).Name;
                archiveActionEl.Attributes.Append(buildConfigurationAttr);
            }

            return doc;
        }

        private void
        CreateManagementPlist()
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

            {
                var key = doc.CreateElement("key");
                key.InnerText = "SchemeUserState";
                dictEl.AppendChild(key);

                var valueDict = doc.CreateElement("dict");
                dictEl.AppendChild(valueDict);

                foreach (var scheme in this.SchemeDocuments)
                {
                    var schemeKey = doc.CreateElement("key");
                    schemeKey.InnerText = System.IO.Path.GetFileName(scheme.Key);
                    valueDict.AppendChild(schemeKey);
                }
            }

            this.ManagementDocument = doc;
        }

        private System.Collections.Generic.Dictionary<string, System.Xml.XmlDocument> SchemeDocuments
        {
            get;
            set;
        }

        private System.Xml.XmlDocument ManagementDocument
        {
            get;
            set;
        }

        private Project Project
        {
            get;
            set;
        }

        private void
        Write(
            System.Xml.XmlDocument document,
            string path)
        {
            // do not write a Byte-Ordering-Mark (BOM)
            var encoding = new System.Text.UTF8Encoding(false);

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            using (var writer = new System.IO.StreamWriter(path, false, encoding))
            {
                var settings = new System.Xml.XmlWriterSettings();
                settings.OmitXmlDeclaration = false;
                settings.NewLineChars = "\n";
                settings.Indent = true;
                settings.IndentChars = "   ";
                settings.NewLineOnAttributes = true;
                using (var xmlWriter = System.Xml.XmlWriter.Create(writer, settings))
                {
                    document.WriteTo(xmlWriter);
                    xmlWriter.WriteWhitespace(settings.NewLineChars);
                }
            }
        }

        public void
        Serialize()
        {
            foreach (var scheme in this.SchemeDocuments)
            {
                this.Write(scheme.Value, scheme.Key);
            }

            var schemePathname = System.String.Format("{0}/xcuserdata/{1}.xcuserdatad/xcschemes/xcschememanagement.plist",
                this.Project.ProjectDir,
                System.Environment.GetEnvironmentVariable("USER"));
            this.Write(this.ManagementDocument, schemePathname);
        }
    }
}
}

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
[assembly: Bam.Core.DeclareBuilder("VSSolution", typeof(VSSolutionBuilder.VSSolutionBuilder))]

namespace VSSolutionBuilder
{
namespace V2
{
#if true
    static class XmlDocumentExtensions
    {
        public static System.Xml.XmlElement
        CreateVSElement(
            this System.Xml.XmlDocument document,
            string name,
            string value = null,
            string condition = null,
            System.Xml.XmlElement parentEl = null)
        {
            const string ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var el = document.CreateElement(name, ns);
            if (null != value)
            {
                el.InnerText = value;
            }
            if (null != condition)
            {
                el.SetAttribute("Condition", condition);
            }
            if (null != parentEl)
            {
                parentEl.AppendChild(el);
            }
            return el;
        }

        public static System.Xml.XmlElement
        CreateVSItemGroup(
            this System.Xml.XmlDocument document,
            string label = null,
            System.Xml.XmlElement parentEl = null)
        {
            var itemGroup = document.CreateVSElement("ItemGroup", parentEl: parentEl);
            if (null != label)
            {
                itemGroup.SetAttribute("Label", label);
            }
            return itemGroup;
        }

        public static System.Xml.XmlElement
        CreateVSPropertyGroup(
            this System.Xml.XmlDocument document,
            string label = null,
            string condition = null,
            System.Xml.XmlElement parentEl = null)
        {
            var propertyGroup = document.CreateVSElement("PropertyGroup", condition: condition, parentEl: parentEl);
            if (null != label)
            {
                propertyGroup.SetAttribute("Label", label);
            }
            return propertyGroup;
        }

        public static System.Xml.XmlElement
        CreateVSItemDefinitionGroup(
            this System.Xml.XmlDocument document,
            string condition = null,
            System.Xml.XmlElement parentEl = null)
        {
            return document.CreateVSElement("ItemDefinitionGroup", condition: condition, parentEl: parentEl);
        }

        public static System.Xml.XmlElement
        CreateVSImport(
            this System.Xml.XmlDocument document,
            string importPath,
            System.Xml.XmlElement parentEl = null)
        {
            var import = document.CreateVSElement("Import", parentEl: parentEl);
            import.SetAttribute("Project", importPath);
            return import;
        }
    }

    public sealed class VSSetting
    {
        public VSSetting(
            string name,
            string value,
            string condition = null)
        {
            this.Name = name;
            this.Value = value;
            this.Condition = condition;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Value
        {
            get;
            private set;
        }

        public string Condition
        {
            get;
            private set;
        }
    }

    public sealed class VSSettingsGroup
    {
        public enum ESettingsGroup
        {
            Compiler,
            Header,
            Librarian,
            Linker,
            PreBuild,
            PostBuild,
            CustomBuild
        }

        public VSSettingsGroup(
            ESettingsGroup group,
            Bam.Core.V2.TokenizedString include = null)
        {
            this.Group = group;
            this.Include = include;
            this.Settings = new Bam.Core.Array<VSSetting>();
        }

        public ESettingsGroup Group
        {
            get;
            private set;
        }

        public Bam.Core.V2.TokenizedString Include
        {
            get;
            private set;
        }

        private Bam.Core.Array<VSSetting> Settings
        {
            get;
            set;
        }

        public void
        AddSetting(
            string name,
            bool value,
            string condition = null)
        {
            var stringValue = value.ToString().ToLower();
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition && item.Value != stringValue))
            {
                throw new Bam.Core.Exception("Cannot change the value of existing boolean option {0} to {1}", name, value);
            }

            this.Settings.AddUnique(new VSSetting(name, stringValue, condition));
        }

        public void
        AddSetting(
            string name,
            string value,
            string condition = null)
        {
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition && item.Value != value))
            {
                throw new Bam.Core.Exception("Cannot change the value of existing string option {0} to {1}", name, value);
            }

            this.Settings.AddUnique(new VSSetting(name, value, condition));
        }

        public void
        AddSetting(
            string name,
            Bam.Core.V2.TokenizedString path,
            string condition = null,
            bool inheritExisting = false)
        {
            var stringValue = path.Parse();
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition && item.Value != stringValue))
            {
                throw new Bam.Core.Exception("Cannot change the value of existing tokenized path option {0} to {1}", name, stringValue);
            }

            this.Settings.AddUnique(new VSSetting(name, stringValue, condition));
        }

        public void
        AddSetting(
            string name,
            Bam.Core.Array<Bam.Core.V2.TokenizedString> value,
            string condition = null,
            bool inheritExisting = false)
        {
            if (0 == value.Count)
            {
                return;
            }
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition))
            {
                throw new Bam.Core.Exception("Cannot append to the option {0}", name);
            }

            var linearized = value.ToString(';');
            this.Settings.AddUnique(new VSSetting(name, inheritExisting ? System.String.Format("{0};%({1})", linearized, name) : linearized, condition));
        }

        public void
        AddSetting(
            string name,
            Bam.Core.StringArray value,
            string condition = null,
            bool inheritExisting = false)
        {
            if (0 == value.Count)
            {
                return;
            }
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition))
            {
                throw new Bam.Core.Exception("Cannot append to the option {0}", name);
            }

            var linearized = value.ToString(';');
            this.Settings.AddUnique(new VSSetting(name, inheritExisting ? System.String.Format("{0};%({1})", linearized, name) : linearized, condition));
        }

        public void
        AddSetting(
            string name,
            C.V2.PreprocessorDefinitions definitions,
            string condition = null,
            bool inheritExisting = false)
        {
            if (this.Settings.Any(item => item.Name == name && item.Condition == condition))
            {
                throw new Bam.Core.Exception("Cannot append to the preprocessor define list {0}", name);
            }

            var linearized = definitions.ToString();
            this.Settings.AddUnique(new VSSetting(name, inheritExisting ? System.String.Format("{0}%({1})", linearized, name) : linearized, condition));
        }

        private string
        GetGroupName()
        {
            switch (this.Group)
            {
                case ESettingsGroup.Compiler:
                    return "ClCompile";

                case ESettingsGroup.Header:
                    return "ClInclude";

                case ESettingsGroup.Librarian:
                    return "Lib";

                case ESettingsGroup.Linker:
                    return "Link";

                case ESettingsGroup.PreBuild:
                    return "PreBuildEvent";

                case ESettingsGroup.PostBuild:
                    return "PostBuildEvent";

                case ESettingsGroup.CustomBuild:
                    return "CustomBuild";

                default:
                    throw new Bam.Core.Exception("Unknown settings group, {0}", this.Group.ToString());
            }
        }

        public void
        Serialize(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentEl)
        {
            if ((this.Settings.Count == 0) && (this.Include == null))
            {
                return;
            }
            var group = document.CreateVSElement(this.GetGroupName(), parentEl: parentEl);
            if (null != this.Include)
            {
                group.SetAttribute("Include", this.Include.Parse());
            }
            foreach (var setting in this.Settings.OrderBy(pair => pair.Name))
            {
                document.CreateVSElement(setting.Name, value: setting.Value, condition: setting.Condition, parentEl: group);
            }
        }
    }

    // abstraction of a project configuration, consisting of platform and configuration
    public sealed class VSProjectConfiguration
    {
        public enum EType
        {
            NA,
            Application,
            DynamicLibrary,
            StaticLibrary,
            Utility
        }

        public enum EPlatformToolset
        {
            NA,
            v120
        }

        public enum ECharacterSet
        {
            NotSet,
            Unicode,
            Multibyte
        }

        public VSProjectConfiguration(
            VSProject project,
            Bam.Core.V2.Module module,
            Bam.Core.EPlatform platform)
        {
            this.Project = project;
            this.Module = module;
            this.Configuration = module.BuildEnvironment.Configuration;
            this.Platform = platform;
            this.FullName = this.CombinedName;

            this.Type = EType.NA;
            this.PlatformToolset = EPlatformToolset.NA;
            this.UseDebugLibraries = false;
            this.CharacterSet = ECharacterSet.NotSet;
            this.WholeProgramOptimization = (module.BuildEnvironment.Configuration != Bam.Core.EConfiguration.Debug);

            this.SettingGroups = new Bam.Core.Array<VSSettingsGroup>();
            this.Sources = new Bam.Core.Array<VSSettingsGroup>();

            this.PreBuildCommands = new Bam.Core.StringArray();
            this.PostBuildCommands = new Bam.Core.StringArray();
        }

        public string
        ConfigurationName
        {
            get
            {
                return this.Configuration.ToString();
            }
        }

        public string
        PlatformName
        {
            get
            {
                switch (this.Platform)
                {
                    case Bam.Core.EPlatform.Win32:
                        return "Win32";

                    case Bam.Core.EPlatform.Win64:
                        return "x64";

                    default:
                        throw new Bam.Core.Exception("Only Win32 and Win64 are supported platforms for VisualStudio projects");
                }
            }
        }

        private string
        CombinedName
        {
            get
            {
                return System.String.Format("{0}|{1}", this.ConfigurationName, this.PlatformName);
            }
        }

        public string
        ConditionText
        {
            get
            {
                return System.String.Format("'$(Configuration)|$(Platform)'=='{0}'", this.CombinedName);
            }
        }

        public Bam.Core.V2.Module Module
        {
            get;
            private set;
        }

        public string FullName
        {
            get;
            private set;
        }

        public Bam.Core.EConfiguration Configuration
        {
            get;
            private set;
        }

        public Bam.Core.EPlatform Platform
        {
            get;
            private set;
        }

        public VSProject Project
        {
            get;
            private set;
        }

        public EType Type
        {
            get;
            private set;
        }

        public EPlatformToolset PlatformToolset
        {
            get;
            private set;
        }

        private bool UseDebugLibraries
        {
            get;
            set;
        }

        private ECharacterSet CharacterSet
        {
            get;
            set;
        }

        private bool WholeProgramOptimization
        {
            get;
            set;
        }

        private string OutputDirectory
        {
            get;
            set;
        }

        private string IntermediateDirectory
        {
            get;
            set;
        }

        private string TargetName
        {
            get;
            set;
        }

        private Bam.Core.Array<VSSettingsGroup> SettingGroups
        {
            get;
            set;
        }

        private Bam.Core.Array<VSSettingsGroup> Sources
        {
            get;
            set;
        }

        private Bam.Core.StringArray PreBuildCommands
        {
            get;
            set;
        }

        private Bam.Core.StringArray PostBuildCommands
        {
            get;
            set;
        }

        public void
        SetType(
            EType type)
        {
            if (this.Type != EType.NA)
            {
                throw new Bam.Core.Exception("Project configuration already has type {0}. Cannot change it to {1}", this.Type.ToString(), type.ToString());
            }

            this.Type = type;
        }

        public void
        SetPlatformToolset(
            EPlatformToolset toolset)
        {
            if (this.PlatformToolset != EPlatformToolset.NA)
            {
                throw new Bam.Core.Exception("Project configuration already has platform toolset of {0}. Cannot change it to {1}", this.PlatformToolset.ToString(), toolset.ToString());
            }

            this.PlatformToolset = toolset;
        }

        public void
        SetOutputPath(
            Bam.Core.V2.TokenizedString path)
        {
            var macros = new Bam.Core.V2.MacroList();
            // TODO: ideally, $(ProjectDir) should replace the following directory separator as well,
            // but it does not seem to be a show stopper if it doesn't
            macros.Add("pkgbuilddir", Bam.Core.V2.TokenizedString.Create("$(ProjectDir)", null, verbatim: true));
            macros.Add("modulename", Bam.Core.V2.TokenizedString.Create("$(ProjectName)", null, verbatim: true));
            var outDir = path.Parse(macros);
            outDir = System.IO.Path.GetDirectoryName(outDir);
            outDir += "\\";
            this.OutputDirectory = outDir;

            // does the target name differ?
            var outputName = this.Module.Macros["OutputName"].Parse();
            var moduleName = this.Module.Macros["modulename"].Parse();
            if (outputName != moduleName)
            {
                this.TargetName = outputName;
            }
        }

        public void
        EnableIntermediatePath()
        {
            this.IntermediateDirectory = @"$(ProjectDir)\$(ProjectName)\$(Configuration)\";
        }

        public VSSettingsGroup
        GetSettingsGroup(
            VSSettingsGroup.ESettingsGroup group,
            Bam.Core.V2.TokenizedString include = null,
            bool uniqueToProject = false)
        {
            foreach (var settings in this.SettingGroups)
            {
                if (null == include)
                {
                    if ((null == settings.Include) && (settings.Group == group))
                    {
                        return settings;
                    }
                }
                else
                {
                    // ignore group, as files can mutate between them during the buildprocess (e.g. headers into custom builds)
                    if ((null != include) && (settings.Include.Parse() == include.Parse()))
                    {
                        return settings;
                    }
                }
            }

            var newGroup = uniqueToProject ? this.Project.GetUniqueSettingsGroup(group, include) : new VSSettingsGroup(group, include);
            this.SettingGroups.Add(newGroup);
            return newGroup;
        }

        public void
        AddHeaderFile(
            C.V2.HeaderFile header)
        {
            var headerGroup = this.Project.GetUniqueSettingsGroup(VSSettingsGroup.ESettingsGroup.Header, header.InputPath);
            this.Project.AddHeader(headerGroup);
        }

        public void
        AddSourceFile(
            Bam.Core.V2.Module module,
            Bam.Core.V2.Settings patchSettings)
        {
            var settings = module.MetaData as VSSettingsGroup;
            if (null != patchSettings)
            {
                (module.Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, settings, condition: this.ConditionText);
            }
            this.Sources.AddUnique(settings);
            this.Project.AddSource(settings);
        }

        public bool
        ContainsSource(
            VSSettingsGroup sourceGroup)
        {
            return this.Sources.Contains(sourceGroup);
        }

        public void
        AddPreBuildCommand(
            string command)
        {
            this.PreBuildCommands.Add(command);
        }

        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands)
        {
            this.PreBuildCommands.AddRange(commands);
        }

        public void
        AddPostBuildCommand(
            string command)
        {
            this.PostBuildCommands.Add(command);
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands)
        {
            this.PostBuildCommands.AddRange(commands);
        }

        public void
        SerializeProperties(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentEl)
        {
            if (this.Type == EType.NA)
            {
                Bam.Core.Log.DebugMessage("Defaulting project {0} to type Utility", this.Project.ProjectPath);
                this.Type = EType.Utility;
                this.EnableIntermediatePath();
            }
            else
            {
                if (this.PlatformToolset == EPlatformToolset.NA)
                {
                    throw new Bam.Core.Exception("Platform toolset not set for project {0}", this.Project.ProjectPath);
                }
            }
            var propGroup = document.CreateVSPropertyGroup(label: "Configuration", condition: this.ConditionText, parentEl: parentEl);
            document.CreateVSElement("ConfigurationType", value: this.Type.ToString(), parentEl: propGroup);
            if (this.PlatformToolset != EPlatformToolset.NA)
            {
                document.CreateVSElement("PlatformToolset", value: this.PlatformToolset.ToString(), parentEl: propGroup);
            }
            document.CreateVSElement("UseDebugLibraries", value: this.UseDebugLibraries.ToString().ToLower(), parentEl: propGroup);
            document.CreateVSElement("CharacterSet", value: this.CharacterSet.ToString(), parentEl: propGroup);
            document.CreateVSElement("WholeProgramOptimization", value: this.WholeProgramOptimization.ToString().ToLower(), parentEl: propGroup);
        }

        public void
        SerializePaths(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentEl)
        {
            var propGroup = document.CreateVSPropertyGroup(condition: this.ConditionText, parentEl: parentEl);
            if (null != this.OutputDirectory)
            {
                document.CreateVSElement("OutDir", value: this.OutputDirectory, parentEl: propGroup);
            }
            if (null != this.IntermediateDirectory)
            {
                document.CreateVSElement("IntDir", value: this.IntermediateDirectory, parentEl: propGroup);
            }
            if (null != this.TargetName)
            {
                document.CreateVSElement("TargetName", value: this.TargetName, parentEl: propGroup);
            }
        }

        public void
        SerializeSettings(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentEl)
        {
            var itemDefnGroup = document.CreateVSItemDefinitionGroup(condition: this.ConditionText, parentEl: parentEl);
            foreach (var group in this.SettingGroups)
            {
                if (group.Include != null)
                {
                    continue;
                }
                group.Serialize(document, itemDefnGroup);
            }

            if (this.PreBuildCommands.Count > 0)
            {
                var preBuildGroup = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.PreBuild);
                preBuildGroup.AddSetting("Command", this.PreBuildCommands.ToString(System.Environment.NewLine));
                preBuildGroup.Serialize(document, itemDefnGroup);
            }
            if (this.PostBuildCommands.Count > 0)
            {
                var preBuildGroup = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.PostBuild);
                preBuildGroup.AddSetting("Command", this.PostBuildCommands.ToString(System.Environment.NewLine));
                preBuildGroup.Serialize(document, itemDefnGroup);
            }
        }
    }

    sealed class VSSolutionFolder
    {
        public VSSolutionFolder(
            string name)
        {
            this.Guid = System.Guid.NewGuid().ToString("B").ToUpper();
            this.Projects = new Bam.Core.Array<VSProject>();
        }

        public string Guid
        {
            get;
            private set;
        }

        public Bam.Core.Array<VSProject> Projects
        {
            get;
            private set;
        }
    }

    public sealed class VSSolution
    {
        private System.Collections.Generic.Dictionary<System.Type, VSProject> ProjectMap = new System.Collections.Generic.Dictionary<System.Type, VSProject>();
        private System.Collections.Generic.Dictionary<string, VSSolutionFolder> SolutionFolders = new System.Collections.Generic.Dictionary<string, VSSolutionFolder>();

        public VSProject
        EnsureProjectExists(
            Bam.Core.V2.Module module)
        {
            var moduleType = module.GetType();
            if (!this.ProjectMap.ContainsKey(moduleType))
            {
                var project = new VSProject(this, module);
                this.ProjectMap.Add(moduleType, project);

                var groups = module.GetType().GetCustomAttributes(typeof(Bam.Core.ModuleGroupAttribute), true);
                if (groups.Length > 0)
                {
                    var solutionFolderName = (groups as Bam.Core.ModuleGroupAttribute[])[0].GroupName;
                    if (!this.SolutionFolders.ContainsKey(solutionFolderName))
                    {
                        this.SolutionFolders.Add(solutionFolderName, new VSSolutionFolder(solutionFolderName));
                    }
                    this.SolutionFolders[solutionFolderName].Projects.AddUnique(project);
                }
            }
            if (null == module.MetaData)
            {
                module.MetaData = this.ProjectMap[moduleType];
            }
            return this.ProjectMap[moduleType];
        }

        public System.Collections.Generic.IEnumerable<VSProject> Projects
        {
            get
            {
                foreach (var project in this.ProjectMap)
                {
                    yield return project.Value;
                }
            }
        }

        public System.Text.StringBuilder
        Serialize()
        {
            var ProjectTypeGuid = System.Guid.Parse("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942");
            var SolutionFolderGuid = System.Guid.Parse("2150E333-8FDC-42A3-9474-1A3956D46DE8");

            var content = new System.Text.StringBuilder();

            // TODO: obviously dependent on version
            content.AppendLine(@"Microsoft Visual Studio Solution File, Format Version 12.00");

            var configs = new Bam.Core.StringArray();
            foreach (var project in this.Projects)
            {
                content.AppendFormat("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"",
                    ProjectTypeGuid.ToString("B").ToUpper(),
                    System.IO.Path.GetFileNameWithoutExtension(project.ProjectPath),
                    project.ProjectPath, // TODO: relative to the solution file
                    project.Guid.ToString("B").ToUpper());
                content.AppendLine();
                content.AppendLine("EndProject");

                foreach (var config in project.Configurations)
                {
                    configs.AddUnique(config.Value.FullName);
                }
            }
            foreach (var folder in this.SolutionFolders)
            {
                content.AppendFormat("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"",
                    SolutionFolderGuid.ToString("B").ToUpper(),
                    folder.Key,
                    folder.Key,
                    folder.Value.Guid);
                content.AppendLine();
                content.AppendLine("EndProject");
            }
            content.AppendLine("Global");
            content.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            foreach (var config in configs)
            {
                // TODO: I'm sure these are not meant to be identical, but I don't know what else to put here
                content.AppendFormat("\t\t{0} = {0}", config);
                content.AppendLine();
            }
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var project in this.Projects)
            {
                foreach (var config in project.Configurations)
                {
                    var guid = project.Guid.ToString("B").ToUpper();
                    content.AppendFormat("\t\t{0}.{1}.ActiveCfg = {1}", guid, config.Value.FullName);
                    content.AppendLine();
                    content.AppendFormat("\t\t{0}.{1}.Build.0 = {1}", guid, config.Value.FullName);
                    content.AppendLine();
                }
            }
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
            content.AppendLine("\t\tHideSolutionNode = FALSE");
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
            foreach (var folder in this.SolutionFolders)
            {
                foreach (var project in folder.Value.Projects)
                {
                    content.AppendFormat("\t\t{0} = {1}", project.Guid.ToString("B").ToUpper(), folder.Value.Guid);
                    content.AppendLine();
                }
            }
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("EndGlobal");

            return content;
        }
    }

    public sealed class VSProjectFilter
    {
        private static readonly string SourceGroupName = "Source Files";
        private static readonly string HeaderGroupName = "Header Files";
        private Bam.Core.Array<VSSettingsGroup> Source = new Bam.Core.Array<VSSettingsGroup>();
        private Bam.Core.Array<VSSettingsGroup> Headers = new Bam.Core.Array<VSSettingsGroup>();

        public void
        AddHeader(
            Bam.Core.V2.TokenizedString path)
        {
            if (!this.Headers.Any(item => item.Include.Parse() == path.Parse()))
            {
                var group = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.Header, include: path);
                group.AddSetting("Filter", HeaderGroupName);
                this.Headers.AddUnique(group);
            }
        }

        public void
        AddSource(
            Bam.Core.V2.TokenizedString path)
        {
            if (!this.Source.Any(item => item.Include.Parse() == path.Parse()))
            {
                var group = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.Compiler, include: path);
                group.AddSetting("Filter", SourceGroupName);
                this.Source.AddUnique(group);
            }
        }

        public System.Xml.XmlDocument
        Serialize()
        {
            var document = new System.Xml.XmlDocument();

            var projectEl = this.CreateRootProject(document);
            projectEl.SetAttribute("ToolsVersion", "4.0"); // TODO: get this number from VisualC

            System.Action<System.Xml.XmlElement, string, Bam.Core.Array<VSSettingsGroup>> createXML = (parentEl, groupName, list) =>
                {
                    var allFiles = document.CreateVSItemGroup(parentEl: projectEl);

                    var idEl = document.CreateVSElement("Filter", parentEl: parentEl);
                    idEl.SetAttribute("Include", groupName);
                    document.CreateVSElement("UniqueIdentifier", System.Guid.NewGuid().ToString("B").ToUpper(), parentEl: idEl);
                    var extensions = new Bam.Core.StringArray();
                    foreach (var setting in list)
                    {
                        var path = setting.Include;
                        var extension = System.IO.Path.GetExtension(path.Parse()).TrimStart(new[] { '.' });
                        extensions.AddUnique(extension);

                        setting.Serialize(document, allFiles);
                    }
                    document.CreateVSElement("Extensions", extensions.ToString(';'), parentEl: idEl);
                };

            var containerEl = document.CreateVSItemGroup(parentEl: projectEl);
            if (this.Source.Count > 0)
            {
                createXML(containerEl, SourceGroupName, this.Source);
            }

            if (this.Headers.Count > 0)
            {
                createXML(containerEl, HeaderGroupName, this.Headers);
            }

            return document;
        }

        private System.Xml.XmlElement
        CreateRootProject(
            System.Xml.XmlDocument document)
        {
            var project = document.CreateVSElement("Project");
            document.AppendChild(project);
            return project;
        }
    }

    public sealed class VSProject
    {
        public VSProject(
            VSSolution solution,
            Bam.Core.V2.Module module)
        {
            this.Solution = solution;
            this.Guid = System.Guid.NewGuid(); // TODO: make this deterministic - can these be re-read upon regenerating the solution? so that the same project gets the same GUID each time Bam is run
            this.Configurations = new System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, VSProjectConfiguration>();
            this.ProjectPath = Bam.Core.V2.TokenizedString.Create("$(pkgbuilddir)/$(modulename).vcxproj", module).Parse();
            this.ProjectSettings = new Bam.Core.Array<VSSettingsGroup>();
            this.Headers = new Bam.Core.Array<VSSettingsGroup>();
            this.Sources = new Bam.Core.Array<VSSettingsGroup>();
            this.Filter = new VSProjectFilter();
            this.OrderOnlyDependentProjects = new Bam.Core.Array<VSProject>();
            this.LinkDependentProjects = new Bam.Core.Array<VSProject>();
        }

        public VSProjectConfiguration
        GetConfiguration(
            Bam.Core.V2.Module module)
        {
            var moduleConfig = module.BuildEnvironment.Configuration;
            if (this.Configurations.ContainsKey(moduleConfig))
            {
                return this.Configurations[moduleConfig];
            }

            var platform = Bam.Core.EPlatform.Invalid;
            var bitDepth = (module as C.V2.CModule).BitDepth;
            switch (bitDepth)
            {
                case C.V2.EBit.ThirtyTwo:
                    platform = Bam.Core.EPlatform.Win32;
                    break;

                case C.V2.EBit.SixtyFour:
                    platform = Bam.Core.EPlatform.Win64;
                    break;
            }
            if (Bam.Core.EPlatform.Invalid == platform)
            {
                throw new Bam.Core.Exception("Platform cannot be extracted from the tool {0} for project {1}", module.Tool.ToString(), this.ProjectPath);
            }
            var configuration = new VSProjectConfiguration(this, module, platform);
            this.Configurations.Add(moduleConfig, configuration);
            return configuration;
        }

        public  VSSettingsGroup
        GetUniqueSettingsGroup(
            VSSettingsGroup.ESettingsGroup group,
            Bam.Core.V2.TokenizedString include = null)
        {
            foreach (var settings in this.ProjectSettings)
            {
                if (null == include)
                {
                    if ((null == settings.Include) && (settings.Group == group))
                    {
                        return settings;
                    }
                }
                else
                {
                    // ignore group, as files can mutate between them during the buildprocess (e.g. headers into custom builds)
                    if ((null != include) && (settings.Include.Parse() == include.Parse()))
                    {
                        return settings;
                    }
                }
            }

            var newGroup = new VSSettingsGroup(group, include);
            this.ProjectSettings.Add(newGroup);
            return newGroup;
        }

        public void
        AddHeader(
            VSSettingsGroup header)
        {
            this.Headers.AddUnique(header);
            this.Filter.AddHeader(header.Include);
        }

        public void
        AddSource(
            VSSettingsGroup source)
        {
            this.Sources.AddUnique(source);
            this.Filter.AddSource(source.Include);
        }

        public void
        RequiresProject(
            VSProject dependentProject)
        {
            if (this.LinkDependentProjects.Contains(dependentProject))
            {
                throw new Bam.Core.Exception("Project already exists as a link dependency");
            }
            this.OrderOnlyDependentProjects.AddUnique(dependentProject);
        }

        public void
        LinkAgainstProject(
            VSProject dependentProject)
        {
            if (this.OrderOnlyDependentProjects.Contains(dependentProject))
            {
                throw new Bam.Core.Exception("Project already exists as an order only dependency");
            }
            this.LinkDependentProjects.AddUnique(dependentProject);
        }

        public string
        ProjectPath
        {
            get;
            private set;
        }

        public VSProjectFilter Filter
        {
            get;
            private set;
        }

        private VSSolution Solution
        {
            get;
            set;
        }

        public System.Guid Guid
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, VSProjectConfiguration> Configurations
        {
            get;
            private set;
        }

        private Bam.Core.Array<VSSettingsGroup> ProjectSettings
        {
            get;
            set;
        }

        private Bam.Core.Array<VSSettingsGroup> Headers
        {
            get;
            set;
        }

        private Bam.Core.Array<VSSettingsGroup> Sources
        {
            get;
            set;
        }

        private Bam.Core.Array<VSProject> OrderOnlyDependentProjects
        {
            get;
            set;
        }

        private Bam.Core.Array<VSProject> LinkDependentProjects
        {
            get;
            set;
        }

        private void
        SerializeDependentProjects(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentEl)
        {
            if (this.OrderOnlyDependentProjects.Count > 0)
            {
                var itemGroupEl = document.CreateVSItemGroup(parentEl: parentEl);
                foreach (var project in this.OrderOnlyDependentProjects)
                {
                    var projectRefEl = document.CreateVSElement("ProjectReference", parentEl: itemGroupEl);
                    projectRefEl.SetAttribute("Include", Bam.Core.RelativePathUtilities.GetPath(project.ProjectPath, this.ProjectPath));

                    document.CreateVSElement("Project", value: project.Guid.ToString("B"), parentEl: projectRefEl);
                    document.CreateVSElement("LinkLibraryDependencies", value: "false", parentEl: projectRefEl);
                }
            }
            if (this.LinkDependentProjects.Count > 0)
            {
                var itemGroupEl = document.CreateVSItemGroup(parentEl: parentEl);
                foreach (var project in this.LinkDependentProjects)
                {
                    var projectRefEl = document.CreateVSElement("ProjectReference", parentEl: itemGroupEl);
                    projectRefEl.SetAttribute("Include", Bam.Core.RelativePathUtilities.GetPath(project.ProjectPath, this.ProjectPath));

                    document.CreateVSElement("Project", value: project.Guid.ToString("B"), parentEl: projectRefEl);
                    document.CreateVSElement("LinkLibraryDependencies", value: "true", parentEl: projectRefEl);
                }
            }
        }

        public System.Xml.XmlDocument
        Serialize()
        {
            var document = new System.Xml.XmlDocument();

            var projectEl = this.CreateRootProject(document);
            projectEl.SetAttribute("DefaultTargets", "Build");
            projectEl.SetAttribute("ToolsVersion", "12.0"); // TODO: get from VisualC package

            // define configurations in the project
            var configurationItemGroup = document.CreateVSItemGroup("ProjectConfigurations", projectEl);
            foreach (var config in this.Configurations)
            {
                var projectConfigEl = document.CreateVSElement("ProjectConfiguration", parentEl: configurationItemGroup);
                projectConfigEl.SetAttribute("Include", config.Value.FullName);
                document.CreateVSElement("Configuration", value: config.Value.ConfigurationName, parentEl: projectConfigEl);
                document.CreateVSElement("Platform", value: config.Value.PlatformName, parentEl: projectConfigEl);
            }

            // global properties
            var globalPropertyGroup = document.CreateVSPropertyGroup(label: "Globals", parentEl: projectEl);
            document.CreateVSElement("ProjectGuid", value: this.Guid.ToString("B").ToUpper(), parentEl: globalPropertyGroup);

            document.CreateVSImport(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props", parentEl: projectEl);

            // configuration properties
            foreach (var config in this.Configurations)
            {
                config.Value.SerializeProperties(document, projectEl);
            }

            document.CreateVSImport(@"$(VCTargetsPath)\Microsoft.Cpp.props", parentEl: projectEl);

            // configuration paths
            foreach (var config in this.Configurations)
            {
                config.Value.SerializePaths(document, projectEl);
            }

            // tool settings
            foreach (var config in this.Configurations)
            {
                config.Value.SerializeSettings(document, projectEl);
            }

            // input files (these are VSSettingsGroups, but configuration agnostic)
            if (this.Sources.Count > 0)
            {
                var sourcesGroup = document.CreateVSItemGroup(parentEl: projectEl);
                foreach (var group in this.Sources)
                {
                    foreach (var config in this.Configurations)
                    {
                        if (!config.Value.ContainsSource(group))
                        {
                            group.AddSetting("ExcludedFromBuild", "true", config.Value.ConditionText);
                        }
                    }
                    group.Serialize(document, sourcesGroup);
                }
            }
            if (this.Headers.Count > 0)
            {
                var headersGroup = document.CreateVSItemGroup(parentEl: projectEl);
                foreach (var group in this.Headers)
                {
                    group.Serialize(document, headersGroup);
                }
            }

            // dependent projects
            this.SerializeDependentProjects(document, projectEl);

            document.CreateVSImport(@"$(VCTargetsPath)\Microsoft.Cpp.targets", parentEl: projectEl);

            return document;
        }

        private System.Xml.XmlElement
        CreateRootProject(
            System.Xml.XmlDocument document)
        {
            var project = document.CreateVSElement("Project");
            document.AppendChild(project);
            return project;
        }
    }

    // although this is project related data, it needs to be named after the builder, VSSolution
    public abstract class VSSolutionMeta
    {
        public static void
        PreExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;
            graph.MetaData = new VSSolution();
        }

        private static string
        PrettyPrintXMLDoc(
            System.Xml.XmlDocument document)
        {
            var content = new System.Text.StringBuilder();
            var settings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                NewLineChars = System.Environment.NewLine,
                Encoding = new System.Text.UTF8Encoding(false) // no BOM
            };
            using (var writer = System.Xml.XmlWriter.Create(content, settings))
            {
                document.Save(writer);
            }
            return content.ToString();
        }

        public static void
        PostExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;
            var solution = graph.MetaData as VSSolution;
            if (0 == solution.Projects.Count())
            {
                throw new Bam.Core.Exception("No projects were generated");
            }

            var xmlWriterSettings = new System.Xml.XmlWriterSettings
                {
                    OmitXmlDeclaration = false,
                    Encoding = new System.Text.UTF8Encoding(false), // no BOM (Byte Ordering Mark)
                    NewLineChars = System.Environment.NewLine,
                    Indent = true
                };

            foreach (var project in solution.Projects)
            {
                var projectPathDir = System.IO.Path.GetDirectoryName(project.ProjectPath);
                if (!System.IO.Directory.Exists(projectPathDir))
                {
                    System.IO.Directory.CreateDirectory(projectPathDir);
                }

                var projectXML = project.Serialize();
                using (var xmlwriter = System.Xml.XmlWriter.Create(project.ProjectPath, xmlWriterSettings))
                {
                    projectXML.WriteTo(xmlwriter);
                }
                Bam.Core.Log.DebugMessage(PrettyPrintXMLDoc(projectXML));

                var projectFilterXML = project.Filter.Serialize();
                using (var xmlwriter = System.Xml.XmlWriter.Create(project.ProjectPath + ".filters", xmlWriterSettings))
                {
                    projectFilterXML.WriteTo(xmlwriter);
                }
                Bam.Core.Log.DebugMessage(PrettyPrintXMLDoc(projectFilterXML));
            }

            var mainPackage = Bam.Core.State.PackageInfo[0];
            var solutionPath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(mainpackagename).sln", null).Parse();
            var solutionContents = solution.Serialize();
            using (var writer = new System.IO.StreamWriter(solutionPath))
            {
                writer.Write(solutionContents);
            }
            Bam.Core.Log.DebugMessage(solutionContents.ToString());

            Bam.Core.Log.Info("Successfully created Visual Studio solution file for package '{0}'\n\t{1}", Bam.Core.State.PackageInfo[0].Name, solutionPath);
        }
    }
#else
    public abstract class Group :
        System.Collections.Generic.IEnumerable<System.Xml.XmlElement>
    {
        protected Group(
            System.Xml.XmlElement element)
        {
            this.Element = element;
        }

        public System.Xml.XmlElement Element
        {
            get;
            private set;
        }

        public System.Collections.Generic.IEnumerator<System.Xml.XmlElement> GetEnumerator()
        {
            foreach (var child in this.Element.ChildNodes)
            {
                yield return child as System.Xml.XmlElement;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public sealed class ItemGroup :
        Group
    {
        public ItemGroup(
            System.Xml.XmlElement element) :
            base(element)
        {
        }
    }

    public sealed class PropertyGroup :
        Group
    {
        public PropertyGroup(
            System.Xml.XmlElement element)
            : base(element)
        {
        }

        public string Condition
        {
            set
            {
                var condition = this.Element.GetAttributeNode("Condition");
                if (null == condition)
                {
                    condition = this.Element.Attributes.Append(this.Element.OwnerDocument.CreateAttribute("Condition"));
                }
                condition.Value = value;
            }
        }
    }

    public sealed class Import :
        Group
    {
        public Import(
            System.Xml.XmlElement element)
            : base(element)
        { }
    }

    public sealed class ItemDefinitionGroup :
        Group
    {
        public ItemDefinitionGroup(
            System.Xml.XmlElement element) :
            base(element)
        {
        }
    }

    sealed class VSSolutionFolder
    {
        public VSSolutionFolder(
            string name)
        {
            this.Guid = System.Guid.NewGuid().ToString("B").ToUpper();
            this.Projects = new Bam.Core.Array<VSProject>();
        }

        public string Guid
        {
            get;
            private set;
        }

        public Bam.Core.Array<VSProject> Projects
        {
            get;
            private set;
        }
    }

    public sealed class VSSolution :
        System.Collections.Generic.IEnumerable<VSProject>
    {
        static private readonly System.Guid SolutionFolderGUID = System.Guid.Parse("2150E333-8FDC-42A3-9474-1A3956D46DE8");

        public VSSolution()
        {
            this.Projects = new System.Collections.Generic.Dictionary<System.Type, VSProject>();
            this.SolutionFolders = new System.Collections.Generic.Dictionary<string, VSSolutionFolder>();
        }

        private System.Collections.Generic.Dictionary<System.Type, VSProject> Projects
        {
            get;
            set;
        }

        private System.Collections.Generic.Dictionary<string, VSSolutionFolder> SolutionFolders
        {
            get;
            set;
        }

        public VSProject
        FindOrCreateProject(
            Bam.Core.V2.Module module,
            VSProject.Type projectType,
            string projectPath)
        {
            var moduleType = module.GetType();
            lock(this)
            {
                if (this.Projects.ContainsKey(moduleType))
                {
                    return this.Projects[moduleType];
                }
                else
                {
                    var project = new VSProject(projectType, module, projectPath);
                    this.Projects[moduleType] = project;

                    var groups = module.GetType().GetCustomAttributes(typeof(Bam.Core.ModuleGroupAttribute), true);
                    if (groups.Length > 0)
                    {
                        var solutionFolderName = (groups as Bam.Core.ModuleGroupAttribute[])[0].GroupName;
                        if (!this.SolutionFolders.ContainsKey(solutionFolderName))
                        {
                            this.SolutionFolders.Add(solutionFolderName, new VSSolutionFolder(solutionFolderName));
                        }
                        this.SolutionFolders[solutionFolderName].Projects.AddUnique(project);
                    }

                    return project;
                }
            }
        }

        public System.Text.StringBuilder Serialize()
        {
            var content = new System.Text.StringBuilder();

            // TODO: obviously dependent on version
            content.AppendLine(@"Microsoft Visual Studio Solution File, Format Version 12.00");
            content.AppendLine(@"# Visual Studio Express 2013 for Windows Desktop");

            var configs = new Bam.Core.StringArray();
            foreach (var project in this.Projects.Values)
            {
                content.AppendFormat("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"",
                    project.TypeGUID.ToString("B").ToUpper(),
                    System.IO.Path.GetFileNameWithoutExtension(project.ProjectPath),
                    project.ProjectPath, // TODO: relative to the solution file
                    project.GUID.ToString("B").ToUpper());
                content.AppendLine();
                content.AppendLine("EndProject");

                foreach (var config in project.Configurations)
                {
                    configs.AddUnique(config.FullName);
                }
            }
            foreach (var folder in this.SolutionFolders)
            {
                content.AppendFormat("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"",
                    SolutionFolderGUID.ToString("B").ToUpper(),
                    folder.Key,
                    folder.Key,
                    folder.Value.Guid);
                content.AppendLine();
                content.AppendLine("EndProject");
            }
            content.AppendLine("Global");
            content.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            foreach (var config in configs)
            {
                // TODO: I'm sure these are not meant to be identical, but I don't know what else to put here
                content.AppendFormat("\t\t{0} = {0}", config);
                content.AppendLine();
            }
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var project in this.Projects.Values)
            {
                foreach (var config in project.Configurations)
                {
                    var guid = project.GUID.ToString("B").ToUpper();
                    content.AppendFormat("\t\t{0}.{1}.ActiveCfg = {1}", guid, config);
                    content.AppendLine();
                    content.AppendFormat("\t\t{0}.{1}.Build.0 = {1}", guid, config);
                    content.AppendLine();
                }
            }
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
            content.AppendLine("\t\tHideSolutionNode = FALSE");
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
            foreach (var folder in this.SolutionFolders)
            {
                foreach (var project in folder.Value.Projects)
                {
                    content.AppendFormat("\t\t{0} = {1}", project.GUID.ToString("B").ToUpper(), folder.Value.Guid);
                    content.AppendLine();
                }
            }
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("EndGlobal");

            return content;
        }

        public System.Collections.Generic.IEnumerator<VSProject> GetEnumerator()
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

    // TODO: this class does share some aspects of the VSProject class
    public sealed class VSProjectFilter :
        System.Xml.XmlDocument
    {
        private static readonly string VCProjNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        private System.Collections.Generic.Dictionary<string, System.Xml.XmlElement> GroupToItemGroup = new System.Collections.Generic.Dictionary<string, System.Xml.XmlElement>();

        public VSProjectFilter(
            string path,
            Bam.Core.V2.Module module)
        {
            this.Path = Bam.Core.V2.TokenizedString.Create(path, module).Parse();
            var project = this.AppendChild(this.CreateProjectElement("Project"));
            project.Attributes.Append(this.CreateAttribute("ToolsVersion")).Value = "4.0";

            this.Groups = this.CreateItemGroup(null).Element;
            project.AppendChild(this.Groups);

            this.ProjectElement = project;
        }

        private System.Xml.XmlNode ProjectElement
        {
            get;
            set;
        }

        public string Path
        {
            get;
            private set;
        }

        public void
        AddFile(
            System.Xml.XmlElement fileElement,
            string groupname,
            string extension)
        {
            System.Func<System.Xml.XmlNode> findGroupName = () =>
                {
                    foreach (System.Xml.XmlNode node in this.Groups.ChildNodes)
                    {
                        if (node.Attributes["Include"].Value == groupname)
                        {
                            return node;
                        }
                    }
                    return null;
                };
            var group = findGroupName();
            if (null == group)
            {
                this.Groups.AppendChild(this.CreateFilter(groupname, extension));
            }
            else
            {
                var existingExtensions = new Bam.Core.StringArray(group.LastChild.InnerText.Split(new[] { ';' }));
                existingExtensions.AddUnique(extension);
                group.LastChild.InnerText = existingExtensions.ToString(';');
            }

            var fileType = fileElement.Name;
            var filePath = fileElement.Attributes["Include"].Value;
            if (!this.GroupToItemGroup.ContainsKey(groupname))
            {
                var files = this.CreateItemGroup(null).Element;
                this.ProjectElement.AppendChild(files);
                this.GroupToItemGroup.Add(groupname, files);
            }
            var fileItemGroup = this.GroupToItemGroup[groupname];

            System.Func<System.Xml.XmlNode> findFileName = () =>
                {
                    foreach (System.Xml.XmlNode node in fileItemGroup.ChildNodes)
                    {
                        if (node.Attributes["Include"].Value == filePath)
                        {
                            return node;
                        }
                    }
                    return null;
                };
            var file = findFileName();
            if (null == file)
            {
                var fileFilter = this.CreateProjectElement(fileType);
                fileFilter.Attributes.Append(this.CreateAttribute("Include")).Value = filePath;

                var filter = this.CreateProjectElement("Filter");
                filter.InnerText = groupname;
                fileFilter.AppendChild(filter);

                fileItemGroup.AppendChild(fileFilter);
            }
        }

        private System.Xml.XmlElement Groups
        {
            get;
            set;
        }

        private System.Xml.XmlElement
        CreateProjectElement(
            string name)
        {
            return this.CreateElement(name, VCProjNamespace);
        }

        private ItemGroup
        CreateItemGroup(
            string label)
        {
            var group = this.CreateProjectElement("ItemGroup");
            if (null != label)
            {
                group.Attributes.Append(this.CreateAttribute("Label")).Value = label;
            }
            return new ItemGroup(group);
        }

        private System.Xml.XmlElement
        CreateFilter(
            string name,
            string extension)
        {
            var filter = this.CreateProjectElement("Filter");
            filter.Attributes.Append(this.CreateAttribute("Include")).Value = name;

            var uid = this.CreateProjectElement("UniqueIdentifier");
            uid.InnerText = System.Guid.NewGuid().ToString("B").ToUpper();
            filter.AppendChild(uid);

            var extensions = this.CreateProjectElement("Extensions");
            extensions.InnerText = extension;
            filter.AppendChild(extensions);

            return filter;
        }
    }

    public sealed class VSProject :
        System.Xml.XmlDocument
    {
        public enum Type
        {
            NA,
            StaticLibrary,
            DynamicLibrary,
            Application,
            Utility
        }

        private static readonly string VCProjNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private ItemGroup ProjectConfigurationsGroup;
        private System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement> ProjectConfigurations = new System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement>();
        private System.Collections.Generic.Dictionary<VSProjectConfiguration, PropertyGroup> ConfigurationGroups = new System.Collections.Generic.Dictionary<VSProjectConfiguration, PropertyGroup>();
        private PropertyGroup Globals;
        private Import DefaultImport;
        private Import LanguageImport;
        private ItemGroup SourceGroup;
        private ItemGroup HeaderGroup;
        private ItemGroup CustomBuildGroup;
        private ItemGroup ProjectDependenciesGroup;
        private Import LanguageTargets;
        private Type ProjectType;
        private System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement> ConfigToolsProperties = new System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement>();
        private System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement> CommonCompilationOptions = new System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement>();
        private System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement> PreBuildCommand = new System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement>();
        private System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement> PostBuildCommand = new System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement>();
        private System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement> AnonymousPropertySettings = new System.Collections.Generic.Dictionary<VSProjectConfiguration, System.Xml.XmlElement>();
        private System.Collections.Generic.List<VSProject> DependentProjects = new System.Collections.Generic.List<VSProject>();
        private System.Collections.Generic.Dictionary<string, System.Xml.XmlElement> SourceXMLMap = new System.Collections.Generic.Dictionary<string, System.Xml.XmlElement>();

        public VSProject(
            Type type,
            Bam.Core.V2.Module module,
            string projectPath)
        {
            this.ProjectType = type;
            this.ProjectPath = Bam.Core.V2.TokenizedString.Create(projectPath, module).Parse();
            this.Filter = new VSProjectFilter(projectPath + ".filters", module);

            this.GUID = System.Guid.NewGuid();
            this.TypeGUID = System.Guid.Parse("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942");
            this.Configurations = new Bam.Core.Array<VSProjectConfiguration>();

            // Project (root) element
            this.AppendChild(this.CreateProjectElement("Project"));
            this.Project.Attributes.Append(this.CreateAttribute("DefaultTargets")).Value = "Build";
            this.Project.Attributes.Append(this.CreateAttribute("ToolsVersion")).Value = "12.0"; // TODO: in tune with VisualC package version

            // ProjectConfigurationsGroup element
            this.ProjectConfigurationsGroup = this.CreateItemGroup("ProjectConfigurations");
            this.Project.AppendChild(this.ProjectConfigurationsGroup.Element);

            // Globals element
            this.Globals = this.CreatePropertyGroup("Globals");
            var guid = this.CreateProjectElement("ProjectGuid", GUID.ToString("B").ToUpper());
            this.Globals.Element.AppendChild(guid);
            this.Project.AppendChild(this.Globals.Element);

            // Default Import
            this.DefaultImport = this.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props");
            this.Project.AppendChild(this.DefaultImport.Element);

            // empty slot for Configuration properties

            // Cxx property import
            this.LanguageImport = this.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.props");
            this.Project.AppendChild(this.LanguageImport.Element);

            // empty slot for Configuration definitions

            if (this.ProjectType != Type.Utility)
            {
                // Sources
                this.SourceGroup = this.CreateItemGroup(null);
                this.Project.AppendChild(this.SourceGroup.Element);

                // Project Dependencies
                this.ProjectDependenciesGroup = this.CreateItemGroup(null);
                this.Project.AppendChild(this.ProjectDependenciesGroup.Element);
            }

            // Language targets
            this.LanguageTargets = this.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.targets");
            this.Project.AppendChild(this.LanguageTargets.Element);
        }

        public void
        AddCustomBuildFile(
            C.V2.HeaderFile module,
            VSProjectCustomBuild custom,
            VSProjectConfiguration config)
        {
            var headerPath = module.InputPath.Parse();

            if (null == this.CustomBuildGroup)
            {
                this.CustomBuildGroup = this.CreateItemGroup(null);
                // TODO: better to just have a 'last added' element?
                if (null == this.HeaderGroup)
                {
                    this.Project.InsertAfter(this.CustomBuildGroup.Element, this.SourceGroup.Element);
                }
                else
                {
                    this.Project.InsertAfter(this.CustomBuildGroup.Element, this.HeaderGroup.Element);
                }
            }

            System.Action<System.Xml.XmlElement> addCustomBuild = (element) =>
            {
                if (null == element)
                {
                    element = this.CreateProjectElement("CustomBuild");
                    element.Attributes.Append(this.CreateAttribute("Include")).Value = headerPath;
                    this.CustomBuildGroup.Element.AppendChild(element);
                }

                var commandEl = this.CreateProjectElement("Command");
                this.MakeNodeConditional(commandEl, config);
                commandEl.InnerText = custom.Command;
                element.AppendChild(commandEl);

                var messageEl = this.CreateProjectElement("Message");
                this.MakeNodeConditional(messageEl, config);
                messageEl.InnerText = custom.Message;
                element.AppendChild(messageEl);

                var outputsEl = this.CreateProjectElement("Outputs");
                this.MakeNodeConditional(outputsEl, config);
                outputsEl.InnerText = custom.Outputs.ToString(';');
                element.AppendChild(outputsEl);

                var ext = System.IO.Path.GetExtension(headerPath).TrimStart(new[] { '.' });
                this.Filter.AddFile(element, "Custom Build", ext);
            };

            // check whether this header file has been added before, for the actual project
            foreach (var el in this.CustomBuildGroup)
            {
                if (!el.HasAttribute("Include"))
                {
                    continue;
                }
                var include = el.Attributes["Include"];
                if (include.Value == headerPath)
                {
                    Bam.Core.Log.DebugMessage("Custom build path '{0}' already added", headerPath);
                    addCustomBuild(el);
                    return;
                }
            }

            addCustomBuild(null);
        }

        public void
        AddHeaderFile(
            C.V2.HeaderFile module)
        {
            var headerPath = module.InputPath.Parse();

            if (null == this.HeaderGroup)
            {
                this.HeaderGroup = this.CreateItemGroup(null);
                if (null == this.SourceGroup)
                {
                    this.Project.InsertAfter(this.HeaderGroup.Element, this.LanguageImport.Element);
                }
                else
                {
                    this.Project.InsertAfter(this.HeaderGroup.Element, this.SourceGroup.Element);
                }
            }

            // check whether this header file has been added before, for the actual project
            foreach (var el in this.HeaderGroup)
            {
                if (!el.HasAttribute("Include"))
                {
                    continue;
                }
                var include = el.Attributes["Include"];
                if (include.Value == headerPath)
                {
                    Bam.Core.Log.DebugMessage("Header path '{0}' already added", headerPath);
                    return;
                }
            }

            var element = this.CreateProjectElement("ClInclude");
            element.Attributes.Append(this.CreateAttribute("Include")).Value = headerPath;
            this.HeaderGroup.Element.AppendChild(element);

            var ext = System.IO.Path.GetExtension(headerPath).TrimStart(new[] { '.' });
            this.Filter.AddFile(element, "Header Files", ext);
        }

        public void
        AddSourceFile(
            Bam.Core.V2.Module module,
            Bam.Core.V2.Settings patchSettings,
            VSProjectConfiguration configuration)
        {
            var objectFile = module.MetaData as VSProjectObjectFile;
            var sourcePath = objectFile.Source.ToString();

            // always add the source to the configuration
            configuration.Source.AddUnique(sourcePath);

            // check whether this source file has been added before, for the actual project
#if true
            foreach (var el in this.SourceGroup)
            {
                if (!el.HasAttribute("Include"))
                {
                    continue;
                }
                var include = el.Attributes["Include"];
                if (include.Value == sourcePath)
                {
                    Bam.Core.Log.DebugMessage("Source path '{0}' already added", sourcePath);
                    // still need to apply the patch
                    if (null != patchSettings)
                    {
                        (patchSettings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, el, configuration);
                    }
                    return;
                }
            }
#else
            var found = this.SourceGroup.Where((el) =>
                {
                    if (!el.HasAttribute("Include"))
                    {
                        return false;
                    }
                    var include = el.Attributes["Include"];
                    return include.Value == path;
                });
#endif

            var element = this.CreateProjectElement("ClCompile");
            element.Attributes.Append(this.CreateAttribute("Include")).Value = sourcePath;
            this.SourceGroup.Element.AppendChild(element);

            this.SourceXMLMap.Add(sourcePath, element);

            if (null != patchSettings)
            {
                (patchSettings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, element, configuration);
            }

            var ext = System.IO.Path.GetExtension(sourcePath).TrimStart(new[] { '.' });
            this.Filter.AddFile(element, "Source Files", ext);
        }

        public static string GetConfigurationName(string configuration, string platform)
        {
            return System.String.Format("{0}|{1}", configuration, platform);
        }

        public void
        AddProjectConfiguration(
            VSProjectConfiguration configuration,
            Bam.Core.V2.Module module,
            Bam.Core.V2.TokenizedString outPath)
        {
            this.Configurations.AddUnique(configuration);

            // overall project configurations
            {
                var projconfig = this.CreateProjectElement("ProjectConfiguration");
                projconfig.Attributes.Append(this.CreateAttribute("Include")).Value = configuration.FullName;
                var config = this.CreateProjectElement("Configuration", configuration.Config);
                var plat = this.CreateProjectElement("Platform", configuration.Platform);
                projconfig.AppendChild(config);
                projconfig.AppendChild(plat);
                this.ProjectConfigurationsGroup.Element.AppendChild(projconfig);
                this.ProjectConfigurations.Add(configuration, projconfig);
            }

            // project properties
            {
                var configProps = this.CreatePropertyGroup("Configuration");
                this.MakeNodeConditional(configProps.Element, configuration);

                // configuration type
                // TODO: can this be better done with a lambda to get the inner text?
                var configType = this.CreateProjectElement("ConfigurationType");
                switch (this.ProjectType)
                {
                    case VSProject.Type.NA:
                        throw new Bam.Core.Exception("Invalid project type");

                    case VSProject.Type.StaticLibrary:
                        configType.InnerText = "StaticLibrary";
                        break;

                    case VSProject.Type.Application:
                        configType.InnerText = "Application";
                        break;

                    case VSProject.Type.DynamicLibrary:
                        configType.InnerText = "DynamicLibrary";
                        break;

                    case VSProject.Type.Utility:
                        configType.InnerText = "Utility";
                        break;

                    default:
                        throw new Bam.Core.Exception("Unknown project type, {0}", this.ProjectType.ToString());
                }
                configProps.Element.AppendChild(configType);

                // platform toolset
                var platformToolset = this.CreateProjectElement("PlatformToolset", "v120"); // TODO: dependent upon the version of VisualC
                configProps.Element.AppendChild(platformToolset);
                this.Project.InsertAfter(configProps.Element, this.DefaultImport.Element);

                // use of debug runtimes?
                // assume not for speed initially
                var useDebugLibs = this.CreateProjectElement("UseDebugLibraries", "false");
                configProps.Element.AppendChild(useDebugLibs);
                this.Project.InsertAfter(configProps.Element, this.DefaultImport.Element);

                // character set
                var characterset = this.CreateProjectElement("CharacterSet", "NotSet");
                // assume unicode for most flexibility
                //var characterset = this.CreateProjectElement("CharacterSet", "Unicode");
                configProps.Element.AppendChild(characterset);
                this.Project.InsertAfter(configProps.Element, this.DefaultImport.Element);

                // whole program optimization
                var wpo = this.CreateProjectElement("WholeProgramOptimization", (configuration.Config == "Debug") ? "false" : "true");
                configProps.Element.AppendChild(wpo);
                this.Project.InsertAfter(configProps.Element, this.DefaultImport.Element);

                this.ConfigurationGroups.Add(configuration, configProps);
            }

            // project definitions
            if (this.ProjectType != Type.Utility)
            {
                var configGroup = this.CreateItemDefinitionGroup(configuration);
                var clCompile = this.CreateProjectElement("ClCompile");
                configGroup.Element.AppendChild(clCompile);
                this.CommonCompilationOptions.Add(configuration, clCompile);
                switch (this.ProjectType)
                {
                    case VSProject.Type.NA:
                        throw new Bam.Core.Exception("Invalid project type");

                    case VSProject.Type.StaticLibrary:
                        {
                            var tool = this.CreateProjectElement("Lib");
                            configGroup.Element.AppendChild(tool);
                            (module.Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, tool, null);
                        }
                        break;

                    case VSProject.Type.Application:
                    case VSProject.Type.DynamicLibrary:
                        {
                            var tool = this.CreateProjectElement("Link");
                            configGroup.Element.AppendChild(tool);
                            (module.Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, tool, null);
                        }
                        break;

                    case VSProject.Type.Utility:
                        // no tools required
                        break;

                    default:
                        throw new Bam.Core.Exception("Unknown project type, {0}", this.ProjectType.ToString());
                }
                this.Project.InsertAfter(configGroup.Element, this.LanguageImport.Element);
                this.ConfigToolsProperties.Add(configuration, configGroup.Element);
            }

            // anonymous project settings
            var anonConfigProps = this.CreatePropertyGroup(null);
            this.AnonymousPropertySettings.Add(configuration, anonConfigProps.Element);
            this.MakeNodeConditional(anonConfigProps.Element, configuration);

            // outPath is null for utility projects
            if (null != outPath)
            {
                // build output directory
                var outDirEl = this.CreateProjectElement("OutDir");
                var macros = new Bam.Core.V2.MacroList();
                // TODO: ideally, $(ProjectDir) should replace the following directory separator as well,
                // but it does not seem to be a show stopper if it doesn't
                macros.Add("pkgbuilddir", Bam.Core.V2.TokenizedString.Create("$(ProjectDir)", null, verbatim: true));
                macros.Add("modulename", Bam.Core.V2.TokenizedString.Create("$(ProjectName)", null, verbatim: true));
                var outDir = outPath.Parse(macros);
                outDir = System.IO.Path.GetDirectoryName(outDir);
                outDir += "\\";
                outDirEl.InnerText = outDir;
                this.AnonymousPropertySettings[configuration].AppendChild(outDirEl);

                // does the target name differ?
                var outputName = module.Macros["OutputName"].Parse();
                var moduleName = module.Macros["modulename"].Parse();
                if (outputName != moduleName)
                {
                    var targetNameEl = this.CreateProjectElement("TargetName");
                    targetNameEl.InnerText = outputName;
                    this.AnonymousPropertySettings[configuration].AppendChild(targetNameEl);
                }
            }
            else
            {
                var intDirEl = this.CreateProjectElement("IntDir");
                intDirEl.InnerText = @"$(ProjectDir)\$(ProjectName)\$(Configuration)\";
                this.AnonymousPropertySettings[configuration].AppendChild(intDirEl);
            }
            this.Project.InsertAfter(this.AnonymousPropertySettings[configuration], this.LanguageImport.Element);
        }

        public void
        AddDependentProject(
            VSProject project)
        {
            if (this.DependentProjects.Contains(project))
            {
                return;
            }

            var projectPath = this.ProjectPath;
            var dependentProjectPath = project.ProjectPath;

            var element = this.CreateProjectElement("ProjectReference");
            element.Attributes.Append(this.CreateAttribute("Include")).Value = Bam.Core.RelativePathUtilities.GetPath(dependentProjectPath, projectPath);

            var projectElement = this.CreateProjectElement("Project");
            projectElement.InnerText = project.GUID.ToString("B");
            element.AppendChild(projectElement);

            var linkElement = this.CreateProjectElement("LinkLibraryDependencies");
            linkElement.InnerText = "true"; // TODO: this would be false for an order only dependency
            element.AppendChild(linkElement);

            this.ProjectDependenciesGroup.Element.AppendChild(element);

            this.DependentProjects.Add(project);
        }

        public System.Collections.Generic.IEnumerable<VSProject>
        Dependents
        {
            get
            {
                foreach (var project in this.DependentProjects)
                {
                    yield return project;
                }
            }
        }

        public void
        SetCommonCompilationOptions(
            Bam.Core.V2.Module module,
            Bam.Core.V2.Settings settings,
            VSProjectConfiguration config)
        {
            (settings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, this.CommonCompilationOptions[config], null);

            var moduleWithPath = (module is Bam.Core.V2.IModuleGroup) ? module.Children[0] : module;
            var intPath = moduleWithPath.GeneratedPaths[C.V2.ObjectFile.Key];

            var intDirEl = this.CreateProjectElement("IntDir");
            var macros = new Bam.Core.V2.MacroList();
            macros.Add("pkgbuilddir", Bam.Core.V2.TokenizedString.Create("$(ProjectDir)", null, verbatim: true));
            macros.Add("modulename", Bam.Core.V2.TokenizedString.Create("$(ProjectName)", null, verbatim: true));
            var intDir = intPath.Parse(macros);
            intDir = System.IO.Path.GetDirectoryName(intDir);
            intDir += "\\";
            intDirEl.InnerText = intDir;
            this.AnonymousPropertySettings[config].AppendChild(intDirEl);
        }

        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands,
            VSProjectConfiguration config)
        {
            if (!this.PreBuildCommand.ContainsKey(config))
            {
                var tool = this.CreateProjectElement("PreBuildEvent");
                this.ConfigToolsProperties[config].AppendChild(tool);

                var commandElement = this.CreateProjectElement("Command");
                tool.AppendChild(commandElement);

                this.PreBuildCommand.Add(config, commandElement);
            }

            var commandText = new System.Text.StringBuilder();
            foreach (var command in commands)
            {
                commandText.AppendFormat("{0}{1}", command, System.Environment.NewLine);
            }
            this.PreBuildCommand[config].InnerText += commandText.ToString();
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands,
            VSProjectConfiguration config)
        {
            if (!this.PostBuildCommand.ContainsKey(config))
            {
                var tool = this.CreateProjectElement("PostBuildEvent");
                this.ConfigToolsProperties[config].AppendChild(tool);

                var commandElement = this.CreateProjectElement("Command");
                tool.AppendChild(commandElement);

                this.PostBuildCommand.Add(config, commandElement);
            }

            var commandText = new System.Text.StringBuilder();
            foreach (var command in commands)
            {
                commandText.AppendFormat("{0}{1}", command, System.Environment.NewLine);
            }
            this.PostBuildCommand[config].InnerText += commandText.ToString();
        }

        public System.Xml.XmlElement CreateProjectElement(string name)
        {
            return this.CreateElement(name, VCProjNamespace);
        }

        public System.Xml.XmlElement CreateProjectElement(string name, string value)
        {
            var el = this.CreateProjectElement(name);
            el.InnerText = value;
            return el;
        }

        public System.Xml.XmlElement
        CreateProjectElement<T>(
            string name,
            System.Action<T, string, System.Text.StringBuilder> function,
            T value)
        {
            var el = this.CreateProjectElement(name);
            var text = new System.Text.StringBuilder();
            function(value, name, text);
            el.InnerText = text.ToString();
            return el;
        }

        private ItemGroup CreateItemGroup(string label)
        {
            var group = this.CreateProjectElement("ItemGroup");
            if (null != label)
            {
                group.Attributes.Append(this.CreateAttribute("Label")).Value = label;
            }
            return new ItemGroup(group);
        }

        private PropertyGroup CreatePropertyGroup(string label)
        {
            var group = this.CreateProjectElement("PropertyGroup");
            if (null != label)
            {
                group.Attributes.Append(this.CreateAttribute("Label")).Value = label;
            }
            return new PropertyGroup(group);
        }

        private Import CreateImport(string projectPath)
        {
            var import = this.CreateProjectElement("Import");
            import.Attributes.Append(this.CreateAttribute("Project")).Value = projectPath;
            return new Import(import);
        }

        private ItemGroup
        CreateItemDefinitionGroup(
            VSProjectConfiguration configuration)
        {
            var group = this.CreateProjectElement("ItemDefinitionGroup");
            this.MakeNodeConditional(group, configuration);
            return new ItemGroup(group);
        }

        private System.Xml.XmlElement Project
        {
            get
            {
                return this.DocumentElement;
            }
        }

        public string ProjectPath
        {
            get;
            set;
        }

        public VSProjectFilter Filter
        {
            get;
            private set;
        }

        public System.Guid GUID
        {
            get;
            private set;
        }

        public System.Guid TypeGUID
        {
            get;
            private set;
        }

        public Bam.Core.Array<VSProjectConfiguration> Configurations
        {
            get;
            private set;
        }

        public void
        MakeNodeConditional(
            System.Xml.XmlNode node,
            VSProjectConfiguration configuration)
        {
            node.Attributes.Append(this.CreateAttribute("Condition")).Value = System.String.Format("'$(Configuration)|$(Platform)'=='{0}'", configuration.FullName);
        }

        public void AddToolSetting<T>(
            System.Xml.XmlElement container,
            string settingName,
            T settingValue,
            VSProjectConfiguration configuration,
            System.Action<T, string, System.Text.StringBuilder> process)
        {
            var settingElement = container.AppendChild(this.CreateProjectElement(settingName, process, settingValue));
            if (null != configuration)
            {
                this.MakeNodeConditional(settingElement, configuration);
            }
        }

        public void
        FixupPerConfigurationData()
        {
            foreach (var config in this.Configurations)
            {
                var delta = (new Bam.Core.StringArray(this.SourceXMLMap.Keys)).Complement(config.Source);
                if (delta.Count > 0)
                {
                    foreach (var excludedSource in delta)
                    {
                        var element = this.SourceXMLMap[excludedSource];
                        this.AddToolSetting(element, "ExcludedFromBuild", excludedSource, config,
                            (setting, attributeName, builder) =>
                            {
                                builder.Append("true");
                            });
                    }
                }
            }
        }
    }

    public sealed class VSProjectConfiguration
    {
        public VSProjectConfiguration(
            string config,
            string platform)
        {
            this.Config = config;
            this.Platform = platform;
            this.FullName = VSProject.GetConfigurationName(config, platform);
            this.Source = new Bam.Core.StringArray();
        }

        public string FullName
        {
            get;
            private set;
        }

        // TODO: should this not be the EConfiguration enum?
        public string Config
        {
            get;
            private set;
        }

        // TODO: should this not be the EPlatform enum?
        public string Platform
        {
            get;
            private set;
        }

        public Bam.Core.StringArray Source
        {
            get;
            private set;
        }
    }

    // although this is project related data, it needs to be named after the builder, VSSolution
    public abstract class VSSolutionMeta
    {
        public enum EPlatform
        {
            ThirtyTwo,
            SixtyFour
        }

        private static System.Collections.Generic.Dictionary<Bam.Core.V2.Module, VSSolutionMeta> InternalMap = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, VSSolutionMeta>();
        private static System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.StringArray> StoredPreBuildCommands = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.StringArray>();
        private static System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.StringArray> StoredPostBuildCommands = new System.Collections.Generic.Dictionary<Bam.Core.V2.Module, Bam.Core.StringArray>();

        public static void
        AddPreBuildCommands(
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commands)
        {
            if (InternalMap.ContainsKey(module))
            {
                InternalMap[module].Project.AddPreBuildCommands(commands, InternalMap[module].Configuration);
            }
            else
            {
                StoredPreBuildCommands.Add(module, commands);
            }
        }

        public static void
        AddPostBuildCommands(
            Bam.Core.V2.Module module,
            Bam.Core.StringArray commands)
        {
            if (InternalMap.ContainsKey(module))
            {
                InternalMap[module].Project.AddPostBuildCommands(commands, InternalMap[module].Configuration);
            }
            else
            {
                StoredPostBuildCommands.Add(module, commands);
            }
        }

        protected VSSolutionMeta(
            Bam.Core.V2.Module module,
            VSProject.Type type,
            Bam.Core.V2.TokenizedString outPath,
            EPlatform platform)
        {
            var graph = Bam.Core.V2.Graph.Instance;
            var isReferenced = graph.IsReferencedModule(module);
            this.IsProjectModule = isReferenced;

            var platformName = (platform == EPlatform.SixtyFour) ? "x64" : "Win32";
            this.Configuration = new VSProjectConfiguration(module.BuildEnvironment.Configuration.ToString(), platformName);

            if (isReferenced)
            {
                var solution = graph.MetaData as VSSolution;
                this.Project = solution.FindOrCreateProject(module, type, "$(pkgbuilddir)/$(modulename).vcxproj");
                this.Project.AddProjectConfiguration(this.Configuration, module, outPath);
                this.ProjectModule = module;

                InternalMap.Add(module, this);
                if (StoredPreBuildCommands.ContainsKey(module))
                {
                    this.Project.AddPreBuildCommands(StoredPreBuildCommands[module], this.Configuration);
                }
                if (StoredPostBuildCommands.ContainsKey(module))
                {
                    this.Project.AddPostBuildCommands(StoredPostBuildCommands[module], this.Configuration);
                }
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

        public VSProject Project
        {
            get;
            protected set;
        }

        public Bam.Core.V2.Module ProjectModule
        {
            get;
            private set;
        }

        public VSProjectConfiguration Configuration
        {
            get;
            private set;
        }

        public static void PreExecution()
        {
            var graph = Bam.Core.V2.Graph.Instance;
            graph.MetaData = new VSSolution();
        }

        public static void PostExecution()
        {
            var settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.Encoding = new System.Text.UTF8Encoding(false); // no BOM
            settings.NewLineChars = System.Environment.NewLine;
            settings.Indent = true;
            settings.ConformanceLevel = System.Xml.ConformanceLevel.Document;

            var graph = Bam.Core.V2.Graph.Instance;
            var solution = graph.MetaData as VSSolution;
            foreach (var project in solution)
            {
                project.FixupPerConfigurationData();

                var builder = new System.Text.StringBuilder();
                using (var xmlwriter = System.Xml.XmlWriter.Create(builder, settings))
                {
                    project.WriteTo(xmlwriter);
                }
                Bam.Core.Log.DebugMessage(builder.ToString());

                var projectPathDir = System.IO.Path.GetDirectoryName(project.ProjectPath);
                if (!System.IO.Directory.Exists(projectPathDir))
                {
                    System.IO.Directory.CreateDirectory(projectPathDir);
                }

                using (var xmlwriter = System.Xml.XmlWriter.Create(project.ProjectPath, settings))
                {
                    project.WriteTo(xmlwriter);
                }

                using (var xmlwriter = System.Xml.XmlWriter.Create(project.Filter.Path, settings))
                {
                    project.Filter.WriteTo(xmlwriter);
                }
            }

            var solutionPath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/solution.sln", null).Parse();
            using (var writer = new System.IO.StreamWriter(solutionPath))
            {
                writer.Write(solution.Serialize());
            }
        }
    }

    // TODO: add XML element
    public sealed class VSProjectObjectFile :
        VSSolutionMeta
    {
        public VSProjectObjectFile(
            Bam.Core.V2.Module module,
            Bam.Core.V2.TokenizedString objectFilePath,
            VSSolutionMeta.EPlatform platform)
            : base(module, VSProject.Type.NA, objectFilePath, platform)
        { }

        public Bam.Core.V2.TokenizedString Source
        {
            get;
            set;
        }

        public Bam.Core.V2.TokenizedString Output
        {
            get;
            set;
        }
    }

    public sealed class VSProjectCustomBuild :
        VSSolutionMeta
    {
        public VSProjectCustomBuild(
            Bam.Core.V2.Module module,
            VSSolutionMeta.EPlatform platform) :
            base(module, VSProject.Type.NA, null, platform)
        {
            this.Outputs = new Bam.Core.StringArray();
        }

        public string Command
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public Bam.Core.StringArray Outputs
        {
            get;
            private set;
        }
    }

    public sealed class VSProjectHeaderFile :
        VSSolutionMeta
    {
        public VSProjectHeaderFile(
            Bam.Core.V2.Module module,
            VSSolutionMeta.EPlatform platform) :
            base(module, VSProject.Type.NA, null, platform)
        {}

        public Bam.Core.V2.TokenizedString Source
        {
            get;
            set;
        }

        public VSProjectCustomBuild CustomBuild
        {
            get;
            set;
        }
    }

    public abstract class VSCommonProject :
        VSSolutionMeta
    {
        protected VSCommonProject(
            Bam.Core.V2.Module module,
            VSProject.Type type,
            Bam.Core.V2.TokenizedString outPath,
            EPlatform platform) :
            base(module, type, outPath, platform)
        {
        }

        public void AddObjectFile(Bam.Core.V2.Module module, Bam.Core.V2.Settings patchSettings)
        {
            this.Project.AddSourceFile(module, patchSettings, this.Configuration);
        }

        public void
        AddHeaderFile(
            C.V2.HeaderFile module)
        {
            if (null == module.MetaData)
            {
                // add a bog-standard header
                this.Project.AddHeaderFile(module);
            }
            else
            {
                var headerFile = module.MetaData as VSProjectHeaderFile;
                if (null == headerFile.CustomBuild)
                {
                    throw new Bam.Core.Exception("Header does not have a custom build");
                }
                var customBuild = headerFile.CustomBuild;
                this.Project.AddCustomBuildFile(module, customBuild, this.Configuration);
            }
        }

        public void
        SetCommonCompilationOptions(
            Bam.Core.V2.Module module,
            Bam.Core.V2.Settings settings)
        {
            this.Project.SetCommonCompilationOptions(module, settings, this.Configuration);
        }

        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands)
        {
            this.Project.AddPreBuildCommands(commands, this.Configuration);
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands)
        {
            this.Project.AddPostBuildCommands(commands, this.Configuration);
        }
    }

    public sealed class VSProjectHeaderLibrary :
        VSSolutionMeta
    {
        public VSProjectHeaderLibrary(
            Bam.Core.V2.Module module) :
            base(module, VSProject.Type.Utility, null, EPlatform.SixtyFour) // TODO: hard coded platform
        {}

        public void
        AddHeaderFile(
            C.V2.HeaderFile module)
        {
            this.Project.AddHeaderFile(module);
        }
    }

    // TODO: add XML document
    public sealed class VSProjectStaticLibrary :
        VSCommonProject
    {
        public VSProjectStaticLibrary(
            Bam.Core.V2.Module module,
            Bam.Core.V2.TokenizedString libraryPath,
            VSSolutionMeta.EPlatform platform) :
            base(module, VSProject.Type.StaticLibrary, libraryPath, platform)
        {
        }
    }

    public abstract class VSCommonLinkableProject :
        VSCommonProject
    {
        protected VSCommonLinkableProject(
            Bam.Core.V2.Module module,
            VSProject.Type type,
            Bam.Core.V2.TokenizedString outPath,
            EPlatform platform) :
            base(module, type, outPath, platform)
        {
            this.Libraries = new System.Collections.Generic.List<VSSolutionMeta>();
        }

        private System.Collections.Generic.List<VSSolutionMeta> Libraries
        {
            get;
            set;
        }

        public void
        AddStaticLibrary(
            VSProjectStaticLibrary library)
        {
            if (this.Libraries.Contains(library))
            {
                return;
            }

            this.Libraries.Add(library);
            this.Project.AddDependentProject(library.Project);
        }

        public void
        AddDynamicLibrary(
            VSProjectDynamicLibrary library)
        {
            if (this.Libraries.Contains(library))
            {
                return;
            }

            this.Libraries.Add(library);
            this.Project.AddDependentProject(library.Project);
        }
    }

    // TODO: add XML document
    public sealed class VSProjectDynamicLibrary :
        VSCommonLinkableProject
    {
        public VSProjectDynamicLibrary(
            Bam.Core.V2.Module module,
            Bam.Core.V2.TokenizedString libraryPath,
            VSSolutionMeta.EPlatform platform) :
            base(module, VSProject.Type.DynamicLibrary, libraryPath, platform)
        {
        }
    }

    // TODO: add XML document
    public sealed class VSProjectProgram :
        VSCommonLinkableProject
    {
        public VSProjectProgram(
            Bam.Core.V2.Module module,
            Bam.Core.V2.TokenizedString applicationPath,
            VSSolutionMeta.EPlatform platform) :
            base(module, VSProject.Type.Application, applicationPath, platform)
        {
        }
    }
#endif
}
    public sealed partial class VSSolutionBuilder :
        Bam.Core.IBuilder
    {
        private static System.Type
        GetProjectClassType()
        {
            var toolchainPackage = Bam.Core.State.PackageInfo["VisualC"];
            if (null != toolchainPackage)
            {
                string projectClassTypeName = null;
                switch (toolchainPackage.Version)
                {
                    case "8.0":
                    case "9.0":
                        projectClassTypeName = "VSSolutionBuilder.VCProject";
                        break;

                    case "10.0":
                    case "11.0":
                    case "12.0":
                        projectClassTypeName = "VSSolutionBuilder.VCXBuildProject";
                        break;

                    default:
                        throw new Bam.Core.Exception("Unrecognized VisualStudio version: '{0}'", toolchainPackage.Version);
                }

                var projectClassType = System.Type.GetType(projectClassTypeName);
                return projectClassType;
            }
            else
            {
                toolchainPackage = Bam.Core.State.PackageInfo["DotNetFramework"];
                if (null != toolchainPackage)
                {
                    var projectClassTypeName = "VSSolutionBuilder.CSBuildProject";
                    var projectClassType = System.Type.GetType(projectClassTypeName);
                    return projectClassType;
                }
                else
                {
                    throw new Bam.Core.Exception("Unable to locate a suitable toolchain package");
                }
            }
        }

        private static string
        GetConfigurationNameFromTarget(
            Bam.Core.Target target)
        {
            var platform = GetPlatformNameFromTarget(target);
            var configurationName = System.String.Format("{0}|{1}", ((Bam.Core.BaseTarget)target).ConfigurationName('p'), platform);
            return configurationName;
        }

        private static string
        GetConfigurationNameFromTarget(
            Bam.Core.Target target,
            string platformName)
        {
            var configurationName = System.String.Format("{0}|{1}", ((Bam.Core.BaseTarget)target).ConfigurationName('p'), platformName);
            return configurationName;
        }

        public static string
        GetPlatformNameFromTarget(
            Bam.Core.Target target)
        {
            string platform;
            if (target.HasPlatform(Bam.Core.EPlatform.Win32))
            {
                platform = "Win32";
            }
            else if (target.HasPlatform(Bam.Core.EPlatform.Win64))
            {
                platform = "x64";
            }
            else
            {
                throw new Bam.Core.Exception("Only Win32 and Win64 are supported platforms for VisualStudio projects");
            }

            return platform;
        }

        private static string
        UseOutDirMacro(
            string path,
            string outputDirectory)
        {
            var updatedPath = path.Replace(outputDirectory, "$(OutDir)");
            return updatedPath;
        }

        private static string
        UseIntDirMacro(
            string path,
            string intermediateDirectory)
        {
            var updatedPath = path.Replace(intermediateDirectory, "$(IntDir)");
            return updatedPath;
        }

        private static string
        UseProjectMacro(
            string path,
            string projectName)
        {
            var updatedPath = path.Replace(projectName, "$(ProjectName)");
            return updatedPath;
        }

        // semi-colons are the splitter in arguments
        private static readonly char PathSplitter = ';';

        private static string
        QuotePathsWithSpaces(
            string path,
            System.Uri projectUri)
        {
            if (path.Contains(new string(new char[] { PathSplitter })))
            {
                throw new Bam.Core.Exception("Path should not contain splitter");
            }

            var quote = new string(new char[] { '\"' });
            if (path.StartsWith(quote) && path.EndsWith(quote))
            {
                return path;
            }

            // remove any stray quotes
            var quotedPath = path.Trim(new char[] { '\"' });

            // only interested in paths
            if (quotedPath.Length < 2)
            {
                return quotedPath;
            }
            // need to test local drives as well as network paths
            var isLocalDrive = (quotedPath[1] == System.IO.Path.VolumeSeparatorChar);
            var isNetworkDrive = (quotedPath[0] == System.IO.Path.AltDirectorySeparatorChar &&
                                  quotedPath[1] == System.IO.Path.AltDirectorySeparatorChar);
            if (!isLocalDrive && !isNetworkDrive)
            {
                return quotedPath;
            }

            quotedPath = Bam.Core.RelativePathUtilities.GetPath(quotedPath, projectUri);
            if (quotedPath.Contains(" "))
            {
                quotedPath = System.String.Format("\"{0}\"", quotedPath);
            }

            return quotedPath;
        }

        internal static string
        RefactorPathForVCProj(
            string path,
            string outputDirectoryPath,
            string intermediateDirectoryPath,
            string projectName,
            System.Uri projectUri)
        {
            if (System.String.IsNullOrEmpty(path))
            {
                Bam.Core.Log.DebugMessage("Cannot refactor an empty path for VisualStudio projects");
                return path;
            }

            var splitPath = path.Split(PathSplitter);

            var joinedPath = new System.Text.StringBuilder();
            foreach (var split in splitPath)
            {
                if (0 == split.Length)
                {
                    continue;
                }

                var refactoredPath = split;
                if (null != outputDirectoryPath)
                {
                    refactoredPath = UseOutDirMacro(refactoredPath, outputDirectoryPath);
                }
                refactoredPath = UseIntDirMacro(refactoredPath, intermediateDirectoryPath);
                refactoredPath = UseProjectMacro(refactoredPath, projectName);
                refactoredPath = QuotePathsWithSpaces(refactoredPath, projectUri);

                joinedPath.AppendFormat("{0};", refactoredPath);
            }

            return joinedPath.ToString().TrimEnd(PathSplitter);
        }

        internal static string
        RefactorPathForVCProj(
            string path,
            Bam.Core.Location outputDirectory,
            Bam.Core.Location intermediateDirectory,
            string projectName,
            System.Uri projectUri)
        {
            var outputDirectoryPath = (outputDirectory != null) ? outputDirectory.GetSinglePath() : null;
            var intermediateDirectoryPath = intermediateDirectory.GetSinglePath();
            return RefactorPathForVCProj(path, outputDirectoryPath, intermediateDirectoryPath, projectName, projectUri);
        }

        // no intermediate directory
        internal static string
        RefactorPathForVCProj(
            string path,
            string outputDirectoryPath,
            string projectName,
            System.Uri projectUri)
        {
            if (System.String.IsNullOrEmpty(path))
            {
                Bam.Core.Log.DebugMessage("Cannot refactor an empty path for VisualStudio projects");
                return path;
            }

            var splitPath = path.Split(PathSplitter);

            var joinedPath = new System.Text.StringBuilder();
            foreach (var split in splitPath)
            {
                var refactoredPath = split;
                refactoredPath = UseOutDirMacro(refactoredPath, outputDirectoryPath);
                refactoredPath = UseProjectMacro(refactoredPath, projectName);
                refactoredPath = QuotePathsWithSpaces(refactoredPath, projectUri);

                joinedPath.AppendFormat("{0};", refactoredPath);
            }

            return joinedPath.ToString().TrimEnd(PathSplitter);
        }

        // no intermediate directory
        internal static string
        RefactorPathForVCProj(
            string path,
            Bam.Core.Location outputDirectory,
            string projectName,
            System.Uri projectUri)
        {
            var outputDirectoryPath = outputDirectory.GetSinglePath();
            return RefactorPathForVCProj(path, outputDirectoryPath, projectName, projectUri);
        }

        private SolutionFile solutionFile;
    }
}

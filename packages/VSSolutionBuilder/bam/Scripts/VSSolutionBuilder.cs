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
            this.CharacterSet = C.ECharacterSet.NotSet;
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

        private C.ECharacterSet CharacterSet
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
        SetCharacterSet(
            C.ECharacterSet charSet)
        {
            this.CharacterSet = charSet;
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
            lock (this.SettingGroups)
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
                (patchSettings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, settings, condition: this.ConditionText);
            }
            this.Sources.AddUnique(settings);
            this.Project.AddSource(settings);
        }

        public void
        AddOtherFile(
            Bam.Core.V2.IInputPath other)
        {
            var otherGroup = this.Project.GetUniqueSettingsGroup(VSSettingsGroup.ESettingsGroup.CustomBuild, other.InputPath);
            this.Project.AddOtherFile(otherGroup);
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
            lock (this.ProjectMap)
            {
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
        private static readonly string OtherGroupName = "Other Files";
        private Bam.Core.Array<VSSettingsGroup> Source = new Bam.Core.Array<VSSettingsGroup>();
        private Bam.Core.Array<VSSettingsGroup> Headers = new Bam.Core.Array<VSSettingsGroup>();
        private Bam.Core.Array<VSSettingsGroup> Others = new Bam.Core.Array<VSSettingsGroup>();

        public void
        AddHeader(
            Bam.Core.V2.TokenizedString path)
        {
            lock (this.Headers)
            {
                if (!this.Headers.Any(item => item.Include.Parse() == path.Parse()))
                {
                    var group = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.Header, include: path);
                    group.AddSetting("Filter", HeaderGroupName);
                    this.Headers.AddUnique(group);
                }
            }
        }

        public void
        AddSource(
            Bam.Core.V2.TokenizedString path)
        {
            lock (this.Source)
            {
                if (!this.Source.Any(item => item.Include.Parse() == path.Parse()))
                {
                    var group = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.Compiler, include: path);
                    group.AddSetting("Filter", SourceGroupName);
                    this.Source.AddUnique(group);
                }
            }
        }

        public void
        AddOther(
            Bam.Core.V2.TokenizedString path)
        {
            lock (this.Others)
            {
                if (!this.Others.Any(item => item.Include.Parse() == path.Parse()))
                {
                    var group = new VSSettingsGroup(VSSettingsGroup.ESettingsGroup.CustomBuild, include: path);
                    group.AddSetting("Filter", OtherGroupName);
                    this.Others.AddUnique(group);
                }
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
            if (this.Others.Count > 0)
            {
                createXML(containerEl, OtherGroupName, this.Others);
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
            this.Others = new Bam.Core.Array<VSSettingsGroup>();
            this.Filter = new VSProjectFilter();
            this.OrderOnlyDependentProjects = new Bam.Core.Array<VSProject>();
            this.LinkDependentProjects = new Bam.Core.Array<VSProject>();
        }

        public VSProjectConfiguration
        GetConfiguration(
            Bam.Core.V2.Module module)
        {
            lock (this.Configurations)
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
        }

        public  VSSettingsGroup
        GetUniqueSettingsGroup(
            VSSettingsGroup.ESettingsGroup group,
            Bam.Core.V2.TokenizedString include = null)
        {
            lock (this.ProjectSettings)
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
        AddOtherFile(
            VSSettingsGroup other)
        {
            this.Others.AddUnique(other);
            this.Filter.AddOther(other.Include);
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

        private Bam.Core.Array<VSSettingsGroup> Others
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
            if (this.Others.Count > 0)
            {
                var otherGroup = document.CreateVSItemGroup(parentEl: projectEl);
                foreach (var group in this.Others)
                {
                    group.Serialize(document, otherGroup);
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

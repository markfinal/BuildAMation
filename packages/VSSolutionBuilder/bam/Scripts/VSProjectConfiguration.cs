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
namespace VSSolutionBuilder
{
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

        public VSProjectConfiguration(
            VSProject project,
            Bam.Core.Module module,
            Bam.Core.EPlatform platform)
        {
            this.Project = project;
            this.Module = module;
            this.Configuration = module.BuildEnvironment.Configuration;
            this.Platform = platform;
            this.FullName = this.CombinedName;

            this.Type = EType.NA;

            var visualCMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            this.PlatformToolset = visualCMeta.PlatformToolset;
            this.UseDebugLibraries = false;
            this.CharacterSet = C.ECharacterSet.NotSet;
            this.WholeProgramOptimization = false;// TODO: false is consistent with Native builds (module.BuildEnvironment.Configuration != Bam.Core.EConfiguration.Debug);

            this.SettingGroups = new Bam.Core.Array<VSSettingsGroup>();
            this.Sources = new Bam.Core.Array<VSSettingsGroup>();
            this.Headers = new Bam.Core.Array<VSSettingsGroup>();
            this.ResourceFiles = new Bam.Core.Array<VSSettingsGroup>();
            this.AssemblyFiles = new Bam.Core.Array<VSSettingsGroup>();

            this.OrderOnlyDependentProjects = new Bam.Core.Array<VSProject>();
            this.LinkDependentProjects = new Bam.Core.Array<VSProject>();

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

        public Bam.Core.Module Module
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

        public string PlatformToolset
        {
            get;
            private set;
        }

        private bool UseDebugLibraries
        {
            get;
            set;
        }

        private bool WholeProgramOptimization
        {
            get;
            set;
        }

        private Bam.Core.TokenizedString OutputDirectory
        {
            get;
            set;
        }

        private Bam.Core.TokenizedString IntermediateDirectory
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

        private Bam.Core.Array<VSSettingsGroup> Headers
        {
            get;
            set;
        }

        private Bam.Core.Array<VSSettingsGroup> ResourceFiles
        {
            get;
            set;
        }

        private Bam.Core.Array<VSSettingsGroup> AssemblyFiles
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
                if (this.Type == type)
                {
                    return;
                }
                throw new Bam.Core.Exception("Project configuration already has type {0}. Cannot change it to {1}", this.Type.ToString(), type.ToString());
            }

            this.Type = type;
        }

        public C.ECharacterSet CharacterSet
        {
            get;
            set;
        }

        public void
        EnableIntermediatePath()
        {
            this.IntermediateDirectory = Bam.Core.TokenizedString.Create(
                "$(packagebuilddir)/$(encapsulatingmodulename)/$(config)/",
                this.Module
            );
            this.IntermediateDirectory.Parse();
        }

        public bool EnableManifest
        {
            get;
            set;
        }

        private Bam.Core.TokenizedString _OutputFile;
        public Bam.Core.TokenizedString OutputFile
        {
            get
            {
                return this._OutputFile;
            }
            set
            {
                this._OutputFile = value;
                // Note: MSB8004 requires the OutDir to have a trailing slash
                this.OutputDirectory = Bam.Core.TokenizedString.Create(
                    "@dir($(0))\\",
                    null,
                    new Bam.Core.TokenizedStringArray { value }
                );
                this.OutputDirectory.Parse();
            }
        }

        public VSSettingsGroup
        GetSettingsGroup(
            VSSettingsGroup.ESettingsGroup group,
            Bam.Core.TokenizedString include = null,
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
                        // TODO: can this be a TokenizedString hash compare?
                        if ((null != include) && (settings.Include.ToString().Equals(include.ToString(), System.StringComparison.Ordinal)))
                        {
                            return settings;
                        }
                    }
                }

                var newGroup = uniqueToProject ?
                    this.Project.GetUniqueSettingsGroup(this.Module, group, include) :
                    new VSSettingsGroup(this.Project, this.Module, group, include);
                this.SettingGroups.Add(newGroup);
                return newGroup;
            }
        }

        public void
        AddHeaderFile(
            C.HeaderFile header)
        {
            var headerGroup = this.Project.GetUniqueSettingsGroup(
                this.Module,
                VSSettingsGroup.ESettingsGroup.Header,
                header.InputPath
            );
            this.Headers.AddUnique(headerGroup);
            this.Project.AddHeader(headerGroup, this);
        }

        public void
        AddSourceFile(
            Bam.Core.Module module,
            Bam.Core.Settings patchSettings)
        {
            var settings = module.MetaData as VSSettingsGroup;
            if (null != patchSettings)
            {
                VisualStudioProcessor.VSSolutionConversion.Convert(
                    patchSettings,
                    module.Settings.GetType(),
                    module,
                    settings,
                    this,
                    condition: this.ConditionText
                );
            }
            this.Sources.AddUnique(settings);
            this.Project.AddSource(settings, this);
        }

        public void
        AddOtherFile(
            Bam.Core.IInputPath other)
        {
            var otherGroup = this.Project.GetUniqueSettingsGroup(
                this.Module,
                VSSettingsGroup.ESettingsGroup.CustomBuild,
                other.InputPath
            );
            this.Project.AddOtherFile(otherGroup, this);
        }

        public void
        AddOtherFile(
            Bam.Core.TokenizedString other_path)
        {
            var otherGroup = this.Project.GetUniqueSettingsGroup(
                this.Module,
                VSSettingsGroup.ESettingsGroup.CustomBuild,
                other_path
            );
            this.Project.AddOtherFile(otherGroup, this);
        }

        public void
        AddResourceFile(
            C.WinResource resource,
            Bam.Core.Settings patchSettings)
        {
            var settings = resource.MetaData as VSSettingsGroup;
            if (null != patchSettings)
            {
                VisualStudioProcessor.VSSolutionConversion.Convert(
                    patchSettings,
                    resource.Settings.GetType(),
                    resource,
                    settings,
                    this,
                    condition: this.ConditionText
                );
            }
            var resourceGroup = this.Project.GetUniqueSettingsGroup(this.Module, VSSettingsGroup.ESettingsGroup.Resource, resource.InputPath);
            this.ResourceFiles.AddUnique(resourceGroup);
            this.Project.AddResourceFile(resourceGroup, this);
        }

        public void
        AddAssemblyFile(
            C.AssembledObjectFile assembler,
            Bam.Core.Settings patchSettings)
        {
            var settings = assembler.MetaData as VSSettingsGroup;
            if (null != patchSettings)
            {
                VisualStudioProcessor.VSSolutionConversion.Convert(
                    patchSettings,
                    assembler.Settings.GetType(),
                    assembler,
                    settings,
                    this,
                    condition: this.ConditionText
                );
            }
            var assemblyGroup = this.Project.GetUniqueSettingsGroup(this.Module, VSSettingsGroup.ESettingsGroup.CustomBuild, assembler.InputPath);
            this.AssemblyFiles.AddUnique(assemblyGroup);
            this.Project.AddAssemblyFile(assemblyGroup, this);
        }

        public void
        RequiresProject(
            VSProject project)
        {
            this.OrderOnlyDependentProjects.AddUnique(project);
            this.Project.AddOrderOnlyDependency(project);
        }

        public void
        LinkAgainstProject(
            VSProject project)
        {
            this.LinkDependentProjects.AddUnique(project);
            this.Project.AddLinkDependency(project);
        }

        public bool
        ContainsSource(
            VSSettingsGroup sourceGroup)
        {
            return this.Sources.Contains(sourceGroup);
        }

        public bool
        ContainsHeader(
            VSSettingsGroup headerGroup)
        {
            return this.Headers.Contains(headerGroup);
        }

        public bool
        ContainsResourceFile(
            VSSettingsGroup resourceFileGroup)
        {
            return this.ResourceFiles.Contains(resourceFileGroup);
        }

        public bool
        ContainsAssemblyFile(
            VSSettingsGroup assemblyFileGroup)
        {
            return this.AssemblyFiles.Contains(assemblyFileGroup);
        }

        public bool
        ContainsOrderOnlyDependency(
            VSProject project)
        {
            return this.OrderOnlyDependentProjects.Contains(project);
        }

        public bool
        ContainsLinkDependency(
            VSProject project)
        {
            return this.LinkDependentProjects.Contains(project);
        }

        public void
        AddPreBuildCommand(
            string command)
        {
            lock (this.PreBuildCommands)
            {
                this.PreBuildCommands.Add(command);
            }
        }

        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands)
        {
            lock (this.PreBuildCommands)
            {
                this.PreBuildCommands.AddRange(commands);
            }
        }

        public void
        AddPostBuildCommand(
            string command)
        {
            lock (this.PostBuildCommands)
            {
                this.PostBuildCommands.Add(command);
            }
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands)
        {
            lock (this.PostBuildCommands)
            {
                this.PostBuildCommands.AddRange(commands);
            }
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
                if (System.String.IsNullOrEmpty(this.PlatformToolset))
                {
                    throw new Bam.Core.Exception("Platform toolset not set for project {0}", this.Project.ProjectPath);
                }
            }
            var propGroup = document.CreateVSPropertyGroup(label: "Configuration", condition: this.ConditionText, parentEl: parentEl);
            document.CreateVSElement("ConfigurationType", value: this.Type.ToString(), parentEl: propGroup);
            document.CreateVSElement("PlatformToolset", value: this.PlatformToolset.ToString(), parentEl: propGroup);
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
            if (null != this.OutputFile)
            {
                document.CreateVSElement(
                    "OutDir",
                    value: this.ToRelativePath(this.OutputDirectory, isOutputDir: true),
                    parentEl: propGroup
                );

                var targetNameTS = this.Module.CreateTokenizedString("@basename($(0))", this.OutputFile);
                targetNameTS.Parse();
                var targetName = targetNameTS.ToString();
                if (!string.IsNullOrEmpty(targetName))
                {
                    var filenameTS = this.Module.CreateTokenizedString("@filename($(0))", this.OutputFile);
                    filenameTS.Parse();
                    var filename = filenameTS.ToString();
                    var ext = filename.Replace(targetName, string.Empty);

                    document.CreateVSElement("TargetName", value: targetName, parentEl: propGroup);
                    document.CreateVSElement("TargetExt", value: ext, parentEl: propGroup);
                }
            }
            if (null != this.IntermediateDirectory)
            {
                document.CreateVSElement(
                    "IntDir",
                    value: @"$(ProjectDir)$(ProjectName)\$(Configuration)\",
                    parentEl: propGroup);
            }
            document.CreateVSElement("GenerateManifest", value: this.EnableManifest.ToString().ToLower(), parentEl: propGroup);
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
                var preBuildGroup = new VSSettingsGroup(this.Project, this.Module, VSSettingsGroup.ESettingsGroup.PreBuild);
                preBuildGroup.AddSetting(
                    "Command",
                    this.PreBuildCommands.ToString(System.Environment.NewLine)
                );
                preBuildGroup.Serialize(document, itemDefnGroup);
            }
            if (this.PostBuildCommands.Count > 0)
            {
                var preBuildGroup = new VSSettingsGroup(this.Project, this.Module, VSSettingsGroup.ESettingsGroup.PostBuild);
                preBuildGroup.AddSetting(
                    "Command",
                    this.PostBuildCommands.ToString(System.Environment.NewLine)
                );
                preBuildGroup.Serialize(document, itemDefnGroup);
            }
        }

        public string
        ToRelativePath(
            string joinedPaths)
        {
            var paths = joinedPaths.Split(new[] { ';' });
            var relative_paths = this.ToRelativePaths(paths);
            return System.String.Join(";", relative_paths);
        }

        public string
        ToRelativePath(
            Bam.Core.TokenizedString path,
            bool isOutputDir = false)
        {
            // TODO: in C#7 use a local function to yield return an IEnumerable with the single value
            return this.ToRelativePaths(
                new[] { path.ToString() },
                isOutputDir: isOutputDir
            );
        }

        public string
        ToRelativePaths(
            Bam.Core.TokenizedStringArray paths)
        {
            return ToRelativePaths(paths.ToEnumerableWithoutDuplicates().Select(item => item.ToString()));
        }

        public string
        ToRelativePaths(
            System.Collections.Generic.IEnumerable<string> paths,
            bool isOutputDir = false)
        {
            var programFiles = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86);

            var distinct_source_paths = paths.Distinct();
            var output_paths = new Bam.Core.StringArray();
            var invalid_chars = System.IO.Path.GetInvalidPathChars();
            foreach (var path in distinct_source_paths)
            {
                if (path.IndexOfAny(invalid_chars) >= 0 || // speaks for itself
                    path.StartsWith("%(") ||               // VS environment variable
                    path.StartsWith(programFiles) ||       // special folders
                    path.StartsWith(programFilesX86))
                {
                    output_paths.Add(path);
                    continue;
                }

                // get relative paths to interesting macros
                // KEY=macro, VALUE=original path
                // projects without output, e.g. headerlibrary projects, will not have a valid OutputDirectory property
                var mapping = new System.Collections.Generic.Dictionary<string, string>();
                mapping.Add("$(ProjectDir)", this.Project.ProjectPath);
                if (null != this.OutputDirectory && !isOutputDir)
                {
                    mapping.Add("$(OutDir)", this.OutputDirectory.ToString());
                }
                // cannot generate the OutDir in terms of the IntDir
                if (null != this.IntermediateDirectory && !isOutputDir)
                {
                    mapping.Add("$(IntDir)", this.IntermediateDirectory.ToString());
                }

                System.Collections.Generic.KeyValuePair<string, string> candidate = default(System.Collections.Generic.KeyValuePair<string, string>);
                foreach (var item in mapping)
                {
                    var relative_path = Bam.Core.RelativePathUtilities.GetRelativePathFromRoot(
                        System.IO.Path.GetDirectoryName(item.Value),
                        path
                    );
                    if (Bam.Core.RelativePathUtilities.IsPathAbsolute(relative_path))
                    {
                        continue;
                    }
                    if (null == candidate.Key)
                    {
                        candidate = new System.Collections.Generic.KeyValuePair<string, string>(item.Key, relative_path);
                    }
                    else
                    {
                        // already a candidate, use the shorter of that and the current option
                        if (relative_path.Length <= candidate.Value.Length)
                        {
                            candidate = new System.Collections.Generic.KeyValuePair<string, string>(item.Key, relative_path);
                        }
                    }
                }
                if (null == candidate.Key)
                {
                    // no candidate, use original path
                    output_paths.Add(path);
                }
                else
                {
                    // prefix the relative path with the VS macro
                    output_paths.Add(
                        System.String.Format(
                            "{0}{1}",
                            candidate.Key,
                            candidate.Value
                        )
                    );
                }
            }
            return output_paths.ToString(';');
        }
    }
}

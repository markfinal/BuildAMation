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
    /// <summary>
    /// Class respresenting an abstraction of a project configuration, consisting of platform and configuration
    /// </summary>
    public sealed class VSProjectConfiguration
    {
        /// <summary>
        /// Type of output
        /// </summary>
        public enum EType
        {
            NA,             //!< Unknown
            Application,    //!< Application
            DynamicLibrary, //!< Dynamic library
            StaticLibrary,  //!< Static library
            Utility         //!< Utility (not buildable)
        }

        /// <summary>
        /// Create an instance of the configuration
        /// </summary>
        /// <param name="project">Project to own the configuration</param>
        /// <param name="module">Module associated</param>
        /// <param name="platform">Platform on which the configuration applies.</param>
        public VSProjectConfiguration(
            VSProject project,
            Bam.Core.EPlatform platform)
        {
            this.Project = project;
            this.Configuration = project.Module.BuildEnvironment.Configuration;
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

        /// <summary>
        /// Get the configuration name
        /// </summary>
        public string ConfigurationName => this.Configuration.ToString();

        /// <summary>
        /// Get the platform name
        /// </summary>
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

        private string CombinedName => $"{this.ConfigurationName}|{this.PlatformName}";

        /// <summary>
        /// Get the 'condition' text for the configuration
        /// </summary>
        public string ConditionText => $"'$(Configuration)|$(Platform)'=='{this.CombinedName}'";

        /// <summary>
        /// Get the full name of the configuration
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Get the EConfiguration associated with the configuration
        /// </summary>
        public Bam.Core.EConfiguration Configuration { get; private set; }

        /// <summary>
        /// Get the EPlatform associated with the configuration
        /// </summary>
        public Bam.Core.EPlatform Platform { get; private set; }

        /// <summary>
        /// Get the VSProject associated with the configuration
        /// </summary>
        public VSProject Project { get; private set; }

        /// <summary>
        /// Get the type of the configuration
        /// </summary>
        public EType Type { get; private set; }

        /// <summary>
        /// Get the toolset associated with the configuration
        /// </summary>
        public string PlatformToolset { get; private set; }

        private bool UseDebugLibraries { get; set; }

        private bool WholeProgramOptimization { get; set; }

        private Bam.Core.TokenizedString OutputDirectory { get; set; }

        private Bam.Core.TokenizedString IntermediateDirectory { get; set; }

        private Bam.Core.Array<VSSettingsGroup> SettingGroups { get; set; }

        private Bam.Core.Array<VSSettingsGroup> Sources { get; set; }

        private Bam.Core.Array<VSSettingsGroup> Headers { get; set; }

        private Bam.Core.Array<VSSettingsGroup> ResourceFiles { get; set; }

        private Bam.Core.Array<VSSettingsGroup> AssemblyFiles { get; set; }

        private Bam.Core.Array<VSProject> OrderOnlyDependentProjects { get; set; }

        private Bam.Core.Array<VSProject> LinkDependentProjects { get; set; }

        private Bam.Core.StringArray PreBuildCommands { get; set; }

        private Bam.Core.StringArray PostBuildCommands { get; set; }

        /// <summary>
        /// Set the type of the configuration
        /// </summary>
        /// <param name="type">Set the new type of the configuration</param>
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
                throw new Bam.Core.Exception($"Project configuration already has type {this.Type.ToString()}. Cannot change it to {type.ToString()}");
            }

            this.Type = type;
        }

        /// <summary>
        /// Get or set the character set in the configuration
        /// </summary>
        public C.ECharacterSet CharacterSet { get; set; }

        /// <summary>
        /// Enable the use of the intermediate directory
        /// </summary>
        public void
        EnableIntermediatePath()
        {
            var module = this.Project.Module;
            this.IntermediateDirectory = module.CreateTokenizedString(
                "$(packagebuilddir)/$(0)/$(config)/",
                new[] {module.EncapsulatingModule.Macros[Bam.Core.ModuleMacroNames.ModuleName] }
            );
            this.IntermediateDirectory.Parse();
        }

        /// <summary>
        /// Get or set whether the manifest is enabled
        /// </summary>
        public bool EnableManifest { get; set; }

        private Bam.Core.TokenizedString _OutputFile;
        /// <summary>
        /// Get or set the output file of the configuration
        /// </summary>
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

        /// <summary>
        /// Get the settings group from the configuration for a path.
        /// If the path is non-null, then the settings group created is unique to the path in the project.
        /// If the path is null, then it is a group that's shared among all files in the project.
        /// </summary>
        /// <param name="group">Type of group</param>
        /// <param name="path">Path associated with the settings group.</param>
        /// <returns>Either the group already created, or a new settings group.</returns>
        public VSSettingsGroup
        GetSettingsGroup(
            VSSettingsGroup.ESettingsGroup group,
            Bam.Core.TokenizedString path)
        {
            lock (this.SettingGroups)
            {
                foreach (var settings in this.SettingGroups)
                {
                    if (null == path)
                    {
                        if ((null == settings.Path) && (settings.Group == group))
                        {
                            return settings;
                        }
                    }
                    else
                    {
                        // ignore group, as files can mutate between them during the buildprocess (e.g. headers into custom builds)
                        // TODO: can this be a TokenizedString hash compare?
                        if (settings.Path.ToString().Equals(path.ToString(), System.StringComparison.Ordinal))
                        {
                            return settings;
                        }
                    }
                }

                var newGroup = (null != path) ?
                    this.Project.GetUniqueSettingsGroup(this.Project.Module, group, path) :
                    new VSSettingsGroup(this.Project, group, null);
                this.SettingGroups.Add(newGroup);
                return newGroup;
            }
        }

        /// <summary>
        /// Add a header file to the configuration.
        /// </summary>
        /// <param name="header">Header to add</param>
        public void
        AddHeaderFile(
            C.HeaderFile header)
        {
            var headerGroup = this.Project.GetUniqueSettingsGroup(
                this.Project.Module,
                VSSettingsGroup.ESettingsGroup.Header,
                header.InputPath
            );
            this.Headers.AddUnique(headerGroup);
            this.Project.AddHeader(headerGroup);
        }

        /// <summary>
        /// Add a source file to the configuration, with optional per-source settings.
        /// </summary>
        /// <param name="module">Module representing the source file.</param>
        /// <param name="patchSettings">Settings representing any settings specified to this source file. Null if there are none.</param>
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
            this.Project.AddSource(settings);
        }

        /// <summary>
        /// Add an arbitrary file to the configuration.
        /// </summary>
        /// <param name="other">File specified by an Bam.Core.IInputPath.</param>
        public void
        AddOtherFile(
            Bam.Core.IInputPath other)
        {
            var otherGroup = this.Project.GetUniqueSettingsGroup(
                this.Project.Module,
                VSSettingsGroup.ESettingsGroup.CustomBuild,
                other.InputPath
            );
            this.Project.AddOtherFile(otherGroup);
        }

        /// <summary>
        /// Add an arbitrary file to the configuration.
        /// </summary>
        /// <param name="other_path">Bam.Core.TokenizedString for the path.</param>
        public void
        AddOtherFile(
            Bam.Core.TokenizedString other_path)
        {
            var otherGroup = this.Project.GetUniqueSettingsGroup(
                this.Project.Module,
                VSSettingsGroup.ESettingsGroup.CustomBuild,
                other_path
            );
            this.Project.AddOtherFile(otherGroup);
        }

        /// <summary>
        /// Add a Windows resource file to the configuration.
        /// </summary>
        /// <param name="resource">Resource file module.</param>
        /// <param name="patchSettings">Optional settings for this specific resource file. Null if none.</param>
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
            var resourceGroup = this.Project.GetUniqueSettingsGroup(
                this.Project.Module,
                VSSettingsGroup.ESettingsGroup.Resource,
                resource.InputPath
            );
            this.ResourceFiles.AddUnique(resourceGroup);
            this.Project.AddResourceFile(resourceGroup);
        }

        /// <summary>
        /// Add an assembly file to this configuration with optional settings.
        /// </summary>
        /// <param name="assembler">Assembler file to add.</param>
        /// <param name="patchSettings">Settings specific to this file. Or null if there are none.</param>
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
            var assemblyGroup = this.Project.GetUniqueSettingsGroup(
                this.Project.Module,
                VSSettingsGroup.ESettingsGroup.CustomBuild,
                assembler.InputPath
            );
            this.AssemblyFiles.AddUnique(assemblyGroup);
            this.Project.AddAssemblyFile(assemblyGroup);
        }

        /// <summary>
        /// Add a project that must exist but does not need to be linked against.
        /// </summary>
        /// <param name="project">The project required to exist.</param>
        public void
        RequiresProject(
            VSProject project)
        {
            this.OrderOnlyDependentProjects.AddUnique(project);
            this.Project.AddOrderOnlyDependency(project);
        }

        /// <summary>
        /// Add a project that must both exist and be linked against.
        /// </summary>
        /// <param name="project">The project to be linked against.</param>
        public void
        LinkAgainstProject(
            VSProject project)
        {
            this.LinkDependentProjects.AddUnique(project);
            this.Project.AddLinkDependency(project);
        }

        /// <summary>
        /// Query if this configuration contains the specified settings group (which represents a source file).
        /// </summary>
        /// <param name="sourceGroup">Settings group that represents a source file.</param>
        /// <returns>True if the settings group is in the configuration. False otherwise.</returns>
        public bool
        ContainsSource(
            VSSettingsGroup sourceGroup)
        {
            return this.Sources.Contains(sourceGroup);
        }

        /// <summary>
        /// Query if this configuration contains the specified settings group (which represents a header file).
        /// </summary>
        /// <param name="headerGroup">Settings group that represents a header file.</param>
        /// <returns>True if the settings group is in the configuration. False otherwise.</returns>
        public bool
        ContainsHeader(
            VSSettingsGroup headerGroup)
        {
            return this.Headers.Contains(headerGroup);
        }

        /// <summary>
        /// Query if this configuration contains the specified settings group (which represents a Windows resource file).
        /// </summary>
        /// <param name="resourceFileGroup">Settings group that represents a Windows resource file.</param>
        /// <returns>True if the settings group is in the configuration. False otherwise.</returns>
        public bool
        ContainsResourceFile(
            VSSettingsGroup resourceFileGroup)
        {
            return this.ResourceFiles.Contains(resourceFileGroup);
        }

        /// <summary>
        /// Query if this configuration contains the specified settings group (which represents an assembly file).
        /// </summary>
        /// <param name="assemblyFileGroup">Settings group that represents an assembly file.</param>
        /// <returns>True if the settings group is in the configuration. False otherwise.</returns>
        public bool
        ContainsAssemblyFile(
            VSSettingsGroup assemblyFileGroup)
        {
            return this.AssemblyFiles.Contains(assemblyFileGroup);
        }

        /// <summary>
        /// Query if a project is an order only dependency in this configuration.
        /// </summary>
        /// <param name="project">Project to query for.</param>
        /// <returns>True if the project is an order only dependency. False otherwise.</returns>
        public bool
        ContainsOrderOnlyDependency(
            VSProject project)
        {
            return this.OrderOnlyDependentProjects.Contains(project);
        }

        /// <summary>
        /// Query if a project is a link-time dependency in this configuration.
        /// </summary>
        /// <param name="project">Project to query for.</param>
        /// <returns>True if the project is a link-time dependency. False othewrise.</returns>
        public bool
        ContainsLinkDependency(
            VSProject project)
        {
            return this.LinkDependentProjects.Contains(project);
        }

        /// <summary>
        /// Add a single command to the prebuild commands for this configuration.
        /// </summary>
        /// <param name="command">Command to add</param>
        public void
        AddPreBuildCommand(
            string command)
        {
            lock (this.PreBuildCommands)
            {
                this.PreBuildCommands.Add(command);
            }
        }

        /// <summary>
        /// Add a list of commands to the prebuild commands for this configuration.
        /// </summary>
        /// <param name="commands">List of commands to add.</param>
        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands)
        {
            lock (this.PreBuildCommands)
            {
                this.PreBuildCommands.AddRange(commands);
            }
        }

        /// <summary>
        /// Add a single command to the postbuild commands for this configuration.
        /// </summary>
        /// <param name="command">Command to add</param>
        public void
        AddPostBuildCommand(
            string command)
        {
            lock (this.PostBuildCommands)
            {
                this.PostBuildCommands.Add(command);
            }
        }

        /// <summary>
        /// Add a list of commands to the postbuild commands for this configuration.
        /// </summary>
        /// <param name="commands">List of commands to add.</param>
        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands)
        {
            lock (this.PostBuildCommands)
            {
                this.PostBuildCommands.AddRange(commands);
            }
        }

        /// <summary>
        /// Serialize the configuration properties to an XML document.
        /// </summary>
        /// <param name="document">XML document to serialize to.</param>
        /// <param name="parentEl">Parent XML element to add this to.</param>
        public void
        SerializeProperties(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentEl)
        {
            if (this.Type == EType.NA)
            {
                Bam.Core.Log.DebugMessage($"Defaulting project {this.Project.ProjectPath} to type Utility");
                this.Type = EType.Utility;
                this.EnableIntermediatePath();
            }
            else
            {
                if (System.String.IsNullOrEmpty(this.PlatformToolset))
                {
                    throw new Bam.Core.Exception($"Platform toolset not set for project {this.Project.ProjectPath}");
                }
            }
            var propGroup = document.CreateVSPropertyGroup(label: "Configuration", condition: this.ConditionText, parentEl: parentEl);
            document.CreateVSElement("ConfigurationType", value: this.Type.ToString(), parentEl: propGroup);
            document.CreateVSElement("PlatformToolset", value: this.PlatformToolset.ToString(), parentEl: propGroup);
            document.CreateVSElement("UseDebugLibraries", value: this.UseDebugLibraries.ToString().ToLower(), parentEl: propGroup);
            document.CreateVSElement("CharacterSet", value: this.CharacterSet.ToString(), parentEl: propGroup);
            document.CreateVSElement("WholeProgramOptimization", value: this.WholeProgramOptimization.ToString().ToLower(), parentEl: propGroup);
        }

        /// <summary>
        /// Serialize the configuration paths to an XML document.
        /// </summary>
        /// <param name="document">XML document to serialize to.</param>
        /// <param name="parentEl">Parent XML element to add this to.</param>
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

                var module = this.Project.Module;
                var targetNameTS = module.CreateTokenizedString("@basename($(0))", this.OutputFile);
                targetNameTS.Parse();
                var targetName = targetNameTS.ToString();
                if (!string.IsNullOrEmpty(targetName))
                {
                    var filenameTS = module.CreateTokenizedString("@filename($(0))", this.OutputFile);
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

        /// <summary>
        /// Serialize the configuration settings to an XML document.
        /// </summary>
        /// <param name="document">XML document to serialize to.</param>
        /// <param name="parentEl">Parent XML element to add this to.</param>
        public void
        SerializeSettings(
            System.Xml.XmlDocument document,
            System.Xml.XmlElement parentEl)
        {
            var itemDefnGroup = document.CreateVSItemDefinitionGroup(condition: this.ConditionText, parentEl: parentEl);
            foreach (var group in this.SettingGroups)
            {
                if (group.Path != null)
                {
                    continue;
                }
                group.Serialize(document, itemDefnGroup);
            }

            if (this.PreBuildCommands.Count > 0)
            {
                var preBuildGroup = new VSSettingsGroup(this.Project, VSSettingsGroup.ESettingsGroup.PreBuild, null);
                preBuildGroup.AddSetting(
                    "Command",
                    this.PreBuildCommands.ToString(System.Environment.NewLine)
                );
                preBuildGroup.Serialize(document, itemDefnGroup);
            }
            if (this.PostBuildCommands.Count > 0)
            {
                var preBuildGroup = new VSSettingsGroup(this.Project, VSSettingsGroup.ESettingsGroup.PostBuild, null);
                preBuildGroup.AddSetting(
                    "Command",
                    this.PostBuildCommands.ToString(System.Environment.NewLine)
                );
                preBuildGroup.Serialize(document, itemDefnGroup);
            }
        }

        /// <summary>
        /// Convert a semi-colon separated number of paths to relative where possible.
        /// </summary>
        /// <param name="joinedPaths">Paths joined by semi-colons.</param>
        /// <returns>Relative paths joined by semi-colons.</returns>
        public string
        ToRelativePath(
            string joinedPaths)
        {
            var paths = joinedPaths.Split(new[] { ';' });
            var relative_paths = this.ToRelativePaths(paths);
            return System.String.Join(";", relative_paths);
        }

        /// <summary>
        /// Convert a path from a TokenizedString to relative paths.
        /// </summary>
        /// <param name="path">Path to convert.</param>
        /// <param name="isOutputDir">Optional boolean, true if this path represents the OutputDir. False by default.</param>
        /// <returns>Relative paths.</returns>
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

        /// <summary>
        /// Convert an array of TokenizedStrings to relative paths.
        /// </summary>
        /// <param name="paths">Array of strings.</param>
        /// <returns>Relative paths</returns>
        public string
        ToRelativePaths(
            Bam.Core.TokenizedStringArray paths)
        {
            return ToRelativePaths(paths.ToEnumerableWithoutDuplicates().Select(item => item.ToString()));
        }

        /// <summary>
        /// Convert an enumeration of paths to relative paths.
        /// </summary>
        /// <param name="paths">IEnumerable of string paths.</param>
        /// <param name="isOutputDir">Optional boolean, true if this path represents the OutputDir. False by default.</param>
        /// <returns>Relative paths</returns>
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
                    output_paths.Add($"{candidate.Key}{candidate.Value}");
                }
            }
            return output_paths.ToString(';');
        }
    }
}

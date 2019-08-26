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
    /// Class representing a .vcxproj on disk.
    /// </summary>
    public sealed class VSProject :
        HasGuid
    {
        /// <summary>
        /// Construct an instance, for the given module, at the given path, and added to the given solution.
        /// </summary>
        /// <param name="solution">VSSolution to add the project to</param>
        /// <param name="module">Module corresponding to the new project</param>
        /// <param name="projectPath">Path at which the project resides.</param>
        public VSProject(
            VSSolution solution,
            Bam.Core.Module module,
            Bam.Core.TokenizedString projectPath)
            :
            base(projectPath.ToString())
        {
            this.Solution = solution;
            this.Module = module;
            this.ProjectPath = projectPath.ToString();
            this.Configurations = new System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, VSProjectConfiguration>();
            this.ProjectSettings = new Bam.Core.Array<VSSettingsGroup>();
            this.Headers = new Bam.Core.Array<VSSettingsGroup>();
            this.Sources = new Bam.Core.Array<VSSettingsGroup>();
            this.Others = new Bam.Core.Array<VSSettingsGroup>();
            this.Resources = new Bam.Core.Array<VSSettingsGroup>();
            this.AssemblyFiles = new Bam.Core.Array<VSSettingsGroup>();
            this.Filter = new VSProjectFilter(this);
            this.OrderOnlyDependentProjects = new Bam.Core.Array<VSProject>();
            this.LinkDependentProjects = new Bam.Core.Array<VSProject>();
        }

        /// <summary>
        /// Get the module corresponding to this project
        /// </summary>
        public Bam.Core.Module Module { get; private set; }

        private static C.EBit
        GetModuleBitDepth(
            Bam.Core.Module module)
        {
            if (module is C.CModule)
            {
                return (module as C.CModule).BitDepth;
            }
            else
            {
                // best guess
                return (C.EBit)Bam.Core.CommandLineProcessor.Evaluate(new C.Options.DefaultBitDepth());
            }
        }

        /// <summary>
        /// Get the VSProjectConfiguration within this project corresponding to this Module
        /// </summary>
        /// <param name="module">Module to get configuration for</param>
        /// <returns>The VSProjectConfiguration</returns>
        public VSProjectConfiguration
        GetConfiguration(
            Bam.Core.Module module)
        {
            lock (this.Configurations)
            {
                var moduleConfig = module.BuildEnvironment.Configuration;
                if (this.Configurations.ContainsKey(moduleConfig))
                {
                    return this.Configurations[moduleConfig];
                }

                var platform = Bam.Core.EPlatform.Invalid;
                var bitDepth = GetModuleBitDepth(module);
                switch (bitDepth)
                {
                    case C.EBit.ThirtyTwo:
                        platform = Bam.Core.EPlatform.Win32;
                        break;

                    case C.EBit.SixtyFour:
                        platform = Bam.Core.EPlatform.Win64;
                        break;
                }
                if (Bam.Core.EPlatform.Invalid == platform)
                {
                    throw new Bam.Core.Exception($"Platform cannot be extracted from the tool {module.Tool.ToString()} for project {this.ProjectPath}");
                }
                var configuration = new VSProjectConfiguration(this, module, platform);
                this.Configurations.Add(moduleConfig, configuration);
                return configuration;
            }
        }

        /// <summary>
        /// Get the unique Settings group for the Module
        /// </summary>
        /// <param name="module">Module to get the group for</param>
        /// <param name="group">The type of the group queried</param>
        /// <param name="path">The path this settings group corresponds to. Default to null.</param>
        /// <returns></returns>
        public VSSettingsGroup
        GetUniqueSettingsGroup(
            Bam.Core.Module module,
            VSSettingsGroup.ESettingsGroup group,
            Bam.Core.TokenizedString path)
        {
            lock (this.ProjectSettings)
            {
                foreach (var settings in this.ProjectSettings)
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
                        // ignore checking the group, as files can mutate between them during the buildprocess
                        // (e.g. headers into custom builds)
                        // TODO: can this be a TokenizedString hash compare?
                        if (settings.Path.ToString().Equals(path.ToString(), System.StringComparison.Ordinal))
                        {
                            return settings;
                        }
                    }
                }

                var newGroup = new VSSettingsGroup(this, module, group, path);
                this.ProjectSettings.Add(newGroup);
                return newGroup;
            }
        }

        private void
        AddToFilter(
            VSSettingsGroup group)
        {
            lock (this.Filter)
            {
                this.Filter.AddFile(group);
            }
        }

        /// <summary>
        /// Add a header file to the project
        /// </summary>
        /// <param name="header">Settings group corresponding to the header</param>
        public void
        AddHeader(
            VSSettingsGroup header)
        {
            lock (this.Headers)
            {
                this.Headers.AddUnique(header);
            }
            AddToFilter(header);
        }

        /// <summary>
        /// Add a source file to the project
        /// </summary>
        /// <param name="source">Settings group correspnding to the source file</param>
        public void
        AddSource(
            VSSettingsGroup source)
        {
            lock (this.Sources)
            {
                this.Sources.AddUnique(source);
            }
        }

        /// <summary>
        /// Add an arbitrary file to the project
        /// </summary>
        /// <param name="other">Settings group corresponding to the file</param>
        public void
        AddOtherFile(
            VSSettingsGroup other)
        {
            lock (this.Others)
            {
                this.Others.AddUnique(other);
            }
            AddToFilter(other);
        }

        /// <summary>
        /// Add a resource file to the project
        /// </summary>
        /// <param name="other">Settings group corresponding to the resource file</param>
        public void
        AddResourceFile(
            VSSettingsGroup other)
        {
            lock (this.Resources)
            {
                this.Resources.AddUnique(other);
            }
            AddToFilter(other);
        }

        /// <summary>
        /// Add an assembly file to the project
        /// </summary>
        /// <param name="other">Settings group corresponding to the assembly file</param>
        public void
        AddAssemblyFile(
            VSSettingsGroup other)
        {
            lock (this.AssemblyFiles)
            {
                this.AssemblyFiles.AddUnique(other);
            }
            AddToFilter(other);
        }

        /// <summary>
        /// Add an order only dependency on another project
        /// </summary>
        /// <param name="dependentProject">Project that is an order only dependency</param>
        public void
        AddOrderOnlyDependency(
            VSProject dependentProject)
        {
            if (this.LinkDependentProjects.Contains(dependentProject))
            {
                Bam.Core.Log.DebugMessage($"Project {this.ProjectPath} already contains a link dependency on {dependentProject.ProjectPath}. There is no need to add it as an order-only dependency.");
                return;
            }
            lock (this.OrderOnlyDependentProjects)
            {
                this.OrderOnlyDependentProjects.AddUnique(dependentProject);
            }
        }

        /// <summary>
        /// Add a link-time dependency on another project
        /// </summary>
        /// <param name="dependentProject">Project that is a link time dependency</param>
        public void
        AddLinkDependency(
            VSProject dependentProject)
        {
            if (this.OrderOnlyDependentProjects.Contains(dependentProject))
            {
                Bam.Core.Log.DebugMessage($"Project {this.ProjectPath} already contains an order-only dependency on {dependentProject.ProjectPath}. Removing the order-only dependency, and upgrading to a link dependency.");
                this.OrderOnlyDependentProjects.Remove(dependentProject);
            }
            lock (this.LinkDependentProjects)
            {
                this.LinkDependentProjects.AddUnique(dependentProject);
            }
        }

        /// <summary>
        /// Get the project filepath
        /// </summary>
        public string ProjectPath { get; private set; }

        /// <summary>
        /// Get the filter applied to this project
        /// </summary>
        public VSProjectFilter Filter { get; private set; }

        private VSSolution Solution { get; set; }

        /// <summary>
        /// Get the dictionary of EConfiguration to VSProjectConfiguration
        /// </summary>
        public System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, VSProjectConfiguration> Configurations { get; private set; }

        private Bam.Core.Array<VSSettingsGroup> ProjectSettings { get; set; }

        private Bam.Core.Array<VSSettingsGroup> Headers { get; set; }

        private Bam.Core.Array<VSSettingsGroup> Sources { get; set; }

        private Bam.Core.Array<VSSettingsGroup> Others { get; set; }

        private Bam.Core.Array<VSSettingsGroup> Resources { get; set; }

        private Bam.Core.Array<VSSettingsGroup> AssemblyFiles { get; set; }

        private Bam.Core.Array<VSProject> OrderOnlyDependentProjects { get; set; }

        private Bam.Core.Array<VSProject> LinkDependentProjects { get; set; }

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
                    foreach (var config in this.Configurations)
                    {
                        if (!config.Value.ContainsOrderOnlyDependency(project))
                        {
                            continue;
                        }

                        var projectRefEl = document.CreateVSElement("ProjectReference", parentEl: itemGroupEl);
                        projectRefEl.SetAttribute(
                            "Include",
                            Bam.Core.RelativePathUtilities.GetRelativePathFromRoot(
                                System.IO.Path.GetDirectoryName(this.ProjectPath),
                                project.ProjectPath
                            )
                        );
                        projectRefEl.SetAttribute("Condition", config.Value.ConditionText);

                        document.CreateVSElement("Project", value: project.Guid.ToString("B"), parentEl: projectRefEl);
                        document.CreateVSElement("LinkLibraryDependencies", value: "false", parentEl: projectRefEl);
                    }
                }
            }
            if (this.LinkDependentProjects.Count > 0)
            {
                var itemGroupEl = document.CreateVSItemGroup(parentEl: parentEl);
                foreach (var project in this.LinkDependentProjects)
                {
                    foreach (var config in this.Configurations)
                    {
                        if (!config.Value.ContainsLinkDependency(project))
                        {
                            continue;
                        }

                        var projectRefEl = document.CreateVSElement("ProjectReference", parentEl: itemGroupEl);
                        projectRefEl.SetAttribute(
                            "Include",
                            Bam.Core.RelativePathUtilities.GetRelativePathFromRoot(
                                System.IO.Path.GetDirectoryName(this.ProjectPath),
                                project.ProjectPath
                            )
                        );
                        projectRefEl.SetAttribute("Condition", config.Value.ConditionText);

                        document.CreateVSElement("Project", value: project.Guid.ToString("B"), parentEl: projectRefEl);
                        document.CreateVSElement("LinkLibraryDependencies", value: "true", parentEl: projectRefEl);
                    }
                }
            }
        }

        /// <summary>
        /// Serialize user settings for the project to XML.
        /// </summary>
        /// <returns>The XML document containing the user settings.</returns>
        public System.Xml.XmlDocument
        SerializeUserSettings()
        {
            var document = new System.Xml.XmlDocument();

            var projectEl = this.CreateRootProject(document);
            var visualCMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            projectEl.SetAttribute("ToolsVersion", visualCMeta.VCXProjToolsVersion);

            // the working directory appears to need to be the same in all configurations
            var el = document.CreateVSPropertyGroup(parentEl: projectEl);
            var debuggerFlavour = document.CreateVSElement("DebuggerFlavor");
            debuggerFlavour.InnerText = "WindowsLocalDebugger";
            var workingDir = document.CreateVSElement("LocalDebuggerWorkingDirectory");
            (this.Module as C.ConsoleApplication).WorkingDirectory.Parse();
            workingDir.InnerText = (this.Module as C.ConsoleApplication).WorkingDirectory.ToString();
            el.AppendChild(debuggerFlavour);
            el.AppendChild(workingDir);

            return document;
        }

        /// <summary>
        /// Serialize the project to XML
        /// </summary>
        /// <returns>XML document containing the project.</returns>
        public System.Xml.XmlDocument
        Serialize()
        {
            var document = new System.Xml.XmlDocument();

            var projectEl = this.CreateRootProject(document);
            projectEl.SetAttribute("DefaultTargets", "Build");

            var visualCMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            projectEl.SetAttribute("ToolsVersion", visualCMeta.VCXProjToolsVersion);

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
            var vcEnv = visualCMeta.Environment(GetModuleBitDepth(this.Module));
            if (vcEnv.ContainsKey("WindowsSDKVersion"))
            {
                var windowssdk_version = vcEnv["WindowsSDKVersion"].First().ToString().TrimEnd(System.IO.Path.DirectorySeparatorChar);
                document.CreateVSElement("WindowsTargetPlatformVersion", value: windowssdk_version, parentEl: globalPropertyGroup);
            }
            else
            {
                // appears to automatically fall back to 8.1
            }

            document.CreateVSImport(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props", parentEl: projectEl);

            // configuration properties
            foreach (var config in this.Configurations)
            {
                config.Value.SerializeProperties(document, projectEl);
            }

            document.CreateVSImport(@"$(VCTargetsPath)\Microsoft.Cpp.props", parentEl: projectEl);
            if (this.AssemblyFiles.Any())
            {
                var extensionSettings = document.CreateVSImportGroup("ExtensionSettings", parentEl: projectEl);
                document.CreateVSImport(@"$(VCTargetsPath)\BuildCustomizations\masm.props", parentEl: extensionSettings);
            }

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
                            group.AddSetting(
                                "ExcludedFromBuild",
                                "true",
                                config.Value.ConditionText
                            );
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
                    foreach (var config in this.Configurations)
                    {
                        if (!config.Value.ContainsHeader(group))
                        {
                            group.AddSetting(
                                "ExcludedFromBuild",
                                "true",
                                config.Value.ConditionText
                            );
                        }
                    }
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
            if (this.Resources.Count > 0)
            {
                var resourceGroup = document.CreateVSItemGroup(parentEl: projectEl);
                foreach (var group in this.Resources)
                {
                    foreach (var config in this.Configurations)
                    {
                        if (!config.Value.ContainsResourceFile(group))
                        {
                            group.AddSetting(
                                "ExcludedFromBuild",
                                "true",
                                config.Value.ConditionText
                            );
                        }
                    }
                    group.Serialize(document, resourceGroup);
                }
            }
            if (this.AssemblyFiles.Count > 0)
            {
                var assemblerGroup = document.CreateVSItemGroup(parentEl: projectEl);
                foreach (var group in this.AssemblyFiles)
                {
                    foreach (var config in this.Configurations)
                    {
                        if (!config.Value.ContainsAssemblyFile(group))
                        {
                            group.AddSetting(
                                "ExcludedFromBuild",
                                "true",
                                config.Value.ConditionText
                            );
                        }
                    }
                    group.Serialize(document, assemblerGroup);
                }
            }

            // dependent projects
            this.SerializeDependentProjects(document, projectEl);

            document.CreateVSImport(@"$(VCTargetsPath)\Microsoft.Cpp.targets", parentEl: projectEl);
            if (this.AssemblyFiles.Any())
            {
                var extensionTargets = document.CreateVSImportGroup("ExtensionTargets", parentEl: projectEl);
                document.CreateVSImport(@"$(VCTargetsPath)\BuildCustomizations\masm.targets", parentEl: extensionTargets);
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

        /// <summary>
        /// Query whether the .vcxproj corresponding to the Module is buildable.
        /// </summary>
        /// <param name="module">Module to query.</param>
        /// <returns>true if the project is buildable</returns>
        public static bool
        IsBuildable(
            Bam.Core.Module module)
        {
            var project = module.MetaData as VSProject;
            if (null == project)
            {
                return false;
            }
            var configuration = project.GetConfiguration(module);
            switch (configuration.Type)
            {
                case VSProjectConfiguration.EType.Application:
                case VSProjectConfiguration.EType.DynamicLibrary:
                case VSProjectConfiguration.EType.StaticLibrary:
                    return true;

                case VSProjectConfiguration.EType.Utility:
                    {
                        // there are some utility projects just for copying files around (and no source files), which do need to build
                        // so query whether the original module had the [C.Prebuilt] attribute
                        var isPrebuilt = (module is C.CModule) && (module as C.CModule).IsPrebuilt;
                        return !isPrebuilt;
                    }

                default:
                    throw new Bam.Core.Exception($"Unrecognized project type, {configuration.Type}");
            }
        }
    }
}

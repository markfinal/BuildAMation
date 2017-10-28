#region License
// Copyright (c) 2010-2017, Mark Final
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
    public sealed class VSProject :
        HasGuid
    {
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

        public Bam.Core.Module Module
        {
            get;
            private set;
        }

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
                    throw new Bam.Core.Exception("Platform cannot be extracted from the tool {0} for project {1}", module.Tool.ToString(), this.ProjectPath);
                }
                var configuration = new VSProjectConfiguration(this, module, platform);
                this.Configurations.Add(moduleConfig, configuration);
                return configuration;
            }
        }

        public VSSettingsGroup
        GetUniqueSettingsGroup(
            Bam.Core.Module module,
            VSSettingsGroup.ESettingsGroup group,
            Bam.Core.TokenizedString include = null)
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
                        // TODO: can this be a TokenizedString hash compare?
                        if (settings.Include.ToString() == include.ToString())
                        {
                            return settings;
                        }
                    }
                }

                var newGroup = new VSSettingsGroup(this, module, group, include);
                this.ProjectSettings.Add(newGroup);
                return newGroup;
            }
        }

        public void
        AddHeader(
            VSSettingsGroup header)
        {
            lock (this)
            {
                this.Headers.AddUnique(header);
                this.Filter.AddFile(header);
            }
        }

        public void
        AddSource(
            VSSettingsGroup source)
        {
            lock (this)
            {
                this.Sources.AddUnique(source);
                this.Filter.AddFile(source);
            }
        }

        public void
        AddOtherFile(
            VSSettingsGroup other)
        {
            lock (this)
            {
                this.Others.AddUnique(other);
                this.Filter.AddFile(other);
            }
        }

        public void
        AddResourceFile(
            VSSettingsGroup other)
        {
            lock (this)
            {
                this.Resources.AddUnique(other);
                this.Filter.AddFile(other);
            }
        }

        public void
        AddAssemblyFile(
            VSSettingsGroup other)
        {
            lock (this)
            {
                this.AssemblyFiles.AddUnique(other);
                this.Filter.AddFile(other);
            }
        }

        public void
        AddOrderOnlyDependency(
            VSProject dependentProject)
        {
            lock (this)
            {
                if (this.LinkDependentProjects.Contains(dependentProject))
                {
                    Bam.Core.Log.DebugMessage("Project {0} already contains a link dependency on {1}. There is no need to add it as an order-only dependency.", this.ProjectPath, dependentProject.ProjectPath);
                    return;
                }
                this.OrderOnlyDependentProjects.AddUnique(dependentProject);
            }
        }

        public void
        AddLinkDependency(
            VSProject dependentProject)
        {
            lock (this)
            {
                if (this.OrderOnlyDependentProjects.Contains(dependentProject))
                {
                    Bam.Core.Log.DebugMessage("Project {0} already contains an order-only dependency on {1}. Removing the order-only dependency, and upgrading to a link dependency.", this.ProjectPath, dependentProject.ProjectPath);
                    this.OrderOnlyDependentProjects.Remove(dependentProject);
                }
                this.LinkDependentProjects.AddUnique(dependentProject);
            }
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

        private Bam.Core.Array<VSSettingsGroup> Resources
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
                        projectRefEl.SetAttribute("Include", Bam.Core.RelativePathUtilities.GetPath(project.ProjectPath, this.ProjectPath));
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
                        projectRefEl.SetAttribute("Include", Bam.Core.RelativePathUtilities.GetPath(project.ProjectPath, this.ProjectPath));
                        projectRefEl.SetAttribute("Condition", config.Value.ConditionText);

                        document.CreateVSElement("Project", value: project.Guid.ToString("B"), parentEl: projectRefEl);
                        document.CreateVSElement("LinkLibraryDependencies", value: "true", parentEl: projectRefEl);
                    }
                }
            }
        }

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
                    foreach (var config in this.Configurations)
                    {
                        if (!config.Value.ContainsHeader(group))
                        {
                            group.AddSetting("ExcludedFromBuild", "true", config.Value.ConditionText);
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
                            group.AddSetting("ExcludedFromBuild", "true", config.Value.ConditionText);
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
                            group.AddSetting("ExcludedFromBuild", "true", config.Value.ConditionText);
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
                    throw new Bam.Core.Exception("Unrecognized project type, {0}", configuration.Type);
            }
        }
    }
}

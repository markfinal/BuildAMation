#region License
// Copyright (c) 2010-2016, Mark Final
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
            Bam.Core.Module module)
            :
            base(module.CreateTokenizedString("$(packagebuilddir)/$(modulename).vcxproj").Parse())
        {
            this.Solution = solution;
            this.ProjectPath = module.CreateTokenizedString("$(packagebuilddir)/$(modulename).vcxproj").Parse();
            this.Configurations = new System.Collections.Generic.Dictionary<Bam.Core.EConfiguration, VSProjectConfiguration>();
            this.ProjectSettings = new Bam.Core.Array<VSSettingsGroup>();
            this.Headers = new Bam.Core.Array<VSSettingsGroup>();
            this.Sources = new Bam.Core.Array<VSSettingsGroup>();
            this.Others = new Bam.Core.Array<VSSettingsGroup>();
            this.Resources = new Bam.Core.Array<VSSettingsGroup>();
            this.Filter = new VSProjectFilter();
            this.OrderOnlyDependentProjects = new Bam.Core.Array<VSProject>();
            this.LinkDependentProjects = new Bam.Core.Array<VSProject>();
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
                        if (settings.Include.Parse() == include.Parse())
                        {
                            return settings;
                        }
                    }
                }

                var newGroup = new VSSettingsGroup(module, group, include);
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
        RequiresProject(
            VSProject dependentProject)
        {
            lock (this)
            {
                if (this.LinkDependentProjects.Contains(dependentProject))
                {
                    throw new Bam.Core.Exception("Project already exists as a link dependency");
                }
                this.OrderOnlyDependentProjects.AddUnique(dependentProject);
            }
        }

        public void
        LinkAgainstProject(
            VSProject dependentProject)
        {
            lock (this)
            {
                if (this.OrderOnlyDependentProjects.Contains(dependentProject))
                {
                    throw new Bam.Core.Exception("Project already exists as an order only dependency");
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
            if (this.Resources.Count > 0)
            {
                var resourceGroup = document.CreateVSItemGroup(parentEl: projectEl);
                foreach (var group in this.Resources)
                {
                    group.Serialize(document, resourceGroup);
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
}

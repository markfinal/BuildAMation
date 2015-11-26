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
        SetCharacterSet(
            C.ECharacterSet charSet)
        {
            this.CharacterSet = charSet;
        }

        public void
        SetOutputPath(
            Bam.Core.TokenizedString path)
        {
            var macros = new Bam.Core.MacroList();
            // TODO: ideally, $(ProjectDir) should replace the following directory separator as well,
            // but it does not seem to be a show stopper if it doesn't
            macros.Add("packagebuilddir", Bam.Core.TokenizedString.CreateVerbatim("$(ProjectDir)"));
            macros.Add("modulename", Bam.Core.TokenizedString.CreateVerbatim("$(ProjectName)"));
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
                        if ((null != include) && (settings.Include.Parse() == include.Parse()))
                        {
                            return settings;
                        }
                    }
                }

                var newGroup = uniqueToProject ? this.Project.GetUniqueSettingsGroup(this.Module, group, include) : new VSSettingsGroup(this.Module, group, include);
                this.SettingGroups.Add(newGroup);
                return newGroup;
            }
        }

        public void
        AddHeaderFile(
            C.HeaderFile header)
        {
            var headerGroup = this.Project.GetUniqueSettingsGroup(this.Module, VSSettingsGroup.ESettingsGroup.Header, header.InputPath);
            this.Project.AddHeader(headerGroup);
        }

        public void
        AddSourceFile(
            Bam.Core.Module module,
            Bam.Core.Settings patchSettings)
        {
            var settings = module.MetaData as VSSettingsGroup;
            if (null != patchSettings)
            {
                (patchSettings as VisualStudioProcessor.IConvertToProject).Convert(module, settings, condition: this.ConditionText);
            }
            this.Sources.AddUnique(settings);
            this.Project.AddSource(settings);
        }

        public void
        AddOtherFile(
            Bam.Core.IInputPath other)
        {
            var otherGroup = this.Project.GetUniqueSettingsGroup(this.Module, VSSettingsGroup.ESettingsGroup.CustomBuild, other.InputPath);
            this.Project.AddOtherFile(otherGroup);
        }

        public void
        AddResourceFile(
            C.WinResource resource,
            Bam.Core.Settings patchSettings)
        {
            var settings = resource.MetaData as VSSettingsGroup;
            if (null != patchSettings)
            {
                (patchSettings as VisualStudioProcessor.IConvertToProject).Convert(resource, settings, condition: this.ConditionText);
            }
            var resourceGroup = this.Project.GetUniqueSettingsGroup(this.Module, VSSettingsGroup.ESettingsGroup.Resource, resource.InputPath);
            this.Project.AddResourceFile(resourceGroup);
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
                var preBuildGroup = new VSSettingsGroup(this.Module, VSSettingsGroup.ESettingsGroup.PreBuild);
                preBuildGroup.AddSetting("Command", this.PreBuildCommands.ToString(System.Environment.NewLine));
                preBuildGroup.Serialize(document, itemDefnGroup);
            }
            if (this.PostBuildCommands.Count > 0)
            {
                var preBuildGroup = new VSSettingsGroup(this.Module, VSSettingsGroup.ESettingsGroup.PostBuild);
                preBuildGroup.AddSetting("Command", this.PostBuildCommands.ToString(System.Environment.NewLine));
                preBuildGroup.Serialize(document, itemDefnGroup);
            }
        }
    }
}

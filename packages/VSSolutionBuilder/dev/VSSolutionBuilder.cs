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
using System.Linq;
[assembly: Bam.Core.DeclareBuilder("VSSolution", typeof(VSSolutionBuilder.VSSolutionBuilder))]

namespace VSSolutionBuilder
{
namespace V2
{
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

    public sealed class VSSolution :
        System.Collections.Generic.IEnumerable<VSProject>
    {
        public VSSolution()
        {
            this.Projects = new System.Collections.Generic.Dictionary<System.Type, VSProject>();
        }

        private System.Collections.Generic.Dictionary<System.Type, VSProject> Projects
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
            Application
        }

        private static readonly string VCProjNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private ItemGroup ProjectConfigurations;
        private PropertyGroup Globals;
        private Import DefaultImport;
        private Import LanguageImport;
        private System.Collections.Generic.List<ItemDefinitionGroup> ConfigurationDefs = new System.Collections.Generic.List<ItemDefinitionGroup>();
        private ItemGroup SourceGroup;
        private ItemGroup HeaderGroup;
        private ItemGroup CustomBuildGroup;
        private ItemGroup ProjectDependenciesGroup;
        private Import LanguageTargets;
        private Type ProjectType;
        private System.Xml.XmlElement ConfigToolsPropertiesElement = null;
        private System.Xml.XmlElement CommonCompilationOptionsElement = null;
        private System.Xml.XmlElement PostBuildCommandElement = null;
        private System.Xml.XmlElement AnonymousPropertySettingsElement = null;
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

            // ProjectConfigurations element
            this.ProjectConfigurations = this.CreateItemGroup("ProjectConfigurations");
            this.Project.AppendChild(this.ProjectConfigurations.Element);

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

            // Sources
            this.SourceGroup = this.CreateItemGroup(null);
            this.Project.AppendChild(this.SourceGroup.Element);

            // Project Dependencies
            this.ProjectDependenciesGroup = this.CreateItemGroup(null);
            this.Project.AppendChild(this.ProjectDependenciesGroup.Element);

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
                this.MakeNodeConditional(commandEl, config.FullName);
                commandEl.InnerText = custom.Command;
                element.AppendChild(commandEl);

                var messageEl = this.CreateProjectElement("Message");
                this.MakeNodeConditional(messageEl, config.FullName);
                messageEl.InnerText = custom.Message;
                element.AppendChild(messageEl);

                var outputsEl = this.CreateProjectElement("Outputs");
                this.MakeNodeConditional(outputsEl, config.FullName);
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
                this.Project.InsertAfter(this.HeaderGroup.Element, this.SourceGroup.Element);
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
                        (patchSettings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, el, configuration.FullName);
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
                (patchSettings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, element, configuration.FullName);
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
                this.ProjectConfigurations.Element.AppendChild(projconfig);
            }

            var configExpression = System.String.Format(@"'$(Configuration)|$(Platform)'=='{0}'", configuration.FullName);

            // project properties
            {
                var configProps = this.CreatePropertyGroup("Configuration");
                configProps.Element.Attributes.Append(this.CreateAttribute("Condition")).Value = configExpression;
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

                    default:
                        throw new Bam.Core.Exception("Unknown project type, {0}", this.ProjectType.ToString());
                }
                configProps.Element.AppendChild(configType);
                var platformToolset = this.CreateProjectElement("PlatformToolset", "v120"); // TODO: dependent upon the version of VisualC
                configProps.Element.AppendChild(platformToolset);
                this.Project.InsertAfter(configProps.Element, this.DefaultImport.Element);
            }

            // project definitions
            {
                var configGroup = this.CreateItemDefinitionGroup(configExpression);
                var clCompile = this.CreateProjectElement("ClCompile");
                configGroup.Element.AppendChild(clCompile);
                this.CommonCompilationOptionsElement = clCompile;
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

                    default:
                        throw new Bam.Core.Exception("Unknown project type, {0}", this.ProjectType.ToString());
                }
                this.Project.InsertAfter(configGroup.Element, this.LanguageImport.Element);
                this.ConfigToolsPropertiesElement = configGroup.Element;
            }

            // anonymous project settings
            {
                var configProps = this.CreatePropertyGroup(null);
                this.AnonymousPropertySettingsElement = configProps.Element;
                configProps.Element.Attributes.Append(this.CreateAttribute("Condition")).Value = configExpression;

                var outDirEl = this.CreateProjectElement("OutDir");
                var macros = new Bam.Core.V2.MacroList();
                // TODO: ideally, $(ProjectDir) should replace the following directory separator as well,
                // but it does not seem to be a show stopper if it doesn't
                macros.Add("pkgbuilddir", Bam.Core.V2.TokenizedString.Create("$(ProjectDir)", null, verbatim:true));
                macros.Add("modulename", Bam.Core.V2.TokenizedString.Create("$(ProjectName)", null, verbatim: true));
                var outDir = outPath.Parse(macros);
                outDir = System.IO.Path.GetDirectoryName(outDir);
                outDir += "\\";
                outDirEl.InnerText = outDir;
                configProps.Element.AppendChild(outDirEl);

                this.Project.InsertAfter(configProps.Element, this.LanguageImport.Element);
            }
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

        public void SetCommonCompilationOptions(Bam.Core.V2.Module module, Bam.Core.V2.Settings settings)
        {
            (settings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, this.CommonCompilationOptionsElement, null);

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
            this.AnonymousPropertySettingsElement.AppendChild(intDirEl);
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands)
        {
            if (null == this.PostBuildCommandElement)
            {
                var tool = this.CreateProjectElement("PostBuildEvent");
                this.ConfigToolsPropertiesElement.AppendChild(tool);

                var commandElement = this.CreateProjectElement("Command");
                tool.AppendChild(commandElement);

                this.PostBuildCommandElement = commandElement;
            }

            var commandText = new System.Text.StringBuilder();
            foreach (var command in commands)
            {
                commandText.AppendFormat("{0}{1}", command, System.Environment.NewLine);
            }
            this.PostBuildCommandElement.InnerText += commandText.ToString();
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

        private ItemGroup CreateItemDefinitionGroup(string condition)
        {
            var group = this.CreateProjectElement("ItemDefinitionGroup");
            group.Attributes.Append(this.CreateAttribute("Condition")).Value = condition;
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
            string conditionalConfiguration)
        {
            node.Attributes.Append(this.CreateAttribute("Condition")).Value = System.String.Format("'$(Configuration)|$(Platform)'=='{0}'", conditionalConfiguration);
        }

        public void AddToolSetting<T>(
            System.Xml.XmlElement container,
            string settingName,
            T settingValue,
            string conditionalConfiguration,
            System.Action<T, string, System.Text.StringBuilder> process)
        {
            var settingElement = container.AppendChild(this.CreateProjectElement(settingName, process, settingValue));
            if (null != conditionalConfiguration)
            {
                this.MakeNodeConditional(settingElement, conditionalConfiguration);
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
                        this.AddToolSetting(element, "ExcludedFromBuild", excludedSource, config.FullName,
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

        public string Config
        {
            get;
            private set;
        }

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

        public void SetCommonCompilationOptions(Bam.Core.V2.Module module, Bam.Core.V2.Settings settings)
        {
            this.Project.SetCommonCompilationOptions(module, settings);
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands)
        {
            this.Project.AddPostBuildCommands(commands);
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

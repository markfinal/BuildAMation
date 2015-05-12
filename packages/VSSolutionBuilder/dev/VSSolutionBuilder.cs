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
        System.Xml.XmlDocument
    {
        public VSSolution()
        {
            this.Projects = new System.Collections.Generic.Dictionary<System.Type, VSProject>();
        }

        public System.Collections.Generic.Dictionary<System.Type, VSProject> Projects
        {
            get;
            private set;
        }
    }

    public sealed class VSProject :
        System.Xml.XmlDocument
    {
        private static readonly string VCProjNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private ItemGroup ProjectConfiguations;
        private PropertyGroup Globals;
        private Import DefaultImport;
        private System.Collections.Generic.List<PropertyGroup> Configurations = new System.Collections.Generic.List<PropertyGroup>();
        private Import LanguageImport;
        private System.Collections.Generic.List<ItemDefinitionGroup> ConfigurationDefs = new System.Collections.Generic.List<ItemDefinitionGroup>();
        private ItemGroup SourceGroup;
        private Import LanguageTargets;
        private System.Guid GUID = System.Guid.NewGuid();
        private VSSolutionMeta.Type Type;

        public VSProject(VSSolutionMeta.Type type)
        {
            this.Type = type;

            // Project (root) element
            this.AppendChild(this.CreateProjectElement("Project"));
            this.Project.Attributes.Append(this.CreateAttribute("DefaultTargets")).Value = "Build";
            this.Project.Attributes.Append(this.CreateAttribute("ToolsVersion")).Value = "12.0"; // TODO: in tune with VisualC package version

            // ProjectConfigurations element
            this.ProjectConfiguations = this.CreateItemGroup("ProjectConfigurations");
            this.Project.AppendChild(this.ProjectConfiguations.Element);

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

            // Language targets
            this.LanguageTargets = this.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.targets");
            this.Project.AppendChild(this.LanguageTargets.Element);
        }

        public void AddSourceFile(string path)
        {
            // check whether this source file has been added before
#if true
            foreach (var el in this.SourceGroup)
            {
                if (!el.HasAttribute("Include"))
                {
                    continue;
                }
                var include = el.Attributes["Include"];
                if (include.Value == path)
                {
                    Bam.Core.Log.DebugMessage("Source path '{0}' already added", path);
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
            element.Attributes.Append(this.CreateAttribute("Include")).Value = path;
            this.SourceGroup.Element.AppendChild(element);
        }

        public void AddProjectConfiguration(string configuration, string platform, Bam.Core.V2.Module module)
        {
            var combined = System.String.Format("{0}|{1}", configuration, platform);

            // overall project configurations
            {
                var projconfig = this.CreateProjectElement("ProjectConfiguration");
                projconfig.Attributes.Append(this.CreateAttribute("Include")).Value = combined;
                var config = this.CreateProjectElement("Configuration", configuration);
                var plat = this.CreateProjectElement("Platform", platform);
                projconfig.AppendChild(config);
                projconfig.AppendChild(plat);
                this.ProjectConfiguations.Element.AppendChild(projconfig);
            }

            var configName = System.String.Format(@"'$(Configuration)|$(Platform)'=='{0}'", combined);

            // project properties
            {
                var configProps = this.CreatePropertyGroup("Configuration");
                configProps.Element.Attributes.Append(this.CreateAttribute("Condition")).Value = configName;
                // TODO: can this be better done with a lambda to get the inner text?
                var configType = this.CreateProjectElement("ConfigurationType");
                switch (this.Type)
                {
                    case VSSolutionMeta.Type.NA:
                        throw new Bam.Core.Exception("Invalid project type");

                    case VSSolutionMeta.Type.StaticLibrary:
                        configType.InnerText = "StaticLibrary";
                        break;

                    case VSSolutionMeta.Type.Application:
                        configType.InnerText = "Application";
                        break;
                }
                configProps.Element.AppendChild(configType);
                var platformToolset = this.CreateProjectElement("PlatformToolset", "v120"); // TODO: dependent upon the version of VisualC
                configProps.Element.AppendChild(platformToolset);
                this.Project.InsertAfter(configProps.Element, this.DefaultImport.Element);
            }

            // project definitions
            {
                var configGroup = this.CreateItemDefinitionGroup(configName);
                var clCompile = this.CreateProjectElement("ClCompile");
                configGroup.Element.AppendChild(clCompile);
                switch (this.Type)
                {
                    case VSSolutionMeta.Type.NA:
                        throw new Bam.Core.Exception("Invalid project type");

                    case VSSolutionMeta.Type.StaticLibrary:
                        {
                            var tool = this.CreateProjectElement("Lib");
                            configGroup.Element.AppendChild(tool);

                            // TODO: convert settings
                            (module.Settings as VisualStudioProcessor.V2.IConvertToProject).Convert(module, tool);
                        }
                        break;

                    case VSSolutionMeta.Type.Application:
                        {
                            var tool = this.CreateProjectElement("Link");
                            configGroup.Element.AppendChild(tool);

                            // TODO: convert settings
                        }
                        break;
                }
                this.Project.InsertAfter(configGroup.Element, this.LanguageImport.Element);
            }
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
            group.Attributes.Append(this.CreateAttribute("Label")).Value = label;
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
    }

    public abstract class VSSolutionMeta
    {
        protected VSProject Project = null;

        public enum Type
        {
            NA,
            StaticLibrary,
            Application
        }

        public VSSolutionMeta(Bam.Core.V2.Module module, Type type)
        {
            var graph = Bam.Core.V2.Graph.Instance;
            var isReferenced = graph.IsReferencedModule(module);
            this.IsProjectModule = isReferenced;
            if (isReferenced)
            {
                var solution = graph.MetaData as VSSolution;
                if (solution.Projects.ContainsKey(module.GetType()))
                {
                    this.Project = solution.Projects[module.GetType()];
                }
                else
                {
                    this.Project = new VSProject(type);
                    solution.Projects[module.GetType()] = this.Project;
                }

                // TODO: platform isn't the Environment platform, but the tools in use
                var platform = "Win32";
                this.Project.AddProjectConfiguration(module.BuildEnvironment.Configuration.ToString(), platform, module);

                var projectPath = Bam.Core.V2.TokenizedString.Create("$(buildroot)/$(modulename).vcxproj", module);
                projectPath.Parse();
                this.Project.ProjectPath = projectPath.ToString();
            }
            module.MetaData = this;
        }

        public bool IsProjectModule
        {
            get;
            private set;
        }

        public Bam.Core.V2.Module ProjectModule
        {
            get;
            set;
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
            foreach (var project in solution.Projects)
            {
                var builder = new System.Text.StringBuilder();
                using (var xmlwriter = System.Xml.XmlWriter.Create(builder, settings))
                {
                    project.Value.WriteTo(xmlwriter);
                }
                Bam.Core.Log.DebugMessage(builder.ToString());

                using (var xmlwriter = System.Xml.XmlWriter.Create(project.Value.ProjectPath, settings))
                {
                    project.Value.WriteTo(xmlwriter);
                }
            }
            // TODO: write out solution
        }
    }

    // TODO: add XML element
    public sealed class VSProjectObjectFile :
        VSSolutionMeta
    {
        public VSProjectObjectFile(Bam.Core.V2.Module module)
            : base(module, Type.NA)
        { }

        public string Source
        {
            get;
            set;
        }

        public string Output
        {
            get;
            set;
        }
    }

    // TODO: add XML document
    public sealed class VSProjectStaticLibrary :
        VSSolutionMeta
    {
        public VSProjectStaticLibrary(Bam.Core.V2.Module module) :
            base(module, Type.StaticLibrary)
        {
            this.ObjectFiles = new System.Collections.Generic.List<VSProjectObjectFile>();
        }

        public void AddObjectFile(VSProjectObjectFile objectFile)
        {
            this.Project.AddSourceFile(objectFile.Source);
        }

        private System.Collections.Generic.List<VSProjectObjectFile> ObjectFiles
        {
            get;
            set;
        }
    }

    // TODO: add XML document
    public sealed class VSProjectProgram :
        VSSolutionMeta
    {
        public VSProjectProgram(Bam.Core.V2.Module module) :
            base(module, Type.Application)
        {
            this.ObjectFiles = new System.Collections.Generic.List<VSProjectObjectFile>();
            this.Libraries = new System.Collections.Generic.List<VSProjectStaticLibrary>();
        }

        public void AddObjectFile(VSProjectObjectFile objectFile)
        {
            this.Project.AddSourceFile(objectFile.Source);
        }

        private System.Collections.Generic.List<VSProjectObjectFile> ObjectFiles
        {
            get;
            set;
        }

        public System.Collections.Generic.List<VSProjectStaticLibrary> Libraries
        {
            get;
            private set;
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

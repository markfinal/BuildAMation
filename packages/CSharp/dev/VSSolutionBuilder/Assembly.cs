// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public partial class VSSolutionBuilder
    {
        public object Build(CSharp.Assembly moduleToBuild, out System.Boolean success)
        {
            Opus.Core.BaseModule assemblyModule = moduleToBuild as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = assemblyModule.OwningNode;
            Opus.Core.Target target = node.Target;
            CSharp.OptionCollection options = assemblyModule.Options as CSharp.OptionCollection;

            string moduleName = node.ModuleName;

            string platformName;
            switch ((options as CSharp.IOptions).Platform)
            {
                case CSharp.EPlatform.AnyCpu:
                    platformName = "AnyCPU";
                    break;

                case CSharp.EPlatform.X86:
                    platformName = "x86";
                    break;

                case CSharp.EPlatform.X64:
                case CSharp.EPlatform.Itanium:
                    platformName = "x64";
                    break;

                default:
                    throw new Opus.Core.Exception("Unrecognized platform");
            }

            ICSProject projectData = null;
            // TODO: want to remove this
            lock (this.solutionFile.ProjectDictionary)
            {
                if (this.solutionFile.ProjectDictionary.ContainsKey(moduleName))
                {
                    projectData = this.solutionFile.ProjectDictionary[moduleName] as ICSProject;
                }
                else
                {
                    System.Type solutionType = Opus.Core.State.Get("VSSolutionBuilder", "SolutionType") as System.Type;
                    object SolutionInstance = System.Activator.CreateInstance(solutionType);
                    System.Reflection.PropertyInfo ProjectExtensionProperty = solutionType.GetProperty("ProjectExtension");
                    string projectExtension = ProjectExtensionProperty.GetGetMethod().Invoke(SolutionInstance, null) as string;

                    string projectPathName = System.IO.Path.Combine(node.GetModuleBuildDirectory(), moduleName);
                    projectPathName += projectExtension;

                    System.Type projectType = VSSolutionBuilder.GetProjectClassType();
                    projectData = System.Activator.CreateInstance(projectType, new object[] { moduleName, projectPathName, node.Package.Identifier, assemblyModule.ProxyPath }) as ICSProject;

                    this.solutionFile.ProjectDictionary.Add(moduleName, projectData);
                }
            }

            {
                if (!projectData.Platforms.Contains(platformName))
                {
                    projectData.Platforms.Add(platformName);
                }
            }

            // solution folder
            {
                var groups = moduleToBuild.GetType().GetCustomAttributes(typeof(Opus.Core.ModuleGroupAttribute), true);
                if (groups.Length > 0)
                {
                    projectData.GroupName = (groups as Opus.Core.ModuleGroupAttribute[])[0].GroupName;
                }
            }

            if (node.ExternalDependents != null)
            {
                foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    if (dependentNode.ModuleName != moduleName)
                    {
                        // TODO: want to remove this
                        lock (this.solutionFile.ProjectDictionary)
                        {
                            if (this.solutionFile.ProjectDictionary.ContainsKey(dependentNode.ModuleName))
                            {
                                IProject dependentProject = this.solutionFile.ProjectDictionary[dependentNode.ModuleName];
                                projectData.DependentProjects.Add(dependentProject);
                            }
                        }
                    }
                }
            }

            // references
            foreach (Opus.Core.Location location in (options as CSharp.IOptions).References)
            {
                var reference = location.GetSinglePath();
                projectData.References.Add(reference);
            }

            string configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target, platformName);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
                    // TODO: fix me?
                    configuration = new ProjectConfiguration(configurationName, projectData);
                    configuration.CharacterSet = EProjectCharacterSet.NotSet;
                    projectData.Configurations.Add(target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                    projectData.Configurations.AddExistingForTarget(target, configuration);
                }
            }

            var fields = moduleToBuild.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // C# files
                {
                    var sourceFileAttributes = field.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                    if (null != sourceFileAttributes && sourceFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Opus.Core.Location)
                        {
                            var file = sourceField as Opus.Core.Location;
                            string absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Source file '{0}' does not exist", absolutePath);
                            }

                            ProjectFile sourceFile;
                            lock (projectData.SourceFiles)
                            {
                                if (!projectData.SourceFiles.Contains(absolutePath))
                                {
                                    sourceFile = new ProjectFile(absolutePath);
                                    sourceFile.FileConfigurations = new ProjectFileConfigurationCollection();
                                    projectData.SourceFiles.Add(sourceFile);
                                }
                                else
                                {
                                    sourceFile = projectData.SourceFiles[absolutePath];
                                }
                            }
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            Opus.Core.FileCollection sourceCollection = sourceField as Opus.Core.FileCollection;
                            foreach (Opus.Core.Location location in sourceCollection)
                            {
                                var absolutePath = location.GetSinglePath();
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception("Source file '{0}' does not exist", absolutePath);
                                }

                                ProjectFile sourceFile;
                                lock (projectData.SourceFiles)
                                {
                                    if (!projectData.SourceFiles.Contains(absolutePath))
                                    {
                                        sourceFile = new ProjectFile(absolutePath);
                                        sourceFile.FileConfigurations = new ProjectFileConfigurationCollection();
                                        projectData.SourceFiles.Add(sourceFile);
                                    }
                                    else
                                    {
                                        sourceFile = projectData.SourceFiles[absolutePath];
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Opus.Core.Exception("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }

                // WPF application definition .xaml file
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.ApplicationDefinitionAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Opus.Core.Location)
                        {
                            var file = sourceField as Opus.Core.Location;
                            string absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                            }

#if false
                            // TODO: in theory, this file should be generated in VS, but it doesn't seem to
                            string csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Opus.Core.Exception("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath);
                            }
#endif

                            projectData.ApplicationDefinition = new ProjectFile(absolutePath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            Opus.Core.FileCollection sourceCollection = sourceField as Opus.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Opus.Core.Exception("There can be only one application definition");
                            }

                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                                }

#if false
                                // TODO: in theory, this file should be generated in VS, but it doesn't seem to
                                string csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Opus.Core.Exception("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath));
                                }
#endif

                                projectData.ApplicationDefinition = new ProjectFile(absolutePath);
                            }
                        }
                        else
                        {
                            throw new Opus.Core.Exception("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }

                // WPF page .xaml files
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.PagesAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Opus.Core.Location)
                        {
                            var file = sourceField as Opus.Core.Location;
                            string absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Page file '{0}' does not exist", absolutePath);
                            }

                            lock (projectData.Pages)
                            {
                                if (!projectData.Pages.Contains(absolutePath))
                                {
                                    projectData.Pages.Add(new ProjectFile(absolutePath));
                                }
                            }
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            Opus.Core.FileCollection sourceCollection = sourceField as Opus.Core.FileCollection;
                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception("Page file '{0}' does not exist", absolutePath);
                                }

                                string csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Opus.Core.Exception("Associated source file '{0}' to page file '{1}' does not exist", csPath, absolutePath);
                                }

                                lock (projectData.Pages)
                                {
                                    if (!projectData.Pages.Contains(absolutePath))
                                    {
                                        projectData.Pages.Add(new ProjectFile(absolutePath));
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Opus.Core.Exception("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }
            }

            configuration.Type = EProjectConfigurationType.Application;

            string toolName = "VCSCompiler";
            ProjectTool vcsCompiler = configuration.GetTool(toolName);
            if (null == vcsCompiler)
            {
                vcsCompiler = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcsCompiler);

                string outputDirectory = (options as CSharp.OptionCollection).OutputDirectoryPath;
                configuration.OutputDirectory = outputDirectory;

                if (options is VisualStudioProcessor.IVisualStudioSupport)
                {
                    VisualStudioProcessor.IVisualStudioSupport visualStudioProjectOption = options as VisualStudioProcessor.IVisualStudioSupport;
                    VisualStudioProcessor.ToolAttributeDictionary settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (System.Collections.Generic.KeyValuePair<string, string> setting in settingsDictionary)
                    {
                        vcsCompiler[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Opus.Core.Exception("Assembly options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}

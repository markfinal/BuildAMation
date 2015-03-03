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
namespace VSSolutionBuilder
{
    public partial class VSSolutionBuilder
    {
        public object
        Build(
            CSharp.Assembly moduleToBuild,
            out System.Boolean success)
        {
            var assemblyModule = moduleToBuild as Bam.Core.BaseModule;
            var node = assemblyModule.OwningNode;
            var target = node.Target;
            var options = assemblyModule.Options as CSharp.OptionCollection;

            var moduleName = node.ModuleName;

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
                    throw new Bam.Core.Exception("Unrecognized platform");
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
                    var solutionType = Bam.Core.State.Get("VSSolutionBuilder", "SolutionType") as System.Type;
                    var SolutionInstance = System.Activator.CreateInstance(solutionType);
                    var ProjectExtensionProperty = solutionType.GetProperty("ProjectExtension");
                    var projectExtension = ProjectExtensionProperty.GetGetMethod().Invoke(SolutionInstance, null) as string;

                    var projectDir = node.GetModuleBuildDirectoryLocation().GetSinglePath();
                    var projectPathName = System.IO.Path.Combine(projectDir, moduleName);
                    projectPathName += projectExtension;

                    var projectType = VSSolutionBuilder.GetProjectClassType();
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
                var groups = moduleToBuild.GetType().GetCustomAttributes(typeof(Bam.Core.ModuleGroupAttribute), true);
                if (groups.Length > 0)
                {
                    projectData.GroupName = (groups as Bam.Core.ModuleGroupAttribute[])[0].GroupName;
                }
            }

            if (node.ExternalDependents != null)
            {
                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (dependentNode.ModuleName == moduleName)
                    {
                        continue;
                    }

                    // TODO: want to remove this
                    lock (this.solutionFile.ProjectDictionary)
                    {
                        if (this.solutionFile.ProjectDictionary.ContainsKey(dependentNode.ModuleName))
                        {
                            var dependentProject = this.solutionFile.ProjectDictionary[dependentNode.ModuleName];
                            projectData.DependentProjects.Add(dependentProject);
                        }
                    }
                }
            }

            // references
            // TODO: convert to var
            foreach (Bam.Core.Location location in (options as CSharp.IOptions).References)
            {
                var reference = location.GetSinglePath();
                projectData.References.Add(reference);
            }

            var configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target, platformName);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
                    // TODO: fix me?
                    configuration = new ProjectConfiguration(configurationName, projectData);
                    configuration.CharacterSet = EProjectCharacterSet.NotSet;
                    projectData.Configurations.Add((Bam.Core.BaseTarget)target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                    projectData.Configurations.AddExistingForTarget((Bam.Core.BaseTarget)target, configuration);
                }
            }

            var fields = moduleToBuild.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // C# files
                {
                    var sourceFileAttributes = field.GetCustomAttributes(typeof(Bam.Core.SourceFilesAttribute), false);
                    if (null != sourceFileAttributes && sourceFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Bam.Core.Location)
                        {
                            var file = sourceField as Bam.Core.Location;
                            var absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Bam.Core.Exception("Source file '{0}' does not exist", absolutePath);
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
                        else if (sourceField is Bam.Core.FileCollection)
                        {
                            var sourceCollection = sourceField as Bam.Core.FileCollection;
                            // TODO: convert to var
                            foreach (Bam.Core.Location location in sourceCollection)
                            {
                                var absolutePath = location.GetSinglePath();
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Bam.Core.Exception("Source file '{0}' does not exist", absolutePath);
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
                            throw new Bam.Core.Exception("Field '{0}' of '{1}' should be of type Bam.Core.File or Bam.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }

                // WPF application definition .xaml file
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.ApplicationDefinitionAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Bam.Core.Location)
                        {
                            var file = sourceField as Bam.Core.Location;
                            var absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Bam.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                            }

#if false
                            // TODO: in theory, this file should be generated in VS, but it doesn't seem to
                            string csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Bam.Core.Exception("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath);
                            }
#endif

                            projectData.ApplicationDefinition = new ProjectFile(absolutePath);
                        }
                        else if (sourceField is Bam.Core.FileCollection)
                        {
                            var sourceCollection = sourceField as Bam.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Bam.Core.Exception("There can be only one application definition");
                            }

                            // TODO: convert to var
                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Bam.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                                }

#if false
                                // TODO: in theory, this file should be generated in VS, but it doesn't seem to
                                string csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Bam.Core.Exception("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath));
                                }
#endif

                                projectData.ApplicationDefinition = new ProjectFile(absolutePath);
                            }
                        }
                        else
                        {
                            throw new Bam.Core.Exception("Field '{0}' of '{1}' should be of type Bam.Core.File or Bam.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }

                // WPF page .xaml files
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.PagesAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Bam.Core.Location)
                        {
                            var file = sourceField as Bam.Core.Location;
                            var absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Bam.Core.Exception("Page file '{0}' does not exist", absolutePath);
                            }

                            lock (projectData.Pages)
                            {
                                if (!projectData.Pages.Contains(absolutePath))
                                {
                                    projectData.Pages.Add(new ProjectFile(absolutePath));
                                }
                            }
                        }
                        else if (sourceField is Bam.Core.FileCollection)
                        {
                            var sourceCollection = sourceField as Bam.Core.FileCollection;
                            // TODO: convert to var
                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Bam.Core.Exception("Page file '{0}' does not exist", absolutePath);
                                }

                                var csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Bam.Core.Exception("Associated source file '{0}' to page file '{1}' does not exist", csPath, absolutePath);
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
                            throw new Bam.Core.Exception("Field '{0}' of '{1}' should be of type Bam.Core.File or Bam.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }
            }

            configuration.Type = EProjectConfigurationType.Application;

            var toolName = "VCSCompiler";
            var vcsCompiler = configuration.GetTool(toolName);
            if (null == vcsCompiler)
            {
                vcsCompiler = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcsCompiler);
                configuration.OutputDirectory = moduleToBuild.Locations[CSharp.Assembly.OutputDir];

                if (options is VisualStudioProcessor.IVisualStudioSupport)
                {
                    var visualStudioProjectOption = options as VisualStudioProcessor.IVisualStudioSupport;
                    var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (var setting in settingsDictionary)
                    {
                        vcsCompiler[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Bam.Core.Exception("Assembly options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}

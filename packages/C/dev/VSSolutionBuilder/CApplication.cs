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
namespace C
{
namespace V2
{
    public sealed class VSSolutionLinker :
        ILinkerPolicy
    {
        void
        ILinkerPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> frameworks)
        {
            var application = new VSSolutionBuilder.V2.VSProjectProgram(sender, executablePath);
            foreach (var input in objectFiles)
            {
                application.SetCommonCompilationOptions(input, input.Settings);
                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        Bam.Core.V2.Settings patchSettings = null;
                        if (child.HasPatches)
                        {
                            patchSettings = System.Activator.CreateInstance(input.Settings.GetType(), child, false) as Bam.Core.V2.Settings;
                            child.ApplySettingsPatches(patchSettings, honourParents: false);
                        }
                        application.AddObjectFile(child, patchSettings, application.Configuration);
                    }
                }
                else
                {
                    application.AddObjectFile(input, null, null);
                }
            }
            foreach (var input in libraries)
            {
                application.AddStaticLibrary(input.MetaData as VSSolutionBuilder.V2.VSProjectStaticLibrary);
            }
        }
    }
}
}
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object
        Build(
            C.Application moduleToBuild,
            out bool success)
        {
            var applicationModule = moduleToBuild as Bam.Core.BaseModule;
            var node = applicationModule.OwningNode;
            var target = node.Target;
            var moduleName = node.ModuleName;

            IProject projectData = null;
            // TODO: want to remove this
            lock (this.solutionFile.ProjectDictionary)
            {
                if (this.solutionFile.ProjectDictionary.ContainsKey(moduleName))
                {
                    projectData = this.solutionFile.ProjectDictionary[moduleName];
                }
                else
                {
                    success = true;
                    return null;
                }
            }

            {
                var platformName = VSSolutionBuilder.GetPlatformNameFromTarget(target);
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

            if (null != node.ExternalDependents)
            {
                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (dependentNode.ModuleName != moduleName)
                    {
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
            }

            // these do get added as project dependencies, so if these are dynamic libaries, ensure they are C.Plugin modules
            // to avoid a link step
            if (null != node.RequiredDependents)
            {
                foreach (var dependentNode in node.RequiredDependents)
                {
                    if (dependentNode.ModuleName != moduleName)
                    {
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
            }

            var applicationOptions = applicationModule.Options;

            var configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
#if false
                    C.ILinkerOptions linkerOptions = applicationOptions as C.ILinkerOptions;
                    C.IToolchainOptions toolchainOptions = linkerOptions.ToolchainOptionCollection as C.IToolchainOptions;
                    EProjectCharacterSet characterSet;
                    switch (toolchainOptions.CharacterSet)
                    {
                        case C.ECharacterSet.NotSet:
                            characterSet = EProjectCharacterSet.NotSet;
                            break;

                        case C.ECharacterSet.Unicode:
                            characterSet = EProjectCharacterSet.UniCode;
                            break;

                        case C.ECharacterSet.MultiByte:
                            characterSet = EProjectCharacterSet.MultiByte;
                            break;

                        default:
                            characterSet = EProjectCharacterSet.Undefined;
                            break;
                    }
                    configuration.CharacterSet = characterSet;
#endif
                    configuration = new ProjectConfiguration(configurationName, projectData);
                    projectData.Configurations.Add((Bam.Core.BaseTarget)target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                    projectData.Configurations.AddExistingForTarget((Bam.Core.BaseTarget)target, configuration);
                }
            }

            var fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic;
            var fields = moduleToBuild.GetType().GetFields(fieldBindingFlags);
            foreach (var field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    var headerFileCollection = field.GetValue(moduleToBuild) as Bam.Core.FileCollection;
                    // TODO: replace with 'var'
                    foreach (Bam.Core.Location location in headerFileCollection)
                    {
                        var headerPath = location.GetSinglePath();
                        var cProject = projectData as ICProject;
                        lock (cProject.HeaderFiles)
                        {
                            if (!cProject.HeaderFiles.Contains(headerPath))
                            {
                                var headerFile = new ProjectFile(headerPath);
                                cProject.HeaderFiles.Add(headerFile);
                            }
                        }
                    }
                }
            }

            configuration.Type = EProjectConfigurationType.Application;

            var toolName = "VCLinkerTool";
            var vcCLLinkerTool = configuration.GetTool(toolName);
            if (null == vcCLLinkerTool)
            {
                vcCLLinkerTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLLinkerTool);

                var linkerOptions = applicationOptions as C.LinkerOptionCollection;
                configuration.OutputDirectory = moduleToBuild.Locations[C.Application.OutputDir];
                configuration.TargetName = linkerOptions.OutputName;

                if (applicationOptions is VisualStudioProcessor.IVisualStudioSupport)
                {
                    var visualStudioProjectOption = applicationOptions as VisualStudioProcessor.IVisualStudioSupport;
                    var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (var setting in settingsDictionary)
                    {
                        vcCLLinkerTool[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Bam.Core.Exception("Linker options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}

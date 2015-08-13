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
using Bam.Core.V2; // for EPlatform.PlatformExtensions
namespace C
{
namespace V2
{
    public sealed partial class VSSolutionLinker :
        ILinkerPolicy
    {
        void
        ILinkerPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> frameworks)
        {
            // inspect prebuilt library dependencies, which are added as part of the linker options
            foreach (var input in libraries)
            {
                if (input.MetaData != null)
                {
                    continue;
                }

                if (input is C.V2.StaticLibrary)
                {
                    // TODO: probably a simplification of the DLL codepath
                    throw new System.NotImplementedException();
                }
                else if (input is C.V2.DynamicLibrary)
                {
                    var linker = sender.Settings as C.V2.ICommonLinkerOptions;
                    if (sender.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        var libraryPath = input.GeneratedPaths[C.V2.DynamicLibrary.ImportLibraryKey].Parse();
                        var libraryDir = System.IO.Path.GetDirectoryName(libraryPath);
                        var libraryName = System.IO.Path.GetFileName(libraryPath);
                        linker.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(libraryDir, null));
                        linker.Libraries.AddUnique(libraryName);
                    }
                    else
                    {
                        var libraryPath = input.GeneratedPaths[C.V2.DynamicLibrary.Key].Parse();
                        var libraryDir = System.IO.Path.GetDirectoryName(libraryPath);
                        linker.LibraryPaths.AddUnique(Bam.Core.V2.TokenizedString.Create(libraryDir, null));
                        if ((sender.Tool as C.V2.LinkerTool).UseLPrefixLibraryPaths)
                        {
                            var libName = System.IO.Path.GetFileNameWithoutExtension(libraryPath);
                            libName = libName.Substring(3); // trim off lib prefix
                            linker.Libraries.AddUnique(System.String.Format("-l{0}", libName));
                        }
                        else
                        {
                            var libraryName = System.IO.Path.GetFileName(libraryPath);
                            linker.Libraries.AddUnique(libraryName);
                        }
                    }
                }
                else if (input is C.V2.CSDKModule)
                {
                    // do nothing for SDKs
                    continue;
                }
                else
                {
                    throw new Bam.Core.Exception("Don't know how to handle this library module, {0}", input.ToString());
                }
            }

            var platform = sender.Linker is VisualC.V2.Linker64 ? VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.SixtyFour : VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.ThirtyTwo;
            VSSolutionBuilder.V2.VSCommonLinkableProject application = null;
            if (sender is DynamicLibrary)
            {
                application = new VSSolutionBuilder.V2.VSProjectDynamicLibrary(sender, executablePath, platform);
            }
            else
            {
                application = new VSSolutionBuilder.V2.VSProjectProgram(sender, executablePath, platform);
            }
            var commonObject = objectFiles[0];
            application.SetCommonCompilationOptions(commonObject, commonObject.Settings);
            foreach (var input in objectFiles)
            {
                C.V2.SettingsBase deltaSettings = null;
                if (input != commonObject)
                {
                    deltaSettings = (input.Settings as C.V2.SettingsBase).Delta(commonObject.Settings, input);
                }

                if (input is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in input.Children)
                    {
                        C.V2.SettingsBase patchSettings = deltaSettings;
                        if (child.HasPatches)
                        {
                            if (null == patchSettings)
                            {
                                patchSettings = System.Activator.CreateInstance(input.Settings.GetType(), child, false) as C.V2.SettingsBase;
                            }
                            else
                            {
                                patchSettings = deltaSettings.Clone(child);
                            }
                            child.ApplySettingsPatches(patchSettings, honourParents: false);
                        }
                        application.AddObjectFile(child, patchSettings);
                    }
                }
                else
                {
                    application.AddObjectFile(input, deltaSettings);
                }
            }

            foreach (var header in headers)
            {
                if (header is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in header.Children)
                    {
                        application.AddHeaderFile(child as HeaderFile);
                    }
                }
                else
                {
                    application.AddHeaderFile(header as HeaderFile);
                }
            }

            // loop over libraries that have been built in projects, and which require a dependency set up
            foreach (var input in libraries)
            {
                if (null == input.MetaData)
                {
                    // prebuilts handled earlier
                    continue;
                }

                if (input is C.V2.StaticLibrary)
                {
                    application.AddStaticLibrary(input.MetaData as VSSolutionBuilder.V2.VSProjectStaticLibrary);
                }
                else if (input is C.V2.DynamicLibrary)
                {
                    application.AddDynamicLibrary(input.MetaData as VSSolutionBuilder.V2.VSProjectDynamicLibrary);
                }
                else if (input is C.V2.CSDKModule)
                {
                    // do nothing for SDKs
                    continue;
                }
                else
                {
                    throw new Bam.Core.Exception("Don't know how to handle this library module, {0}", input.ToString());
                }
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

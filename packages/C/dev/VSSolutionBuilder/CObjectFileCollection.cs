// <copyright file="CObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object Build(C.ObjectFileCollectionBase moduleToBuild, out bool success)
        {
            var objectFileCollectionModule = moduleToBuild as Opus.Core.BaseModule;
            var node = objectFileCollectionModule.OwningNode;
            if (null == node.Parent ||
                (node.Parent.Module.GetType().BaseType.BaseType == typeof(C.ObjectFileCollection) &&
                 null == node.Parent.Parent))
            {
                // utility project
                success = true;
                return null;
            }

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
                    var solutionType = Opus.Core.State.Get("VSSolutionBuilder", "SolutionType") as System.Type;
                    var SolutionInstance = System.Activator.CreateInstance(solutionType);
                    var ProjectExtensionProperty = solutionType.GetProperty("ProjectExtension");
                    var projectExtension = ProjectExtensionProperty.GetGetMethod().Invoke(SolutionInstance, null) as string;

                    var projectDir = node.GetModuleBuildDirectoryLocation().GetSinglePath();
                    var projectPathName = System.IO.Path.Combine(projectDir, moduleName);
                    projectPathName += projectExtension;

                    var projectType = VSSolutionBuilder.GetProjectClassType();
                    projectData = System.Activator.CreateInstance(projectType, new object[] { moduleName, projectPathName, node.Package.Identifier, objectFileCollectionModule.ProxyPath }) as IProject;

                    this.solutionFile.ProjectDictionary.Add(moduleName, projectData);
                }
            }

            {
                var platformName = VSSolutionBuilder.GetPlatformNameFromTarget(target);
                if (!projectData.Platforms.Contains(platformName))
                {
                    projectData.Platforms.Add(platformName);
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

            var configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            // TODO: want to remove this
            lock (this.solutionFile.ProjectConfigurations)
            {
                if (!this.solutionFile.ProjectConfigurations.ContainsKey(configurationName))
                {
                    this.solutionFile.ProjectConfigurations.Add(configurationName, new System.Collections.Generic.List<IProject>());
                }
            }
            this.solutionFile.ProjectConfigurations[configurationName].Add(projectData);

            var objectFileCollectionOptions = objectFileCollectionModule.Options;

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
#if false
                    C.ICCompilerOptions compilerOptions = objectFileCollectionOptions as C.ICCompilerOptions;
                    C.IToolchainOptions toolchainOptions = compilerOptions.ToolchainOptionCollection as C.IToolchainOptions;
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
                    projectData.Configurations.Add((Opus.Core.BaseTarget)target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
#if false
                    configuration.CharacterSet = (EProjectCharacterSet)((objectFileCollectionOptions as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions).CharacterSet;
#endif
                    projectData.Configurations.AddExistingForTarget((Opus.Core.BaseTarget)target, configuration);
                }

#if true
                configuration.IntermediateDirectory = moduleToBuild.Locations[C.ObjectFile.OutputDir];
#else
                C.CompilerOptionCollection options = objectFileCollectionOptions as C.CompilerOptionCollection;
                configuration.IntermediateDirectory = options.OutputDirectoryPath;
#endif
            }

            var toolName = "VCCLCompilerTool";
            var vcCLCompilerTool = configuration.GetTool(toolName);
            if (null == vcCLCompilerTool)
            {
                vcCLCompilerTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLCompilerTool);

                var commonOptions = node.EncapsulatingNode.Module as Opus.Core.ICommonOptionCollection;
                if ((commonOptions != null) &&
                    (commonOptions.CommonOptionCollection is VisualStudioProcessor.IVisualStudioSupport))
                {
                    var visualStudioProjectOption = (node.EncapsulatingNode.Module as Opus.Core.ICommonOptionCollection).CommonOptionCollection as VisualStudioProcessor.IVisualStudioSupport;
                    var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (var setting in settingsDictionary)
                    {
                        vcCLCompilerTool[setting.Key] = setting.Value;
                    }
                }
                else if (objectFileCollectionOptions is VisualStudioProcessor.IVisualStudioSupport)
                {
                    var visualStudioProjectOption = objectFileCollectionOptions as VisualStudioProcessor.IVisualStudioSupport;
                    var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (var setting in settingsDictionary)
                    {
                        vcCLCompilerTool[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Opus.Core.Exception("Compiler options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}
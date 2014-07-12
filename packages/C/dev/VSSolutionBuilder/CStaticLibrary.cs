// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object Build(C.StaticLibrary moduleToBuild, out bool success)
        {
            var staticLibraryModule = moduleToBuild as Opus.Core.BaseModule;
            var node = staticLibraryModule.OwningNode;
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
                var groups = moduleToBuild.GetType().GetCustomAttributes(typeof(Opus.Core.ModuleGroupAttribute), true);
                if (groups.Length > 0)
                {
                    projectData.GroupName = (groups as Opus.Core.ModuleGroupAttribute[])[0].GroupName;
                }
            }

            var staticLibraryOptions = staticLibraryModule.Options;

            var configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
#if false
                    C.IArchiverOptions archiverOptions = staticLibraryOptions as C.IArchiverOptions;
                    C.IToolchainOptions toolchainOptions = archiverOptions.ToolchainOptionCollection as C.IToolchainOptions;
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
                    projectData.Configurations.AddExistingForTarget((Opus.Core.BaseTarget)target, configuration);
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
                    var headerFileCollection = field.GetValue(moduleToBuild) as Opus.Core.FileCollection;
                    // TODO: change to var
                    foreach (Opus.Core.Location location in headerFileCollection)
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

            configuration.Type = EProjectConfigurationType.StaticLibrary;

            var toolName = "VCLibrarianTool";
            var vcCLLibrarianTool = configuration.GetTool(toolName);
            if (null == vcCLLibrarianTool)
            {
                vcCLLibrarianTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLLibrarianTool);

                var archiverOptions = staticLibraryOptions as C.ArchiverOptionCollection;
#if true
                configuration.OutputDirectory = moduleToBuild.Locations[C.StaticLibrary.OutputDirLocKey];
#else
                configuration.OutputDirectory = archiverOptions.OutputDirectoryPath;
#endif
                configuration.TargetName = archiverOptions.OutputName;

                if (staticLibraryOptions is VisualStudioProcessor.IVisualStudioSupport)
                {
                    var visualStudioProjectOption = staticLibraryOptions as VisualStudioProcessor.IVisualStudioSupport;
                    var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (var setting in settingsDictionary)
                    {
                        vcCLLibrarianTool[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Opus.Core.Exception("Archiver options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}
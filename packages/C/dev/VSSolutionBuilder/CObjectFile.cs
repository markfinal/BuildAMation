// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object Build(C.ObjectFile objectFile, out bool success)
        {
            Opus.Core.BaseModule objectFileModule = objectFile as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = objectFileModule.OwningNode;
            Opus.Core.Target target = node.Target;
            string moduleName = node.ModuleName;
            var moduleToolAttributes = objectFile.GetType().GetCustomAttributes(typeof(Opus.Core.ModuleToolAssignmentAttribute), true);
            System.Type toolType = (moduleToolAttributes[0] as Opus.Core.ModuleToolAssignmentAttribute).ToolType;
            Opus.Core.ITool toolInterface = target.Toolset.Tool(toolType);

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
                    System.Type solutionType = Opus.Core.State.Get("VSSolutionBuilder", "SolutionType") as System.Type;
                    object SolutionInstance = System.Activator.CreateInstance(solutionType);
                    System.Reflection.PropertyInfo ProjectExtensionProperty = solutionType.GetProperty("ProjectExtension");
                    string projectExtension = ProjectExtensionProperty.GetGetMethod().Invoke(SolutionInstance, null) as string;

                    string projectPathName = System.IO.Path.Combine(node.GetModuleBuildDirectory(), moduleName);
                    projectPathName += projectExtension;

                    System.Type projectType = VSSolutionBuilder.GetProjectClassType();
                    projectData = System.Activator.CreateInstance(projectType, new object[] { moduleName, projectPathName, node.Package.Identifier, objectFileModule.ProxyPath }) as IProject;

                    this.solutionFile.ProjectDictionary.Add(moduleName, projectData);
                }
            }

            {
                string platformName = VSSolutionBuilder.GetPlatformNameFromTarget(target);
                if (!projectData.Platforms.Contains(platformName))
                {
                    projectData.Platforms.Add(platformName);
                }
            }

            string configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            // TODO: want to remove this
            lock (this.solutionFile.ProjectConfigurations)
            {
                if (!this.solutionFile.ProjectConfigurations.ContainsKey(configurationName))
                {
                    this.solutionFile.ProjectConfigurations.Add(configurationName, new System.Collections.Generic.List<IProject>());
                }
            }
            this.solutionFile.ProjectConfigurations[configurationName].Add(projectData);

            Opus.Core.BaseOptionCollection objectFileOptions = objectFileModule.Options;

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
#if false
                    C.ICCompilerOptions compilerOptions = objectFileOptions as C.ICCompilerOptions;
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

                    projectData.Configurations.Add(target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
#if false
                    configuration.CharacterSet = (EProjectCharacterSet)((objectFileOptions as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions).CharacterSet;
#endif
                    projectData.Configurations.AddExistingForTarget(target, configuration);
                }

                C.CompilerOptionCollection options = objectFileOptions as C.CompilerOptionCollection;
                configuration.IntermediateDirectory = options.OutputDirectoryPath;
            }

            string sourceFilePath = objectFile.SourceFile.AbsolutePath;

            ProjectFile sourceFile;
            lock (projectData.SourceFiles)
            {
                if (!projectData.SourceFiles.Contains(sourceFilePath))
                {
                    sourceFile = new ProjectFile(sourceFilePath);
                    sourceFile.FileConfigurations = new ProjectFileConfigurationCollection();
                    projectData.SourceFiles.Add(sourceFile);
                }
                else
                {
                    sourceFile = projectData.SourceFiles[sourceFilePath];
                }
            }

            // TODO: this expression needs to be refactored
            if (null == node.Parent || (node.Parent.Module.GetType().BaseType.BaseType == typeof(C.ObjectFileCollectionBase) && null == node.Parent.Parent))
            {
                // this must be a utility configuration
                configuration.Type = EProjectConfigurationType.Utility;

                string executable = toolInterface.Executable((Opus.Core.BaseTarget)target);
                // TODO: pdb if it exists?

                Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
                if (executable.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("\"{0}\"", executable));
                }
                else
                {
                    commandLineBuilder.Add(executable);
                }
                if (objectFileOptions is CommandLineProcessor.ICommandLineSupport)
                {
                    CommandLineProcessor.ICommandLineSupport commandLineOption = objectFileOptions as CommandLineProcessor.ICommandLineSupport;
                    commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
                }
                else
                {
                    throw new Opus.Core.Exception("Compiler options does not support command line translation");
                }

                ProjectTool customTool = new ProjectTool("VCCustomBuildTool");

                C.ICompilerTool compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
                string objectFileSuffix = compilerTool.ObjectFileSuffix;

                string commandToken;
                string outputsToken;
                string messageToken;
                string message;
                string outputPathname;
                if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == projectData.VSTarget)
                {
                    outputPathname = System.String.Format("\"$(IntDir)$(InputName){0}\"", objectFileSuffix);

                    // add source file
                    commandLineBuilder.Add(@" $(InputPath)");

                    commandToken = "CommandLine";
                    outputsToken = "Outputs";
                    messageToken = "Description";
                    message = System.String.Format("Compiling $(InputFileName) with '{0}'", executable);
                }
                else
                {
                    outputPathname = System.String.Format("$(IntDir)%(Filename){0}", objectFileSuffix);

                    // add source file
                    commandLineBuilder.Add(@" %(FullPath)");

                    commandToken = "Command";
                    outputsToken = "Outputs";
                    messageToken = "Message";
                    message = System.String.Format("Compiling %(Filename)%(Extension) with {0}", executable);
                }
                customTool.AddAttribute(commandToken, commandLineBuilder.ToString(' '));
                customTool.AddAttribute(outputsToken, outputPathname);
                customTool.AddAttribute(messageToken, message);

                ProjectFileConfiguration fileConfiguration = new ProjectFileConfiguration(configuration, customTool, false);
                sourceFile.FileConfigurations.Add(fileConfiguration);
            }
            else
            {
                string toolName = "VCCLCompilerTool";

                // do not share the compiler tool in order to handle differences
                ProjectTool vcCLCompilerTool = new ProjectTool(toolName);

                // if the main configuration does not yet have an instance of this tool, add it (could happen if a single ObjectFile is added to a library or application)
                configuration.AddToolIfMissing(vcCLCompilerTool);

                // need to add each configuration a source file is applicable to in order to determine exclusions later
                ProjectFileConfiguration fileConfiguration = new ProjectFileConfiguration(configuration, vcCLCompilerTool, false);
                sourceFile.FileConfigurations.Add(fileConfiguration);

                if (objectFileOptions is VisualStudioProcessor.IVisualStudioSupport)
                {
                    VisualStudioProcessor.IVisualStudioSupport visualStudioProjectOption = objectFileOptions as VisualStudioProcessor.IVisualStudioSupport;
                    VisualStudioProcessor.ToolAttributeDictionary settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (System.Collections.Generic.KeyValuePair<string, string> setting in settingsDictionary)
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
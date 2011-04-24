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
            Opus.Core.DependencyNode node = objectFile.OwningNode;
            Opus.Core.Target target = node.Target;
            string moduleName = node.ModuleName;
            C.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target);
            C.Compiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool);
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;

            ProjectData projectData = null;
            // TODO: want to remove this
            lock (this.solutionFile.ProjectDictionary)
            {
                if (this.solutionFile.ProjectDictionary.ContainsKey(moduleName))
                {
                    projectData = this.solutionFile.ProjectDictionary[moduleName];
                }
                else
                {
                    string projectPathName = System.IO.Path.Combine(node.GetModuleBuildDirectory(), moduleName);
                    projectPathName += ".vcproj";

                    projectData = new ProjectData(moduleName, projectPathName, node.Package.Directory);
                    projectData.Platforms.Add(VSSolutionBuilder.GetPlatformNameFromTarget(target));
                    this.solutionFile.ProjectDictionary.Add(moduleName, projectData);
                }
            }

            string configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            // TODO: want to remove this
            lock (this.solutionFile.ProjectConfigurations)
            {
                if (!this.solutionFile.ProjectConfigurations.ContainsKey(configurationName))
                {
                    this.solutionFile.ProjectConfigurations.Add(configurationName, new System.Collections.Generic.List<ProjectData>());
                }
            }
            this.solutionFile.ProjectConfigurations[configurationName].Add(projectData);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
                    configuration = new ProjectConfiguration(configurationName, (objectFile.Options as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions, projectData);

                    C.CompilerOptionCollection options = objectFile.Options as C.CompilerOptionCollection;
                    configuration.IntermediateDirectory = options.OutputDirectoryPath;

                    projectData.Configurations.Add(configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                    if ((C.ECharacterSet)configuration.CharacterSet != ((objectFile.Options as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions).CharacterSet)
                    {
                        throw new Opus.Core.Exception("Inconsistent character set in project");
                    }
                }
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

                string executable = compilerTool.Executable(target);
                string outputPathname = System.String.Format("\"$(IntDir)\\$(InputName){0}\"", toolchain.ObjectFileExtension);
                // TODO: pdb if it exists?

                Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
                commandLineBuilder.Add(System.String.Format("\"{0}\" ", executable));
                if (objectFile.Options is CommandLineProcessor.ICommandLineSupport)
                {
                    CommandLineProcessor.ICommandLineSupport commandLineOption = objectFile.Options as CommandLineProcessor.ICommandLineSupport;
                    commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
                }
                else
                {
                    throw new Opus.Core.Exception("Compiler options does not support command line translation");
                }

                // add source file
                commandLineBuilder.Add(@" $(InputPath)");

                ProjectTool customTool = new ProjectTool("VCCustomBuildTool");
                customTool.AddAttribute("CommandLine", commandLineBuilder.ToString(' '));
                customTool.AddAttribute("Outputs", outputPathname);
                customTool.AddAttribute("Description", System.String.Format("Compiling $(InputFileName) with '{0}'", executable));

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

                if (objectFile.Options is VisualStudioProcessor.IVisualStudioSupport)
                {
                    VisualStudioProcessor.IVisualStudioSupport visualStudioProjectOption = objectFile.Options as VisualStudioProcessor.IVisualStudioSupport;
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
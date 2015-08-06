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
    public sealed class VSSolutionCompilation :
        ICompilationPolicy
    {
        void
        ICompilationPolicy.Compile(
            ObjectFile sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString objectFilePath,
            Bam.Core.V2.Module source)
        {
            if (!(sender is Bam.Core.V2.IChildModule) || (null == (sender as Bam.Core.V2.IChildModule).Parent))
            {
                Bam.Core.Log.DebugMessage("Standalone source file {0} not converted into VS project", source.GeneratedPaths[C.V2.SourceFile.Key].Parse());
                // do nothing for source files not part of a library or application
                return;
            }
            var platform = (sender.Compiler is VisualC.Compiler64 || sender.Compiler is VisualC.CxxCompiler64) ? VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.SixtyFour : VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.ThirtyTwo;
            var objectFile = new VSSolutionBuilder.V2.VSProjectObjectFile(sender, objectFilePath, platform);
            objectFile.Source = source.GeneratedPaths[C.V2.SourceFile.Key];
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
            C.ObjectFile moduleToBuild,
            out bool success)
        {
            var objectFileModule = moduleToBuild as Bam.Core.BaseModule;
            var node = objectFileModule.OwningNode;
            var target = node.Target;
            var moduleName = node.ModuleName;
            var moduleToolAttributes = moduleToBuild.GetType().GetCustomAttributes(typeof(Bam.Core.ModuleToolAssignmentAttribute), true);
            var toolType = (moduleToolAttributes[0] as Bam.Core.ModuleToolAssignmentAttribute).ToolType;
            var toolInterface = target.Toolset.Tool(toolType);

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
                    var solutionType = Bam.Core.State.Get("VSSolutionBuilder", "SolutionType") as System.Type;
                    var SolutionInstance = System.Activator.CreateInstance(solutionType);
                    var ProjectExtensionProperty = solutionType.GetProperty("ProjectExtension");
                    var projectExtension = ProjectExtensionProperty.GetGetMethod().Invoke(SolutionInstance, null) as string;

                    var projectDir = node.GetModuleBuildDirectoryLocation().GetSinglePath();
                    var projectPathName = System.IO.Path.Combine(projectDir, moduleName);
                    projectPathName += projectExtension;

                    var projectType = VSSolutionBuilder.GetProjectClassType();
                    projectData = System.Activator.CreateInstance(projectType, new object[] { moduleName, projectPathName, node.Package.Identifier, objectFileModule.ProxyPath }) as IProject;

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

            var objectFileOptions = objectFileModule.Options;

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

                    projectData.Configurations.Add((Bam.Core.BaseTarget)target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
#if false
                    configuration.CharacterSet = (EProjectCharacterSet)((objectFileOptions as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions).CharacterSet;
#endif
                    projectData.Configurations.AddExistingForTarget((Bam.Core.BaseTarget)target, configuration);
                }

                configuration.IntermediateDirectory = moduleToBuild.Locations[C.ObjectFile.OutputDir];
            }

            var sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();

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

                var executable = toolInterface.Executable((Bam.Core.BaseTarget)target);
                // TODO: pdb if it exists? WHY would a PDB file be needed?

                var commandLineBuilder = new Bam.Core.StringArray();
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
                    var commandLineOption = objectFileOptions as CommandLineProcessor.ICommandLineSupport;
                    commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
                }
                else
                {
                    throw new Bam.Core.Exception("Compiler options does not support command line translation");
                }

                var customTool = new ProjectTool("VCCustomBuildTool");

                var compilerTool = target.Toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;
                var objectFileSuffix = compilerTool.ObjectFileSuffix;

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

                var fileConfiguration = new ProjectFileConfiguration(configuration, customTool, false);
                sourceFile.FileConfigurations.Add(fileConfiguration);
            }
            else
            {
                var toolName = "VCCLCompilerTool";

                // create a brand new compiler tool to set the compiler options for this object file
                var vcCLCompilerTool = new ProjectTool(toolName);

                // need to add, each configuration a source file is applicable to, in order to determine exclusions later
                var fileConfiguration = new ProjectFileConfiguration(configuration, vcCLCompilerTool, false);
                sourceFile.FileConfigurations.Add(fileConfiguration);

                Bam.Core.BaseOptionCollection deltaOptionCollection = null;
                if (node.EncapsulatingNode.Module is Bam.Core.ICommonOptionCollection)
                {
                    var commonOptions = (node.EncapsulatingNode.Module as Bam.Core.ICommonOptionCollection).CommonOptionCollection;
                    if (commonOptions is C.ICCompilerOptions)
                    {
                        deltaOptionCollection = moduleToBuild.Options.Complement(commonOptions);
                    }
                }

                // only need to write the delta options if they exist
                // the super-set of options are written by the objectfilecollection
                if ((deltaOptionCollection != null) && !deltaOptionCollection.Empty)
                {
                    if (deltaOptionCollection is VisualStudioProcessor.IVisualStudioSupport)
                    {
                        var visualStudioProjectOption = deltaOptionCollection as VisualStudioProcessor.IVisualStudioSupport;
                        var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                        // there was a switch between VCPROJ and MSBuild style projects in regard of when inheritance of values happen
                        // VCPROJ: automatic, manual disabling
                        // MSBuild: disabled inheritence, manual enable by quoting name of state
                        var postFixInheritance = (VisualStudioProcessor.EVisualStudioTarget.MSBUILD == projectData.VSTarget);

                        // this is ONLY setting the delta
                        foreach (var setting in settingsDictionary)
                        {
                            if (postFixInheritance && settingsDictionary.CanInherit(setting.Key))
                            {
                                vcCLCompilerTool[setting.Key] = System.String.Format("{0};%({1})", setting.Value, setting.Key);
                            }
                            else
                            {
                                vcCLCompilerTool[setting.Key] = setting.Value;
                            }
                        }
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Compiler options does not support VisualStudio project translation");
                    }
                }
                else if ((node.Parent != null) && !(node.Parent.Module is C.ObjectFileCollectionBase))
                {
                    // this is a child object, but not part of an object collection
                    vcCLCompilerTool = new ProjectTool(toolName);
                    configuration.AddToolIfMissing(vcCLCompilerTool);

                    if (moduleToBuild.Options is VisualStudioProcessor.IVisualStudioSupport)
                    {
                        var visualStudioProjectOption = moduleToBuild.Options as VisualStudioProcessor.IVisualStudioSupport;
                        var settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                        // this is ONLY setting the delta
                        foreach (var setting in settingsDictionary)
                        {
                            vcCLCompilerTool[setting.Key] = setting.Value;
                        }
                    }
                    else
                    {
                        throw new Bam.Core.Exception("Compiler options does not support VisualStudio project translation");
                    }
                }
            }

            success = true;
            return projectData;
        }
    }
}

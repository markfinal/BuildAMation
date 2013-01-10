// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object Build(C.Win32Resource resourceFile, out bool success)
        {
            Opus.Core.BaseModule resourceFileModule = resourceFile as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = resourceFileModule.OwningNode;
            Opus.Core.Target target = node.Target;

            // do not generate a project file for this module
            // instead, find what it is used by, and add it into that project
            Opus.Core.DependencyNode parentNode = node.Parent;
            Opus.Core.DependencyNode targetNode;
            if ((null != parentNode) && (parentNode.Module is Opus.Core.IModuleCollection))
            {
                targetNode = parentNode.ExternalDependentFor[0];
            }
            else
            {
                targetNode = node.ExternalDependentFor[0];
            }

            string moduleName = targetNode.ModuleName;

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

                    string projectPathName = System.IO.Path.Combine(targetNode.GetModuleBuildDirectory(), moduleName);
                    projectPathName += projectExtension;

                    System.Type projectType = VSSolutionBuilder.GetProjectClassType();
                    projectData = System.Activator.CreateInstance(projectType, new object[] { moduleName, projectPathName, targetNode.Package.Identifier, resourceFileModule.ProxyPath }) as IProject;

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

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
                    configuration = new ProjectConfiguration(configurationName, projectData);
                    projectData.Configurations.Add(target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                    projectData.Configurations.AddExistingForTarget(target, configuration);
                }

                // Don't overwrite the intermediate directory
                // since C output is "prioritized" over resource compilation
#if false
                C.Win32ResourceCompilerOptionCollection options = resourceFile.Options as C.Win32ResourceCompilerOptionCollection;
                configuration.IntermediateDirectory = options.OutputDirectoryPath;
#endif
            }

            string resourceFilePath = resourceFile.ResourceFile.AbsolutePath;
            if (!System.IO.File.Exists(resourceFilePath))
            {
                throw new Opus.Core.Exception("Resource file '{0}' does not exist", resourceFilePath);
            }

            ProjectFile sourceFile;
            ICProject cProjectData = projectData as ICProject;
            lock (cProjectData.ResourceFiles)
            {
                if (!cProjectData.ResourceFiles.Contains(resourceFilePath))
                {
                    sourceFile = new ProjectFile(resourceFilePath);
                    sourceFile.FileConfigurations = new ProjectFileConfigurationCollection();
                    cProjectData.ResourceFiles.Add(sourceFile);
                }
                else
                {
                    sourceFile = cProjectData.ResourceFiles[resourceFilePath];
                }
            }

            Opus.Core.BaseOptionCollection resourceFileOptions = resourceFileModule.Options;

            {
                string toolName = "VCResourceCompilerTool";

                // do not share the compiler tool in order to handle differences
                ProjectTool vcResourceCompilerTool = new ProjectTool(toolName);

                // if the main configuration does not yet have an instance of this tool, add it (could happen if a single ObjectFile is added to a library or application)
                configuration.AddToolIfMissing(vcResourceCompilerTool);

                // need to add each configuration a source file is applicable to in order to determine exclusions later
                ProjectFileConfiguration fileConfiguration = new ProjectFileConfiguration(configuration, vcResourceCompilerTool, false);
                sourceFile.FileConfigurations.Add(fileConfiguration);

                if (resourceFileOptions is VisualStudioProcessor.IVisualStudioSupport)
                {
                    VisualStudioProcessor.IVisualStudioSupport visualStudioProjectOption = resourceFileOptions as VisualStudioProcessor.IVisualStudioSupport;
                    VisualStudioProcessor.ToolAttributeDictionary settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (System.Collections.Generic.KeyValuePair<string, string> setting in settingsDictionary)
                    {
                        vcResourceCompilerTool[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Opus.Core.Exception("Compiler options does not support VisualStudio project translation");
                }

                // add the output file spec
                C.Win32ResourceCompilerOptionCollection compilerOptions = resourceFileOptions as C.Win32ResourceCompilerOptionCollection;
                vcResourceCompilerTool["ResourceOutputFileName"] = compilerOptions.CompiledResourceFilePath;
            }

            success = true;
            return projectData;
        }
    }
}
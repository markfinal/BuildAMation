// <copyright file="CObjectFileCollection.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object Build(C.ObjectFileCollectionBase objectFileCollection, out bool success)
        {
            Opus.Core.DependencyNode node = objectFileCollection.OwningNode;
            if (null == node.Parent || (node.Parent.Module.GetType().BaseType.BaseType == typeof(C.ObjectFileCollection) && null == node.Parent.Parent))
            {
                // utility project
                success = true;
                return null;
            }

            Opus.Core.Target target = node.Target;
            string moduleName = node.ModuleName;

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
                    configuration = new ProjectConfiguration(configurationName, (objectFileCollection.Options as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions, projectData);

                    C.CompilerOptionCollection options = objectFileCollection.Options as C.CompilerOptionCollection;
                    configuration.IntermediateDirectory = options.OutputDirectoryPath;

                    projectData.Configurations.Add(configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                    if ((C.ECharacterSet)configuration.CharacterSet != ((objectFileCollection.Options as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions).CharacterSet)
                    {
                        throw new Opus.Core.Exception("Inconsistent character set in project");
                    }
                }
            }

            string toolName = "VCCLCompilerTool";
            ProjectTool vcCLCompilerTool = configuration.GetTool(toolName);
            if (null == vcCLCompilerTool)
            {
                vcCLCompilerTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLCompilerTool);

                if (objectFileCollection.Options is VisualStudioProcessor.IVisualStudioSupport)
                {
                    VisualStudioProcessor.IVisualStudioSupport visualStudioProjectOption = objectFileCollection.Options as VisualStudioProcessor.IVisualStudioSupport;
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
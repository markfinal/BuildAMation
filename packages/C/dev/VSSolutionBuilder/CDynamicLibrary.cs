// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object Build(C.DynamicLibrary dynamicLibrary, out bool success)
        {
            Opus.Core.DependencyNode node = dynamicLibrary.OwningNode;
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
            if (null != node.ExternalDependents)
            {
                foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    if (dependentNode.ModuleName != moduleName)
                    {
                        // TODO: want to remove this
                        lock (this.solutionFile.ProjectDictionary)
                        {
                            if (this.solutionFile.ProjectDictionary.ContainsKey(dependentNode.ModuleName))
                            {
                                ProjectData dependentProject = this.solutionFile.ProjectDictionary[dependentNode.ModuleName];
                                projectData.DependentProjects.Add(dependentProject);
                            }
                        }
                    }
                }
            }

            string configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
                    configuration = new ProjectConfiguration(configurationName, (dynamicLibrary.Options as C.ILinkerOptions).ToolchainOptionCollection as C.IToolchainOptions, projectData);
                    projectData.Configurations.Add(configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                }
            }

            System.Reflection.BindingFlags fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                                               System.Reflection.BindingFlags.Public |
                                                               System.Reflection.BindingFlags.NonPublic;
            System.Reflection.FieldInfo[] fields = dynamicLibrary.GetType().GetFields(fieldBindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    Opus.Core.FileCollection headerFileCollection = field.GetValue(dynamicLibrary) as Opus.Core.FileCollection;
                    foreach (string headerPath in headerFileCollection)
                    {
                        lock (projectData.HeaderFiles)
                        {
                            if (!projectData.HeaderFiles.Contains(headerPath))
                            {
                                ProjectFile headerFile = new ProjectFile(headerPath);
                                projectData.HeaderFiles.Add(headerFile);
                            }
                        }
                    }
                }
            }

            configuration.Type = EProjectConfigurationType.DynamicLibrary;

            string toolName = "VCLinkerTool";
            ProjectTool vcCLLinkerTool = configuration.GetTool(toolName);
            if (null == vcCLLinkerTool)
            {
                vcCLLinkerTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLLinkerTool);

                string outputDirectory = (dynamicLibrary.Options as C.LinkerOptionCollection).OutputDirectoryPath;
                configuration.OutputDirectory = outputDirectory;

                if (dynamicLibrary.Options is VisualStudioProcessor.IVisualStudioSupport)
                {
                    VisualStudioProcessor.IVisualStudioSupport visualStudioProjectOption = dynamicLibrary.Options as VisualStudioProcessor.IVisualStudioSupport;
                    VisualStudioProcessor.ToolAttributeDictionary settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (System.Collections.Generic.KeyValuePair<string, string> setting in settingsDictionary)
                    {
                        vcCLLinkerTool[setting.Key] = setting.Value;
                    }
                }
                else
                {
                    throw new Opus.Core.Exception("Linker options does not support VisualStudio project translation");
                }
            }

            success = true;
            return projectData;
        }
    }
}
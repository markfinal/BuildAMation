// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, out bool success)
        {
            Opus.Core.DependencyNode node = staticLibrary.OwningNode;
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

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
                    configuration = new ProjectConfiguration(configurationName, (staticLibrary.Options as C.IArchiverOptions).ToolchainOptionCollection as C.IToolchainOptions, projectData);
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
            System.Reflection.FieldInfo[] fields = staticLibrary.GetType().GetFields(fieldBindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    Opus.Core.FileCollection headerFileCollection = field.GetValue(staticLibrary) as Opus.Core.FileCollection;
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

            configuration.Type = EProjectConfigurationType.StaticLibrary;

            string toolName = "VCLibrarianTool";
            ProjectTool vcCLLibrarianTool = configuration.GetTool(toolName);
            if (null == vcCLLibrarianTool)
            {
                vcCLLibrarianTool = new ProjectTool(toolName);
                configuration.AddToolIfMissing(vcCLLibrarianTool);

                string outputDirectory = (staticLibrary.Options as C.ArchiverOptionCollection).OutputDirectoryPath;
                configuration.OutputDirectory = outputDirectory;

                if (staticLibrary.Options is VisualStudioProcessor.IVisualStudioSupport)
                {
                    VisualStudioProcessor.IVisualStudioSupport visualStudioProjectOption = staticLibrary.Options as VisualStudioProcessor.IVisualStudioSupport;
                    VisualStudioProcessor.ToolAttributeDictionary settingsDictionary = visualStudioProjectOption.ToVisualStudioProjectAttributes(target);

                    foreach (System.Collections.Generic.KeyValuePair<string, string> setting in settingsDictionary)
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
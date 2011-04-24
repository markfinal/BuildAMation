// <copyright file="CHeaderLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object Build(C.HeaderLibrary headerLibrary, out bool success)
        {
            Opus.Core.DependencyNode node = headerLibrary.OwningNode;
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
                    // arbitrary character set, as nothing is built
                    configuration = new ProjectConfiguration(configurationName, C.ECharacterSet.NotSet, projectData);
                    projectData.Configurations.Add(configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                }
            }

            configuration.Type = EProjectConfigurationType.Utility;

            System.Reflection.BindingFlags fieldBindingFlags = System.Reflection.BindingFlags.Instance |
                                                               System.Reflection.BindingFlags.Public |
                                                               System.Reflection.BindingFlags.NonPublic;
            System.Reflection.FieldInfo[] fields = headerLibrary.GetType().GetFields(fieldBindingFlags);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                var headerFileAttributes = field.GetCustomAttributes(typeof(C.HeaderFilesAttribute), false);
                if (headerFileAttributes.Length > 0)
                {
                    Opus.Core.FileCollection headerFileCollection = field.GetValue(headerLibrary) as Opus.Core.FileCollection;
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

            success = true;
            return projectData;
        }
    }
}
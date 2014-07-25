// <copyright file="CHeaderLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed partial class VSSolutionBuilder
    {
        public object
        Build(
            C.HeaderLibrary moduleToBuild,
            out bool success)
        {
            var headerLibraryModule = moduleToBuild as Opus.Core.BaseModule;
            var node = headerLibraryModule.OwningNode;
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
                    projectData = System.Activator.CreateInstance(projectType, new object[] { moduleName, projectPathName, node.Package.Identifier, headerLibraryModule.ProxyPath }) as IProject;

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

            // solution folder
            {
                var groups = moduleToBuild.GetType().GetCustomAttributes(typeof(Opus.Core.ModuleGroupAttribute), true);
                if (groups.Length > 0)
                {
                    projectData.GroupName = (groups as Opus.Core.ModuleGroupAttribute[])[0].GroupName;
                }
            }

            var configurationName = VSSolutionBuilder.GetConfigurationNameFromTarget(target);

            ProjectConfiguration configuration;
            lock (projectData.Configurations)
            {
                if (!projectData.Configurations.Contains(configurationName))
                {
                    configuration = new ProjectConfiguration(configurationName, projectData);
                    // arbitrary character set, as nothing is built
                    configuration.CharacterSet = EProjectCharacterSet.NotSet;
                    projectData.Configurations.Add((Opus.Core.BaseTarget)target, configuration);
                }
                else
                {
                    configuration = projectData.Configurations[configurationName];
                    projectData.Configurations.AddExistingForTarget((Opus.Core.BaseTarget)target, configuration);
                }
            }

            configuration.Type = EProjectConfigurationType.Utility;

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

            success = true;
            return projectData;
        }
    }
}

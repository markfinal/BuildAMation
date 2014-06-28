// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public partial class VSSolutionBuilder
    {
        public object Build(QtCommon.MocFile moduleToBuild, out System.Boolean success)
        {
            var mocFileModule = moduleToBuild as Opus.Core.BaseModule;
            var node = mocFileModule.OwningNode;
            var target = node.Target;
            var mocFileOptions = mocFileModule.Options;

            var parentNode = node.Parent;
            Opus.Core.DependencyNode targetNode;
            if ((null != parentNode) && (parentNode.Module is Opus.Core.IModuleCollection))
            {
                targetNode = parentNode.ExternalDependentFor[0];
            }
            else
            {
                targetNode = node.ExternalDependentFor[0];
            }

            IProject projectData = null;
            // TODO: want to remove this
            var moduleName = targetNode.ModuleName;
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

                    var projectPathName = System.IO.Path.Combine(targetNode.GetModuleBuildDirectory(), moduleName);
                    projectPathName += projectExtension;

                    var projectType = VSSolutionBuilder.GetProjectClassType();
                    projectData = System.Activator.CreateInstance(projectType, new object[] { moduleName, projectPathName, targetNode.Package.Identifier, mocFileModule.ProxyPath }) as IProject;

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
                }
            }

            var sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();

            ProjectFile sourceFile;
            var cProject = projectData as ICProject;
            lock (cProject.HeaderFiles)
            {
                if (!cProject.HeaderFiles.Contains(sourceFilePath))
                {
                    sourceFile = new ProjectFile(sourceFilePath);
                    sourceFile.FileConfigurations = new ProjectFileConfigurationCollection();
                    cProject.HeaderFiles.Add(sourceFile);
                }
                else
                {
                    sourceFile = cProject.HeaderFiles[sourceFilePath];
                }
            }

            var tool = target.Toolset.Tool(typeof(QtCommon.IMocTool));
            var toolExePath = tool.Executable((Opus.Core.BaseTarget)target);

            var commandLineBuilder = new Opus.Core.StringArray();
            if (toolExePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", toolExePath));
            }
            else
            {
                commandLineBuilder.Add(toolExePath);
            }
            if (mocFileOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = mocFileOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            var customTool = new ProjectTool("VCCustomBuildTool");

            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == projectData.VSTarget)
            {
                // add source file
                commandLineBuilder.Add(@" $(InputPath)");

#if true
                var mocPathName = moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSinglePath();
#else
                string mocPathName = mocFileOptions.OutputPaths[QtCommon.OutputFileFlags.MocGeneratedSourceFile];
#endif
                var outputPathname = mocPathName;
                var commandLine = System.String.Format("IF NOT EXIST {0} MKDIR {0}{1}{2}", System.IO.Path.GetDirectoryName(mocPathName), System.Environment.NewLine, commandLineBuilder.ToString(' '));
                customTool.AddAttribute("CommandLine", commandLine);

                customTool.AddAttribute("Outputs", outputPathname);

                customTool.AddAttribute("Description", "Moc'ing $(InputPath)...");
                customTool.AddAttribute("AdditionalDependencies", toolExePath);

            }
            else
            {
                // add source file
                commandLineBuilder.Add(@" %(FullPath)");

#if true
                var mocPathName = moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSinglePath();
#else
                string mocPathName = mocFileOptions.OutputPaths[QtCommon.OutputFileFlags.MocGeneratedSourceFile];
#endif
                var outputPathname = mocPathName;
                var commandLine = System.String.Format("IF NOT EXIST {0} MKDIR {0}{1}{2}", System.IO.Path.GetDirectoryName(mocPathName), System.Environment.NewLine, commandLineBuilder.ToString(' '));
                customTool.AddAttribute("Command", commandLine);

                customTool.AddAttribute("Outputs", outputPathname);

                customTool.AddAttribute("Message", "Moc'ing %(FullPath)...");
                customTool.AddAttribute("AdditionalInputs", toolExePath);
            }

            var fileConfiguration = new ProjectFileConfiguration(configuration, customTool, false);
            sourceFile.FileConfigurations.Add(fileConfiguration);

            success = true;
            return null;
        }
    }
}
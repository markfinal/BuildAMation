// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public partial class VSSolutionBuilder
    {
        public object Build(QtCommon.MocFile mocFile, out System.Boolean success)
        {
            Opus.Core.DependencyNode node = mocFile.OwningNode;
            Opus.Core.Target target = node.Target;
            QtCommon.MocTool tool = new QtCommon.MocTool();
            QtCommon.MocOptionCollection toolOptions = mocFile.Options as QtCommon.MocOptionCollection;
            string toolExePath = tool.Executable(target);

            Opus.Core.DependencyNode parentNode = node.Parent;
            Opus.Core.DependencyNode targetNode;
            if (parentNode.Module is Opus.Core.IModuleCollection)
            {
                targetNode = parentNode.ExternalDependentFor[0];
            }
            else
            {
                targetNode = node.ExternalDependentFor[0];
            }

            IProject projectData = null;
            // TODO: want to remove this
            string moduleName = targetNode.ModuleName;
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
                    projectData = System.Activator.CreateInstance(projectType, new object[] { moduleName, projectPathName, targetNode.Package.Identifier, mocFile.ProxyPath }) as IProject;

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
                }
            }

            string sourceFilePath = mocFile.SourceFile.AbsolutePath;

            ProjectFile sourceFile;
            ICProject cProject = projectData as ICProject;
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

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (toolExePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", toolExePath));
            }
            else
            {
                commandLineBuilder.Add(toolExePath);
            }
            if (mocFile.Options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = mocFile.Options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            ProjectTool customTool = new ProjectTool("VCCustomBuildTool");

            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == projectData.VSTarget)
            {
                // add source file
                commandLineBuilder.Add(@" $(InputPath)");

                string mocPathName = mocFile.Options.OutputPaths[QtCommon.OutputFileFlags.MocGeneratedSourceFile];
                string outputPathname = mocPathName;
                string commandLine = System.String.Format("IF NOT EXIST {0} MKDIR {0}\n\r{1}", System.IO.Path.GetDirectoryName(mocPathName), commandLineBuilder.ToString(' '));
                customTool.AddAttribute("CommandLine", commandLine);

                customTool.AddAttribute("Outputs", outputPathname);

                customTool.AddAttribute("Description", "Moc'ing $(InputPath)...");
                customTool.AddAttribute("AdditionalDependencies", toolExePath);

            }
            else
            {
                // add source file
                commandLineBuilder.Add(@" %(FullPath)");

                string mocPathName = mocFile.Options.OutputPaths[QtCommon.OutputFileFlags.MocGeneratedSourceFile];
                string outputPathname = mocPathName;
                string commandLine = System.String.Format("IF NOT EXIST {0} MKDIR {0}\n\r{1}", System.IO.Path.GetDirectoryName(mocPathName), commandLineBuilder.ToString(' '));
                customTool.AddAttribute("Command", commandLine);

                customTool.AddAttribute("Outputs", outputPathname);

                customTool.AddAttribute("Message", "Moc'ing %(FullPath)...");
                customTool.AddAttribute("AdditionalInputs", toolExePath);
            }

            ProjectFileConfiguration fileConfiguration = new ProjectFileConfiguration(configuration, customTool, false);
            sourceFile.FileConfigurations.Add(fileConfiguration);

            success = true;
            return null;
        }
    }
}
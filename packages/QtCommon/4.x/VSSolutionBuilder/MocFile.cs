#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace QtCommon
{
namespace V2
{
    public sealed class VSSolutionMocGeneration :
        IMocGenerationPolicy
    {
        void
        IMocGenerationPolicy.Moc(
            MocModule sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.ICommandLineTool mocCompiler,
            Bam.Core.V2.TokenizedString generatedMocSource,
            C.V2.HeaderFile source)
        {
#if true
            var encapsulating = sender.GetEncapsulatingReferencedModule();

            var solution = Bam.Core.V2.Graph.Instance.MetaData as VSSolutionBuilder.V2.VSSolution;
            var project = solution.EnsureProjectExists(encapsulating);
            var config = project.GetConfiguration(encapsulating);

            var output = generatedMocSource.Parse();

            var args = new Bam.Core.StringArray();
            args.Add(mocCompiler.Executable.Parse());
            args.Add(System.String.Format("-o{0}", output));
            args.Add("%(FullPath)");

            var customBuild = config.GetSettingsGroup(VSSolutionBuilder.V2.VSSettingsGroup.ESettingsGroup.CustomBuild, include: source.InputPath, uniqueToProject: true);
            customBuild.AddSetting("Command", args.ToString(' '), condition: config.ConditionText);
            customBuild.AddSetting("Message", System.String.Format("Moccing {0}", System.IO.Path.GetFileName(source.InputPath.Parse())), condition: config.ConditionText);
            customBuild.AddSetting("Outputs", output, condition: config.ConditionText);
#else
            if (null != source.MetaData)
            {
                throw new Bam.Core.Exception("Header file {0} already has VSSolution metadata", source.InputPath.Parse());
            }

            // TODO: this is hardcoded - needed a better way to figure this out
            var platform = VSSolutionBuilder.V2.VSSolutionMeta.EPlatform.SixtyFour;

            var output = generatedMocSource.Parse();

            var args = new Bam.Core.StringArray();
            args.Add(mocCompiler.Executable.Parse());
            args.Add(System.String.Format("-o{0}", output));
            args.Add("%(FullPath)");

            var customBuild = new VSSolutionBuilder.V2.VSProjectCustomBuild(source, platform);
            customBuild.Message = "Moccing";
            customBuild.Command = args.ToString(' ');
            customBuild.Outputs.AddUnique(generatedMocSource.Parse());

            var headerFile = new VSSolutionBuilder.V2.VSProjectHeaderFile(source, platform);
            headerFile.Source = source.GeneratedPaths[C.V2.HeaderFile.Key];
            headerFile.CustomBuild = customBuild;
#endif
        }
    }
}
}
namespace VSSolutionBuilder
{
    public partial class VSSolutionBuilder
    {
        public object
        Build(
            QtCommon.MocFile moduleToBuild,
            out System.Boolean success)
        {
            var mocFileModule = moduleToBuild as Bam.Core.BaseModule;
            var node = mocFileModule.OwningNode;
            var target = node.Target;
            var mocFileOptions = mocFileModule.Options;

            var parentNode = node.Parent;
            Bam.Core.DependencyNode targetNode;
            if ((null != parentNode) && (parentNode.Module is Bam.Core.IModuleCollection))
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
                    var solutionType = Bam.Core.State.Get("VSSolutionBuilder", "SolutionType") as System.Type;
                    var SolutionInstance = System.Activator.CreateInstance(solutionType);
                    var ProjectExtensionProperty = solutionType.GetProperty("ProjectExtension");
                    var projectExtension = ProjectExtensionProperty.GetGetMethod().Invoke(SolutionInstance, null) as string;

                    var projectDir = node.GetModuleBuildDirectoryLocation().GetSinglePath();
                    var projectPathName = System.IO.Path.Combine(projectDir, moduleName);
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

                    projectData.Configurations.Add((Bam.Core.BaseTarget)target, configuration);
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
            var toolExePath = tool.Executable((Bam.Core.BaseTarget)target);

            var commandLineBuilder = new Bam.Core.StringArray();
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
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            var customTool = new ProjectTool("VCCustomBuildTool");

            // TODO: collapse these with variables for keys
            if (VisualStudioProcessor.EVisualStudioTarget.VCPROJ == projectData.VSTarget)
            {
                // TODO: is there a macro to use?
                // add output file
                commandLineBuilder.Add(System.String.Format("-o {0}", moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSinglePath()));
                // add source file
                commandLineBuilder.Add(@" $(InputPath)");

                var mocPathName = moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSinglePath();
                var outputPathname = mocPathName;
                var commandLine = System.String.Format("IF NOT EXIST {0} MKDIR {0}{1}{2}", System.IO.Path.GetDirectoryName(mocPathName), System.Environment.NewLine, commandLineBuilder.ToString(' '));
                customTool.AddAttribute("CommandLine", commandLine);

                customTool.AddAttribute("Outputs", outputPathname);

                customTool.AddAttribute("Description", "Moc'ing $(InputPath)...");
                customTool.AddAttribute("AdditionalDependencies", toolExePath);

            }
            else
            {
                // TODO: is there a macro to use?
                // add output file
                commandLineBuilder.Add(System.String.Format("-o {0}", moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSinglePath()));
                // add source file
                commandLineBuilder.Add(@" %(FullPath)");

                var mocPathName = moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSinglePath();
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

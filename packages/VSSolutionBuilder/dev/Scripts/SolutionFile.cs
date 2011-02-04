// <copyright file="SolutionFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>VSSolutionBuilder package</summary>
// <author>Mark Final</author>
namespace VSSolutionBuilder
{
    public sealed class SolutionFile
    {
        public SolutionFile(string pathName)
        {
            this.PathName = pathName;
            this.ProjectDictionary = new System.Collections.Generic.Dictionary<string, ProjectData>();
            this.ProjectConfigurations = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ProjectData>>();
        }

        public string PathName
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, ProjectData> ProjectDictionary
        {
            get;
            private set;
        }

        //TODO: not sure if this is necessary
        public System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ProjectData>> ProjectConfigurations
        {
            get;
            private set;
        }

        public void ResolveSourceFileConfigurationExclusions()
        {
            foreach (System.Collections.Generic.KeyValuePair<string, ProjectData> project in this.ProjectDictionary)
            {
                ProjectData projectData = project.Value;
                foreach (ProjectFile sourceFile in projectData.SourceFiles)
                {
                    foreach (ProjectConfiguration configuration in projectData.Configurations)
                    {
                        lock (sourceFile.FileConfigurations)
                        {
                            if (!sourceFile.FileConfigurations.Contains(configuration.Name))
                            {
                                ProjectTool tool = new ProjectTool("VCCLCompilerTool");
                                ProjectFileConfiguration fileConfiguration = new ProjectFileConfiguration(configuration, tool, true);
                                sourceFile.FileConfigurations.Add(fileConfiguration);
                            }
                        }
                    }
                }
            }
        }

        public void Serialize()
        {
            // serialize each vcproj
            foreach (System.Collections.Generic.KeyValuePair<string, ProjectData> project in this.ProjectDictionary)
            {
                project.Value.Serialize();
            }

            // serialize the sln
            using (System.IO.TextWriter textWriter = new System.IO.StreamWriter(this.PathName))
            {
                textWriter.Write(VisualC.Solution.Header);

                System.Uri solutionLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                // projects
                foreach (System.Collections.Generic.KeyValuePair<string, ProjectData> project in this.ProjectDictionary)
                {
                    System.Uri projectLocationUri = new System.Uri(project.Value.PathName, System.UriKind.RelativeOrAbsolute);
                    System.Uri relativeProjectLocationUri = solutionLocationUri.MakeRelativeUri(projectLocationUri);

                    textWriter.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"",
                                         VisualC.Project.Guid.ToString("B").ToUpper(),
                                         project.Value.Name,
                                         relativeProjectLocationUri.ToString(),
                                         project.Value.Guid.ToString("B").ToUpper());

                    if (project.Value.DependentProjects.Count > 0)
                    {
                        textWriter.WriteLine("\tProjectSection(ProjectDependencies) = postProject");
                        foreach (ProjectData dependentProject in project.Value.DependentProjects)
                        {
                            textWriter.WriteLine("\t\t{0} = {0}", dependentProject.Guid.ToString("B").ToUpper());
                        }
                        textWriter.WriteLine("\tEndProjectSection");
                    }

                    textWriter.WriteLine("EndProject");
                }

                // global sections
                textWriter.WriteLine("Global");
                {
                    // solution configuration (presolution)
                    textWriter.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
                    foreach (System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.List<ProjectData>> configuration in this.ProjectConfigurations)
                    {
                        textWriter.WriteLine("\t\t{0} = {1}", configuration.Key, configuration.Key); // TODO: fixme
                    }
                    textWriter.WriteLine("\tEndGlobalSection");

                    // project configuration platforms (post solution)
                    textWriter.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
                    foreach (System.Collections.Generic.KeyValuePair<string, ProjectData> project in this.ProjectDictionary)
                    {
                        // TODO: fixme
                        ProjectData projectData = project.Value;

                        foreach (ProjectConfiguration configurations in projectData.Configurations)
                        {
                            // TODO: the second .Name should be from the solution configurations
                            textWriter.WriteLine("\t\t{0}.{1}.{2} = {3}", projectData.Guid.ToString("B").ToUpper(), configurations.Name, "ActiveCfg", configurations.Name);
                            textWriter.WriteLine("\t\t{0}.{1}.{2}.{3} = {4}", projectData.Guid.ToString("B").ToUpper(), configurations.Name, "Build", 0, configurations.Name);
                        }
                    }
                    textWriter.WriteLine("\tEndGlobalSection");

                    // solution properties (presolution)
                    textWriter.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
                    textWriter.WriteLine("\t\tHideSolutionNode = FALSE");
                    textWriter.WriteLine("\tEndGlobalSection");
                }
                textWriter.WriteLine("EndGlobal");
            }

            Opus.Core.Log.Info("Solution file written to '{0}'", this.PathName);
        }
    }
}
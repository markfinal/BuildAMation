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
            this.ProjectDictionary = new System.Collections.Generic.Dictionary<string, IProject>();
            this.ProjectConfigurations = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<IProject>>();
        }

        public string PathName
        {
            get;
            private set;
        }

        public System.Collections.Generic.Dictionary<string, IProject> ProjectDictionary
        {
            get;
            private set;
        }

        //TODO: not sure if this is necessary
        public System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<IProject>> ProjectConfigurations
        {
            get;
            private set;
        }

        public void ResolveSourceFileConfigurationExclusions()
        {
            foreach (System.Collections.Generic.KeyValuePair<string, IProject> project in this.ProjectDictionary)
            {
                IProject projectData = project.Value;
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
            foreach (System.Collections.Generic.KeyValuePair<string, IProject> project in this.ProjectDictionary)
            {
                project.Value.Serialize();
            }

            System.Type solutionType = Opus.Core.State.Get("VSSolutionBuilder", "SolutionType") as System.Type;
            object SolutionInstance = System.Activator.CreateInstance(solutionType);

            // serialize the sln
            using (System.IO.TextWriter textWriter = new System.IO.StreamWriter(this.PathName))
            {
                System.Reflection.PropertyInfo HeaderProperty = solutionType.GetProperty("Header");
                textWriter.Write(HeaderProperty.GetGetMethod().Invoke(SolutionInstance, null));

                System.Uri solutionLocationUri = new System.Uri(this.PathName, System.UriKind.RelativeOrAbsolute);

                System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.Guid>> solutionFolders = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.Guid>>();
                System.Collections.Generic.Dictionary<string, System.Guid> solutionFolderGuids = new System.Collections.Generic.Dictionary<string, System.Guid>();

                // projects
                foreach (System.Collections.Generic.KeyValuePair<string, IProject> project in this.ProjectDictionary)
                {
                    IProject p = project.Value;
                    if (null != p.GroupName)
                    {
                        if (!solutionFolders.ContainsKey(p.GroupName))
                        {
                            solutionFolders.Add(p.GroupName, new System.Collections.Generic.List<System.Guid>());
                            solutionFolderGuids.Add(p.GroupName, new DeterministicGuid(p.GroupName).Guid);
                        }

                        solutionFolders[p.GroupName].Add(p.Guid);
                    }

                    System.Uri projectLocationUri = new System.Uri(project.Value.PathName, System.UriKind.RelativeOrAbsolute);
                    System.Uri relativeProjectLocationUri = solutionLocationUri.MakeRelativeUri(projectLocationUri);

                    System.Reflection.PropertyInfo GuidProperty = solutionType.GetProperty("ProjectGuid");
                    System.Guid projectTypeGuid = (System.Guid)GuidProperty.GetGetMethod().Invoke(SolutionInstance, null);
                    textWriter.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"",
                                         projectTypeGuid.ToString("B").ToUpper(),
                                         project.Value.Name,
                                         relativeProjectLocationUri.ToString(),
                                         project.Value.Guid.ToString("B").ToUpper());

                    System.Reflection.PropertyInfo ProjectExtensionProperty = solutionType.GetProperty("ProjectExtension");
                    string projectExtension = ProjectExtensionProperty.GetGetMethod().Invoke(SolutionInstance, null) as string;
                    if (projectExtension.Equals(".vcproj") && (project.Value.DependentProjects.Count > 0))
                    {
                        textWriter.WriteLine("\tProjectSection(ProjectDependencies) = postProject");
                        foreach (IProject dependentProject in project.Value.DependentProjects)
                        {
                            textWriter.WriteLine("\t\t{0} = {0}", dependentProject.Guid.ToString("B").ToUpper());
                        }
                        textWriter.WriteLine("\tEndProjectSection");
                    }

                    textWriter.WriteLine("EndProject");
                }

                System.Reflection.PropertyInfo SolutionFolderGuidProperty = solutionType.GetProperty("SolutionFolderGuid");
                System.Guid solutionFolderTypeGuid = (System.Guid)SolutionFolderGuidProperty.GetGetMethod().Invoke(SolutionInstance, null);

                // solution folders
                if ((solutionFolders.Count > 0) && (0 != solutionFolderTypeGuid.CompareTo(System.Guid.Empty)))
                {
                    foreach (System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.List<System.Guid>> folder in solutionFolders)
                    {
                        textWriter.WriteLine("Project(\"{0}\") = \"{1}\", \"{1}\", \"{2}\"",
                                             solutionFolderTypeGuid.ToString("B").ToUpper(), folder.Key, solutionFolderGuids[folder.Key].ToString("B").ToUpper());
                        textWriter.WriteLine("EndProject");
                    }
                }

                // global sections
                textWriter.WriteLine("Global");
                {
                    // solution configuration (presolution)
                    textWriter.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
                    foreach (System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.List<IProject>> configuration in this.ProjectConfigurations)
                    {
                        textWriter.WriteLine("\t\t{0} = {1}", configuration.Key, configuration.Key); // TODO: fixme
                    }
                    textWriter.WriteLine("\tEndGlobalSection");

                    // project configuration platforms (post solution)
                    textWriter.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
                    foreach (System.Collections.Generic.KeyValuePair<string, IProject> project in this.ProjectDictionary)
                    {
                        // TODO: fixme
                        IProject projectData = project.Value;

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

                    // solution folders
                    if ((solutionFolders.Count > 0) && (0 != solutionFolderTypeGuid.CompareTo(System.Guid.Empty)))
                    {
                        textWriter.WriteLine("\tGlobalSection(NestedProjects) = preSolution");
                        foreach (System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.List<System.Guid>> folder in solutionFolders)
                        {
                            foreach (System.Guid projectGuid in folder.Value)
                            {
                                textWriter.WriteLine("\t\t{0} = {1}", projectGuid.ToString("B").ToUpper(), solutionFolderGuids[folder.Key].ToString("B").ToUpper());
                            }
                        }
                        textWriter.WriteLine("\tEndGlobalSection");
                    }
                }
                textWriter.WriteLine("EndGlobal");
            }

            SolutionInstance = null;

            Opus.Core.Log.Info("Solution file written to '{0}'", this.PathName);
        }
    }
}
#region License
// Copyright (c) 2010-2016, Mark Final
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
using System.Linq;
namespace VSSolutionBuilder
{
    public sealed class VSSolution
    {
        private System.Collections.Generic.Dictionary<System.Type, VSProject> ProjectMap = new System.Collections.Generic.Dictionary<System.Type, VSProject>();
        private System.Collections.Generic.Dictionary<string, VSSolutionFolder> SolutionFolders = new System.Collections.Generic.Dictionary<string, VSSolutionFolder>();

        private void
        AddNestedEntity(
            string parentpath,
            string currentpath,
            VSProject project,
            VSSolutionFolder parent = null)
        {
            var split = currentpath.Split('/');
            var path = split[0];
            var keyPath = string.Join("/", parentpath, path);
            if (!this.SolutionFolders.ContainsKey(keyPath))
            {
                this.SolutionFolders.Add(keyPath, new VSSolutionFolder(keyPath, path));
            }
            var folder = this.SolutionFolders[keyPath];
            if (null != parent)
            {
                parent.NestedEntities.AddUnique(folder);
            }
            if (1 == split.Length)
            {
                folder.NestedEntities.AddUnique(project);
            }
            else
            {
                this.AddNestedEntity(keyPath, string.Join("/", split.Skip(1)), project, folder);
            }
        }

        public VSProject
        EnsureProjectExists(
            Bam.Core.Module module)
        {
            var moduleType = module.GetType();
            lock (this.ProjectMap)
            {
                if (!this.ProjectMap.ContainsKey(moduleType))
                {
                    var project = new VSProject(this, module);
                    this.ProjectMap.Add(moduleType, project);

                    var groups = module.GetType().GetCustomAttributes(typeof(Bam.Core.ModuleGroupAttribute), true);
                    if (groups.Length > 0)
                    {
                        var solutionFolderName = (groups as Bam.Core.ModuleGroupAttribute[])[0].GroupName;
                        lock (this)
                        {
                            this.AddNestedEntity(".", solutionFolderName, project);
                        }
                    }
                }
                if (null == module.MetaData)
                {
                    module.MetaData = this.ProjectMap[moduleType];
                }
                return this.ProjectMap[moduleType];
            }
        }

        public System.Collections.Generic.IEnumerable<VSProject> Projects
        {
            get
            {
                foreach (var project in this.ProjectMap)
                {
                    yield return project.Value;
                }
            }
        }

        public System.Text.StringBuilder
        Serialize(
            string solutionPath)
        {
            var ProjectTypeGuid = System.Guid.Parse("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942");
            var SolutionFolderGuid = System.Guid.Parse("2150E333-8FDC-42A3-9474-1A3956D46DE8");

            var content = new System.Text.StringBuilder();

            var visualCMeta = Bam.Core.Graph.Instance.PackageMetaData<VisualC.MetaData>("VisualC");
            content.AppendFormat(@"Microsoft Visual Studio Solution File, Format Version {0}", visualCMeta.SolutionFormatVersion);
            content.AppendLine();

            var configs = new Bam.Core.StringArray();
            foreach (var project in this.Projects)
            {
                var relativeProjectPath = Bam.Core.RelativePathUtilities.GetPath(project.ProjectPath, solutionPath);
                content.AppendFormat("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"",
                    ProjectTypeGuid.ToString("B").ToUpper(),
                    System.IO.Path.GetFileNameWithoutExtension(project.ProjectPath),
                    relativeProjectPath,
                    project.GuidString);
                content.AppendLine();
                content.AppendLine("EndProject");

                foreach (var config in project.Configurations)
                {
                    configs.AddUnique(config.Value.FullName);
                }
            }
            foreach (var folder in this.SolutionFolders)
            {
                var folderPath = folder.Value.Path;
                var folderGuid = folder.Value.GuidString;
                content.AppendFormat("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"",
                    SolutionFolderGuid.ToString("B").ToUpper(),
                    folderPath,
                    folderPath,
                    folderGuid);
                content.AppendLine();
                content.AppendLine("EndProject");
            }
            content.AppendLine("Global");
            content.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            foreach (var config in configs)
            {
                // TODO: I'm sure these are not meant to be identical, but I don't know what else to put here
                content.AppendFormat("\t\t{0} = {0}", config);
                content.AppendLine();
            }
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var project in this.Projects)
            {
                var guid = project.GuidString;
                var thisProjectConfigs = new Bam.Core.StringArray();

                // write the configurations for which build steps have been defined
                foreach (var config in project.Configurations)
                {
                    var configName = config.Value.FullName;
                    content.AppendFormat("\t\t{0}.{1}.ActiveCfg = {1}", guid, configName);
                    content.AppendLine();
                    content.AppendFormat("\t\t{0}.{1}.Build.0 = {1}", guid, configName);
                    content.AppendLine();
                    thisProjectConfigs.AddUnique(configName);
                }

                // now cater for any configurations that the project does not support
                var unsupportedConfigs = configs.Complement(thisProjectConfigs) as Bam.Core.StringArray;
                foreach (var uConfig in unsupportedConfigs)
                {
                    // a missing "XX.YY.Build.0" line means not configured to build
                    // also, the remapping between config names seems a little arbitrary, but seems to work
                    // might be related to the project not having an ProjectConfiguration for the unsupported config
                    content.AppendFormat("\t\t{0}.{1}.ActiveCfg = {2}", guid, uConfig, thisProjectConfigs[0]);
                    content.AppendLine();
                }
            }
            content.AppendLine("\tEndGlobalSection");
            content.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
            content.AppendLine("\t\tHideSolutionNode = FALSE");
            content.AppendLine("\tEndGlobalSection");
            if (this.SolutionFolders.Count() > 0)
            {
                content.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
                foreach (var folder in this.SolutionFolders)
                {
                    foreach (var nested in folder.Value.NestedEntities)
                    {
                        content.AppendFormat("\t\t{0} = {1}", nested.GuidString, folder.Value.GuidString);
                        content.AppendLine();
                    }
                }
                content.AppendLine("\tEndGlobalSection");
            }
            content.AppendLine("EndGlobal");

            return content;
        }
    }
}

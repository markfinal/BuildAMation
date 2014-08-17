#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
namespace VSSolutionBuilder
{
    public sealed class ProjectConfigurationCollection :
        System.Collections.IEnumerable
    {
        private System.Collections.Generic.List<ProjectConfiguration> list = new System.Collections.Generic.List<ProjectConfiguration>();
        private System.Collections.Generic.Dictionary<Bam.Core.BaseTarget, string> targetToConfig = new System.Collections.Generic.Dictionary<Bam.Core.BaseTarget, string>();

        public System.Collections.IEnumerator
        GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public void
        Add(
            Bam.Core.BaseTarget target,
            ProjectConfiguration configuration)
        {
            this.list.Add(configuration);
            this.AddExistingForTarget(target, configuration);
        }

        public void
        AddExistingForTarget(
            Bam.Core.BaseTarget target,
            ProjectConfiguration configuration)
        {
            if (!this.targetToConfig.ContainsKey(target))
            {
                this.targetToConfig.Add(target, configuration.Name);
            }
        }

        public string
        GetConfigurationNameForTarget(
            Bam.Core.BaseTarget target)
        {
            var configurationName = this.targetToConfig[target];
            return configurationName;
        }

        public bool
        Contains(
            string configurationName)
        {
            foreach (var configuration in this.list)
            {
                if (configurationName == configuration.Name)
                {
                    return true;
                }
            }

            return false;
        }

        public ProjectConfiguration this[string configurationName]
        {
            get
            {
                foreach (var configuration in this.list)
                {
                    if (configurationName == configuration.Name)
                    {
                        return configuration;
                    }
                }

                throw new Bam.Core.Exception("There is no ProjectConfiguration called '{0}'", configurationName);
            }
        }

        public void
        SerializeMSBuild(
            MSBuildProject project,
            System.Uri projectUri)
        {
            // ProjectConfigurations item group
            {
                var configurationsGroup = project.CreateItemGroup();
                configurationsGroup.Label = "ProjectConfigurations";
                foreach (var configuration in this.list)
                {
                    configuration.SerializeMSBuild(configurationsGroup, projectUri);
                }
            }

            // configuration type and character set
            foreach (var configuration in this.list)
            {
                var split = configuration.ConfigurationPlatform();

                var configurationGroup = project.CreatePropertyGroup();
                configurationGroup.Label = "Configuration";
                configurationGroup.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                configurationGroup.CreateProperty("ConfigurationType", configuration.Type.ToString());

                {
                    var solutionType = Bam.Core.State.Get("VSSolutionBuilder", "SolutionType") as System.Type;
                    var SolutionInstance = System.Activator.CreateInstance(solutionType);
                    var PlatformToolsetProperty = solutionType.GetProperty("PlatformToolset");
                    if (null != PlatformToolsetProperty)
                    {
                        configurationGroup.CreateProperty("PlatformToolset", PlatformToolsetProperty.GetGetMethod().Invoke(SolutionInstance, null) as string);
                    }
                }

#if false
                configurationGroup.CreateProperty("CharacterSet", configuration.CharacterSet.ToString());
#endif
            }

            // import property sheets AFTER the configuration types
            project.CreateImport(@"$(VCTargetsPath)\Microsoft.Cpp.props");

            // output and intermediate directories
            foreach (var configuration in this.list)
            {
                var split = configuration.ConfigurationPlatform();

                var dirGroup = project.CreatePropertyGroup();
                dirGroup.CreateProperty("_ProjectFileVersion", "10.0.40219.1"); // TODO, and this means what?

                if (null != configuration.OutputDirectory)
                {
                    var outputDir = Bam.Core.RelativePathUtilities.GetPath(configuration.OutputDirectory, projectUri);
                    // MSBuild complains if the output directory does not end with a trailing slash
                    if (!outputDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                    {
                        outputDir += System.IO.Path.DirectorySeparatorChar;
                    }
                    var outDirProperty = dirGroup.CreateProperty("OutDir", outputDir);
                    outDirProperty.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                }

                if (null != configuration.IntermediateDirectory)
                {
                    var intermediateDir = Bam.Core.RelativePathUtilities.GetPath(configuration.IntermediateDirectory, projectUri);
                    // MSBuild complains if the intermediate directory does not end with a trailing slash
                    if (!intermediateDir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                    {
                        intermediateDir += System.IO.Path.DirectorySeparatorChar;
                    }
                    var intDirProperty = dirGroup.CreateProperty("IntDir", intermediateDir);
                    intDirProperty.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                }

                if (null != configuration.TargetName)
                {
                    var targetNameProperty = dirGroup.CreateProperty("TargetName", configuration.TargetName);
                    targetNameProperty.Condition = System.String.Format("'$(Configuration)|$(Platform)'=='{0}|{1}'", split[0], split[1]);
                }
            }

            // tools
            foreach (var configuration in this.list)
            {
                configuration.Tools.SerializeMSBuild(project, configuration, projectUri);
            }
        }
    }
}

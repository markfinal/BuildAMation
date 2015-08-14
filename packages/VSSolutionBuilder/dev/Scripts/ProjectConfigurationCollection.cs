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

                foreach (var property in configuration.Properties)
                {
                    configurationGroup.CreateProperty(property.Key, property.Value);
                }
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

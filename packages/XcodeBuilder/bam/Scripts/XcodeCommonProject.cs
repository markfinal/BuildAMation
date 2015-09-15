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
namespace XcodeBuilder
{
    public abstract class XcodeCommonProject :
        XcodeMeta
    {
        public XcodeCommonProject(
            Bam.Core.Module module,
            Bam.Core.TokenizedString libraryPath,
            XcodeMeta.Type type) :
            base(module, type)
        {
        }

        protected void
        PullInProjectPreOrPostBuildSteps()
        {
            var postBuildCommands = this.Project.ProjectConfigurations[this.ProjectModule.BuildEnvironment.Configuration].PostBuildCommands;
            if (postBuildCommands.Count > 0)
            {
                this.AddPostBuildCommands(postBuildCommands);
            }
            var preBuildCommands = this.Project.ProjectConfigurations[this.ProjectModule.BuildEnvironment.Configuration].PreBuildCommands;
            if (preBuildCommands.Count > 0)
            {
                this.AddPreBuildCommands(preBuildCommands);
            }
        }

        public void
        AddHeader(
            FileReference header)
        {
            this.Project.HeaderFilesGroup.AddReference(header);
        }

        public void AddSource(Bam.Core.Module module, FileReference source, BuildFile output, Bam.Core.Settings patchSettings)
        {
            if (null != patchSettings)
            {
                var commandLine = new Bam.Core.StringArray();
                (patchSettings as CommandLineProcessor.IConvertToCommandLine).Convert(module, commandLine);
                output.Settings = commandLine;
            }
            this.Target.SourcesBuildPhase.AddBuildFile(output); // this is shared among configurations
            this.Project.SourceFilesGroup.AddReference(source);
            this.Configuration.BuildFiles.Add(output);
        }

        public void SetCommonCompilationOptions(Bam.Core.Module module, Bam.Core.Settings settings)
        {
            this.Target.SetCommonCompilationOptions(module, this.Configuration, settings);
        }

        public void
        AddPreBuildCommands(
            Bam.Core.StringArray commands)
        {
            if (null == this.Target.PreBuildBuildPhase)
            {
                var preBuildBuildPhase = new ShellScriptBuildPhase(this.Target, "Pre Build", (target) =>
                    {
                        var content = new System.Text.StringBuilder();
                        foreach (var config in target.ConfigurationList)
                        {
                            content.AppendFormat("if [ \\\"$CONFIGURATION\\\" = \\\"{0}\\\" ]; then\\n\\n", config.Name);
                            foreach (var line in config.PreBuildCommands)
                            {
                                content.AppendFormat("  {0}\\n", line);
                            }
                            content.AppendFormat("fi\\n\\n");
                        }
                        return content.ToString();
                    });
                this.Project.ShellScriptsBuildPhases.Add(preBuildBuildPhase);
                this.Target.PreBuildBuildPhase = preBuildBuildPhase;
                // do not add PreBuildBuildPhase to this.Target.BuildPhases, so that it can be serialized in the right order
            }

            this.Configuration.PreBuildCommands.AddRange(commands);
        }

        public void
        AddPostBuildCommands(
            Bam.Core.StringArray commands)
        {
            if (null == this.Target.PostBuildBuildPhase)
            {
                var postBuildBuildPhase = new ShellScriptBuildPhase(this.Target, "Post Build", (target) =>
                    {
                        var content = new System.Text.StringBuilder();
                        foreach (var config in target.ConfigurationList)
                        {
                            content.AppendFormat("if [ \\\"$CONFIGURATION\\\" = \\\"{0}\\\" ]; then\\n\\n", config.Name);
                            foreach (var line in config.PostBuildCommands)
                            {
                                content.AppendFormat("  {0}\\n", line);
                            }
                            content.AppendFormat("fi\\n\\n");
                        }
                        return content.ToString();
                    });
                this.Project.ShellScriptsBuildPhases.Add(postBuildBuildPhase);
                this.Target.PostBuildBuildPhase = postBuildBuildPhase;
                // do not add PostBuildBuildPhase to this.Target.BuildPhases, so that it can be serialized in the right order
            }

            this.Configuration.PostBuildCommands.AddRange(commands);
        }

        public FileReference Output
        {
            get;
            protected set;
        }

        public void
        RequiresProject(
            XcodeCommonProject dependent)
        {
            var proxy = new ContainerItemProxy(this.Project, dependent.Target);
            var dependency = new TargetDependency(dependent.Target, proxy);
            this.Target.TargetDependencies.AddUnique(dependency);
        }
    }
}

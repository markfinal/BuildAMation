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
    public sealed class XcodeMocGeneration :
        IMocGenerationPolicy
    {
        void
        IMocGenerationPolicy.Moc(
            MocModule sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.Tool mocCompiler,
            Bam.Core.V2.TokenizedString generatedMocSource,
            C.V2.HeaderFile source)
        {
            var output = generatedMocSource.Parse();

            var commands = new Bam.Core.StringArray();
            commands.Add(System.String.Format("[[ ! -d {0} ]] && mkdir -p {0}", System.IO.Path.GetDirectoryName(output)));

            var mocInvoke = new System.Text.StringBuilder();
            mocInvoke.AppendFormat("{0} -o{1} {2}", mocCompiler.Executable.Parse(), output, source.InputPath.Parse());
            commands.Add(mocInvoke.ToString());

            var header = new XcodeBuilder.V2.XcodeHeaderFile(sender);
            header.Project.ProjectConfigurations[sender.BuildEnvironment.Configuration].PreBuildCommands.AddRange(commands);
        }
    }
}
}
namespace XcodeBuilder
{
    public partial class XcodeBuilder
    {
        public object
        Build(
            QtCommon.MocFile moduleToBuild,
            out System.Boolean success)
        {
            var node = moduleToBuild.OwningNode;

            var parentNode = node.Parent;
            Bam.Core.DependencyNode targetNode;
            if ((parentNode != null) && (parentNode.Module is Bam.Core.IModuleCollection))
            {
                targetNode = parentNode.ExternalDependentFor[0];
            }
            else
            {
                targetNode = node.ExternalDependentFor[0];
            }

            var project = this.Workspace.GetProject(targetNode);
            var baseTarget = (Bam.Core.BaseTarget)targetNode.Target;
            var configuration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), targetNode.ModuleName);

            if (null != parentNode)
            {
                Bam.Core.BaseOptionCollection complementOptionCollection = null;
                if (node.EncapsulatingNode.Module is Bam.Core.ICommonOptionCollection)
                {
                    var commonOptions = (node.EncapsulatingNode.Module as Bam.Core.ICommonOptionCollection).CommonOptionCollection;
                    if (commonOptions is QtCommon.MocOptionCollection)
                    {
                        complementOptionCollection = moduleToBuild.Options.Complement(commonOptions);
                    }
                }

                if ((complementOptionCollection != null) && (complementOptionCollection.OptionNames.Count > 0))
                {
                    // use a custom moc
                    var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOCing files for " + node.UniqueModuleName, node.ModuleName);

                    MocShellScriptHelper.WriteShellCommand(node.Target, moduleToBuild.Options, shellScriptBuildPhase, configuration);

                    shellScriptBuildPhase.InputPaths.Add(moduleToBuild.SourceFileLocation.GetSingleRawPath());
                    shellScriptBuildPhase.OutputPaths.Add(moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSingleRawPath());
                }
                else
                {
                    var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOCing files for " + parentNode.ModuleName, parentNode.ModuleName);
                    shellScriptBuildPhase.InputPaths.Add(moduleToBuild.SourceFileLocation.GetSingleRawPath());
                    shellScriptBuildPhase.OutputPaths.Add(moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSingleRawPath());
                }
            }
            else
            {
                var shellScriptBuildPhase = project.ShellScriptBuildPhases.Get("MOCing files for " + node.UniqueModuleName, node.ModuleName);

                MocShellScriptHelper.WriteShellCommand(node.Target, moduleToBuild.Options, shellScriptBuildPhase, configuration);

                shellScriptBuildPhase.InputPaths.Add(moduleToBuild.SourceFileLocation.GetSingleRawPath());
                shellScriptBuildPhase.OutputPaths.Add(moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSingleRawPath());
            }

            success = true;
            return null;
        }
    }
}

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
namespace Publisher
{
    public sealed class XcodeIdNameOSX :
        IInstallNameToolPolicy
    {
        void
        IInstallNameToolPolicy.InstallName(
            InstallNameModule sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString oldName,
            Bam.Core.TokenizedString newName)
        {
            var originalModule = sender.Source.SourceModule;
            if (null == originalModule)
            {
                return;
            }

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(sender, commandLine);

            var commands = new Bam.Core.StringArray();

            if (sender.Source.SourceModule != null && sender.Source.SourceModule.MetaData != null)
            {
                commands.Add(System.String.Format("{0} {1} {2} {3} {4}",
                    CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                    commandLine.ToString(' '),
                    oldName.Parse(),
                    newName.Parse(),
                    sender.Source.GeneratedPaths[CollatedObject.CopiedObjectKey].Parse()));

                var target = sender.Source.SourceModule.MetaData as XcodeBuilder.Target;
                var configuration = target.GetConfiguration(sender.Source.SourceModule);
                target.AddPostBuildCommands(commands, configuration);
            }
            else
            {
                var destinationFolder = "$CONFIGURATION_BUILD_DIR";
                if (sender.Source.Reference != null)
                {
                    destinationFolder = "$CONFIGURATION_BUILD_DIR/$EXECUTABLE_FOLDER_PATH";
                }

                commands.Add(System.String.Format("{0} {1} {2} {3}/{4}",
                    CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                    commandLine.ToString(' '),
                    newName.Parse(),
                    destinationFolder,
                    sender.Source.CreateTokenizedString("$(0)/@filename($(1))", sender.Source.SubDirectory, sender.Source.SourcePath).Parse()));

                var target = sender.Source.Reference.SourceModule.MetaData as XcodeBuilder.Target;
                var configuration = target.GetConfiguration(sender.Source.Reference.SourceModule);
                target.AddPostBuildCommands(commands, configuration);
            }
        }
    }
}

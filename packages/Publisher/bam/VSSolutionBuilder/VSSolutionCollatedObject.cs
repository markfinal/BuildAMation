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
namespace Publisher
{
    public sealed class VSSolutionCollatedObject :
        ICollatedObjectPolicy
    {
        void
        ICollatedObjectPolicy.Collate(
            CollatedObject sender,
            Bam.Core.ExecutionContext context)
        {
            var sourcePath = sender.SourcePath;
            if (null == sender.Reference)
            {
                // the main file is not copied anywhere, as we copy required files around it where VS wrote the main file
                // this is managed by the Collation class, querying the build mode for where publishing is relative to
                // ignore any subdirectory on this module
                return;
            }

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            if (sender.SourceModule != null && sender.SourceModule.MetaData != null)
            {
                var destinationPath = sender.Macros["CopyDir"].Parse();

                var commands = new Bam.Core.StringArray();
                commands.Add(System.String.Format("IF NOT EXIST {0} MKDIR {0}", destinationPath));
                commands.Add(System.String.Format(@"{0} {1} $(OutputPath)$(TargetFileName) {2} {3}",
                    CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                    commandLine.ToString(' '),
                    destinationPath,
                    CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));

                var project = sender.SourceModule.MetaData as VSSolutionBuilder.VSProject;
                var config = project.GetConfiguration(sender.SourceModule);
                config.AddPostBuildCommands(commands);
            }
            else
            {
                var commands = new Bam.Core.StringArray();
                if (sender is CollatedDirectory)
                {
                    // Windows XCOPY requires the directory name to be added to the destination, while Posix cp does not
                    commands.Add(System.String.Format(@"{0} {1} {2} $(OutDir){3}\ {4}",
                        CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                        commandLine.ToString(' '),
                        sourcePath.ParseAndQuoteIfNecessary(),
                        sender.CreateTokenizedString("$(0)/@ifnotempty($(CopiedFilename),$(CopiedFilename),@filename($(1)))", sender.SubDirectory, sourcePath).Parse(),
                        CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
                }
                else
                {
                    commands.Add(System.String.Format(@"{0} {1} {2} $(OutDir){3}\ {4}",
                        CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                        commandLine.ToString(' '),
                        sourcePath.ParseAndQuoteIfNecessary(),
                        sender.SubDirectory,
                        CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
                }

                var project = sender.Reference.SourceModule.MetaData as VSSolutionBuilder.VSProject;
                var config = project.GetConfiguration(sender.Reference.SourceModule);
                config.AddPostBuildCommands(commands);
            }
        }
    }
}

#region License
// Copyright (c) 2010-2017, Mark Final
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
            if (sender.Ignore)
            {
                return;
            }

            var collatedInterface = sender as ICollatedObject;
            if (sender.IsAnchor && null != collatedInterface.SourceModule)
            {
                // since all dependents are copied _beside_ their anchor, the anchor copy is a no-op
                return;
            }

            var copySourcePath = sender.SourcePath;

            // post-fix with a directory separator to enforce that this is a directory destination
            var destinationDir = System.String.Format("{0}{1}",
                collatedInterface.PublishingDirectory.ToString(),
                System.IO.Path.DirectorySeparatorChar);

            if (null == sender.PreExistingSourcePath)
            {
                Bam.Core.Log.MessageAll("** {0}[{1}]:\t'{2}' -> '{3}'",
                    collatedInterface.SourceModule.ToString(),
                    collatedInterface.SourcePathKey.ToString(),
                    copySourcePath.ToString(),
                    destinationDir);
            }
            else
            {
                Bam.Core.Log.MessageAll("** {0}: '{1}' -> '{2}'",
                    sender,
                    copySourcePath.ToString(),
                    destinationDir);
            }

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            var commands = new Bam.Core.StringArray();
            commands.Add(System.String.Format("IF NOT EXIST {0} MKDIR {0}", destinationDir));

            var arePostBuildCommands = true;
            Bam.Core.Module sourceModule;
            if (null != collatedInterface.SourceModule)
            {
                sourceModule = collatedInterface.SourceModule;

                // check for runtime dependencies that won't have projects, use their anchor
                if (null == sourceModule.MetaData)
                {
                    sourceModule = collatedInterface.Anchor.SourceModule;
                }
            }
            else
            {
                if (null != collatedInterface.Anchor)
                {
                    // usually preexisting files that are published as part of an executable's distribution
                    // in which case, their anchor is the executable (or a similar binary)
                    sourceModule = collatedInterface.Anchor.SourceModule;
                }
                else
                {
                    if (sender is CollatedPreExistingFile)
                    {
                        sourceModule = (sender as CollatedPreExistingFile).ParentOfCollationModule;

                        // ensure a project exists, as this collation may be visited prior to
                        // the source which invoked it
                        var solution = Bam.Core.Graph.Instance.MetaData as VSSolutionBuilder.VSSolution;
                        solution.EnsureProjectExists(sourceModule);

                        arePostBuildCommands = false;
                    }
                    else
                    {
                        throw new Bam.Core.Exception("No anchor set on '{0}' with source path '{1}'", sender.GetType().ToString(), sender.SourcePath);
                    }
                }
            }

            var project = sourceModule.MetaData as VSSolutionBuilder.VSProject;
            var config = project.GetConfiguration(sourceModule);

            commands.Add(System.String.Format(@"{0} {1} {2} {3} {4}",
                CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                commandLine.ToString(' '),
                copySourcePath.ToStringQuoteIfNecessary(),
                destinationDir,
                CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
            if (config.Type != VSSolutionBuilder.VSProjectConfiguration.EType.Utility && arePostBuildCommands)
            {
                config.AddPostBuildCommands(commands);
            }
            else
            {
                config.AddPreBuildCommands(commands);
            }
        }
    }
}

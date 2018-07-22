#region License
// Copyright (c) 2010-2018, Mark Final
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
#if BAM_V2
    public static partial class NativeSupport
    {
        public static void
        CollateObject(
            CollatedObject module,
            Bam.Core.ExecutionContext context)
        {
            if (module.Ignore)
            {
                return;
            }

            foreach (var dir in module.OutputDirectories)
            {
                Bam.Core.IOWrapper.CreateDirectoryIfNotExists(dir.ToString());
            }

            CommandLineProcessor.Processor.Execute(
                context,
                module.Tool as Bam.Core.ICommandLineTool,
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                )
            );
        }
    }
#else
    public sealed class NativeCollatedObject :
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

            var copyFileTool = sender.Tool as CopyFileTool;
            string copySourcePath;
            string destinationDir;
            copyFileTool.convertPaths(
                sender,
                sender.SourcePath,
                collatedInterface.PublishingDirectory,
                out copySourcePath,
                out destinationDir);

            if (null == sender.PreExistingSourcePath)
            {
                Bam.Core.Log.DebugMessage("** {0}[{1}]:\t'{2}' -> '{3}'",
                    collatedInterface.SourceModule.ToString(),
                    collatedInterface.SourcePathKey.ToString(),
                    copySourcePath,
                    destinationDir);
            }
            else
            {
                Bam.Core.Log.DebugMessage("** {0}: '{1}' -> '{2}'",
                    sender,
                    copySourcePath,
                    destinationDir);
            }

            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(destinationDir);

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);
            commandLine.Add(copyFileTool.escapePath(copySourcePath));
            commandLine.Add(copyFileTool.escapePath(destinationDir));
            CommandLineProcessor.Processor.Execute(context, sender.Tool as Bam.Core.ICommandLineTool, commandLine);
        }
    }
#endif
        }

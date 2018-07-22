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
    public static partial class MakeFileSupport
    {
        public static void
        CollateObject(
            CollatedObject module)
        {
            if (module.Ignore)
            {
                return;
            }

            var meta = new MakeFileBuilder.MakeFileMeta(module);

            foreach (var dir in module.OutputDirectories)
            {
                meta.CommonMetaData.AddDirectory(dir.ToString());
            }

            var rule = meta.AddRule();

            var variableName = new System.Text.StringBuilder();
            variableName.Append((module as ICollatedObject).SourceModule.GetType().Name);

            if (module is CollatedFile)
            {
                rule.AddTarget(
                    module.GeneratedPaths[CollatedObject.CopiedFileKey],
                    variableName: "CollatedFile" + variableName
                );
            }
            else if (module is CollatedDirectory)
            {
                rule.AddTarget(
                    module.GeneratedPaths[CollatedObject.CopiedDirectoryKey],
                    variableName: "CollatedDir" + variableName
                );
            }
            else
            {
                throw new Bam.Core.Exception(
                    "Unsupported module type {0} for MakeFile collation",
                    module.GetType().ToString()
                );
            }
            rule.AddPrerequisite((module as ICollatedObject).SourceModule.GeneratedPaths[(module as ICollatedObject).SourcePathKey]);

            rule.AddShellCommand(System.String.Format("{0} {1} {2}",
                CommandLineProcessor.Processor.StringifyTool(module.Tool as Bam.Core.ICommandLineTool),
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                ).ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(module.Tool as Bam.Core.ICommandLineTool))
            );
        }
    }
#else
    public sealed class MakeFileCollatedObject :
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

            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();

            var topLevel = sender.GetEncapsulatingReferencedModule().GetType().Name;
            var senderType = sender.GetType().Name;
            var sourceType = (null != collatedInterface.SourceModule) ? collatedInterface.SourceModule.GetType().FullName : "publishroot";
            var basename = sourceType + "_" + topLevel + "_" + senderType + "_" + sender.BuildEnvironment.Configuration.ToString() + "_";
            var sourceFilename = System.IO.Path.GetFileName(sender.SourcePath.ToString());
            var isPosixLeafRename = copySourcePath.EndsWith("*");

            string prerequisitePath;
            var destinationPath = sender.GeneratedPaths[CollatedObject.Key];

            if (isPosixLeafRename)
            {
                sourceFilename = System.String.Format("{0}-to-{1}", sourceFilename, sender.Macros["RenameLeaf"].ToString());
                prerequisitePath = System.IO.Path.GetDirectoryName(copySourcePath);
                // there would be multiple commands for the target directory if this was
                // added to meta.CommonMetaData
                rule.AddShellCommand("mkdir -p $@");
            }
            else
            {
                prerequisitePath = copySourcePath;
                meta.CommonMetaData.AddDirectory(destinationDir);
            }
            rule.AddTarget(destinationPath, variableName: basename + sourceFilename);

            var commandLine = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            if (isPosixLeafRename)
            {
                rule.AddShellCommand(System.String.Format(@"{0} {1} $</* $@ {2}",
                    CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                    commandLine.ToString(' '),
                    CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
            }
            else
            {
                if (MakeFileBuilder.MakeFileCommonMetaData.IsNMAKE)
                {
                    // $< didn't work in NMAKE here
                    rule.AddShellCommand(System.String.Format(@"{0} {1} $** $(@D) {2}",
                        CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                        commandLine.ToString(' '),
                        CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
                }
                else
                {
                    rule.AddShellCommand(System.String.Format(@"{0} {1} $< $(dir $@) {2}",
                        CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool),
                        commandLine.ToString(' '),
                        CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool)));
                }
            }
            rule.AddPrerequisite(Bam.Core.TokenizedString.CreateVerbatim(prerequisitePath));
        }
    }
#endif
}

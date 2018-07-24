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
    public static partial class XcodeSupport
    {
        public static void
        Zip(
            ZipModule module)
        {
            var encapsulating = module.GetEncapsulatingReferencedModule();

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(encapsulating);
            target.EnsureOutputFileReferenceExists(
                module.GeneratedPaths[ZipModule.ZipKey],
                XcodeBuilder.FileReference.EFileType.ZipArchive,
                XcodeBuilder.Target.EProductType.Utility);
            var configuration = target.GetConfiguration(encapsulating);
            configuration.SetProductName(Bam.Core.TokenizedString.CreateVerbatim("PythonZip"));

            var commands = new Bam.Core.StringArray();
            foreach (var dir in module.OutputDirectories)
            {
                commands.Add(
                    System.String.Format(
                        "[[ ! -d {0} ]] && mkdir -p {0}",
                        dir.ToStringQuoteIfNecessary()
                    )
                );
            }

            var args = new Bam.Core.StringArray();
            if (module.WorkingDirectory != null)
            {
                args.Add(
                    System.String.Format(
                        "cd {0} &&",
                        Bam.Core.IOWrapper.EscapeSpacesInPath(module.WorkingDirectory.ToString())
                    )
                );
            }
            args.Add(CommandLineProcessor.Processor.StringifyTool(module.Tool as Bam.Core.ICommandLineTool));
            args.AddRange(
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                )
            );
            args.Add(CommandLineProcessor.Processor.TerminatingArgs(module.Tool as Bam.Core.ICommandLineTool));
            args.Add("|| true"); // because zip returns 12 (nothing to do) upon success
            commands.Add(args.ToString(' '));

            target.AddPreBuildCommands(commands, configuration);
        }
    }
#else
    public sealed class XcodeZip :
        IZipToolPolicy
    {
        void
        IZipToolPolicy.Zip(
            ZipModule sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString zipOutputPath,
            Bam.Core.TokenizedString zipInputPath)
        {
            var encapsulating = sender.GetEncapsulatingReferencedModule();

            var workspace = Bam.Core.Graph.Instance.MetaData as XcodeBuilder.WorkspaceMeta;
            var target = workspace.EnsureTargetExists(encapsulating);
            target.EnsureOutputFileReferenceExists(
                zipOutputPath,
                XcodeBuilder.FileReference.EFileType.ZipArchive,
                XcodeBuilder.Target.EProductType.Utility);
            var configuration = target.GetConfiguration(encapsulating);
            configuration.SetProductName(Bam.Core.TokenizedString.CreateVerbatim("PythonZip"));

            var commands = new Bam.Core.StringArray();
            var args = new Bam.Core.StringArray();
            args.Add(System.String.Format("cd {0} &&", Bam.Core.IOWrapper.EscapeSpacesInPath(zipInputPath.ToString())));
            args.Add(CommandLineProcessor.Processor.StringifyTool(sender.Tool as Bam.Core.ICommandLineTool));
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(args);
            args.Add(System.String.Format("{0}", Bam.Core.IOWrapper.EscapeSpacesInPath(zipOutputPath.ToString())));
            args.Add("*");
            args.Add("|| true"); // because zip returns 12 (nothing to do) upon success
            args.Add(CommandLineProcessor.Processor.TerminatingArgs(sender.Tool as Bam.Core.ICommandLineTool));
            commands.Add(args.ToString(' '));

            target.AddPreBuildCommands(commands, configuration);
        }
    }
#endif
}

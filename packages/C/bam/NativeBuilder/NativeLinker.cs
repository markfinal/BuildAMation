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
namespace C
{
#if BAM_V2
    public static partial class NativeSupport
    {
        public static void
        Link(
            ConsoleApplication module,
            Bam.Core.ExecutionContext context)
        {
            // any libraries added prior to here, need to be moved to the end
            // they are external dependencies, and thus all built modules (to be added now) may have
            // a dependency on them (and not vice versa)
            var linker = module.Settings as C.ICommonLinkerSettings;
            var externalLibs = linker.Libraries;
            linker.Libraries = new Bam.Core.StringArray();
            foreach (var library in module.Libraries)
            {
                (module.Tool as C.LinkerTool).ProcessLibraryDependency(module as CModule, library as CModule);
            }
            linker.Libraries.AddRange(externalLibs);

            if (module.Settings is ICommonHasOutputPath)
            {
                var output_path = module.GeneratedPaths[(module.Settings as ICommonHasOutputPath).OutputPath].ToString();
                var output_dir = System.IO.Path.GetDirectoryName(output_path);
                Bam.Core.IOWrapper.CreateDirectoryIfNotExists(output_dir);
            }

            var commandLine = new Bam.Core.StringArray();

            // first object files
            foreach (var input in module.ObjectFiles)
            {
                if (!(input as C.ObjectFileBase).PerformCompilation)
                {
                    continue;
                }
                commandLine.Add(input.GeneratedPaths[C.ObjectFile.Key].ToStringQuoteIfNecessary());
            }

            // then all options
            commandLine.AddRange(
                CommandLineProcessor.NativeConversion.Convert(
                    module.Settings,
                    module
                )
            );

            CommandLineProcessor.Processor.Execute(
                context,
                module.Tool as Bam.Core.ICommandLineTool,
                commandLine);
        }
    }
#else
    public sealed class NativeLinker :
        ILinkingPolicy
    {
        void
        ILinkingPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> libraries)
        {
            // any libraries added prior to here, need to be moved to the end
            // they are external dependencies, and thus all built modules (to be added now) may have
            // a dependency on them (and not vice versa)
            var linker = sender.Settings as C.ICommonLinkerSettings;
            var externalLibs = linker.Libraries;
            linker.Libraries = new Bam.Core.StringArray();
            foreach (var library in libraries)
            {
                (sender.Tool as C.LinkerTool).ProcessLibraryDependency(sender as CModule, library as CModule);
            }
            linker.Libraries.AddRange(externalLibs);

            var executableDir = System.IO.Path.GetDirectoryName(executablePath.ToString());
            Bam.Core.IOWrapper.CreateDirectoryIfNotExists(executableDir);

            var commandLine = new Bam.Core.StringArray();

            // first object files
            foreach (var input in objectFiles)
            {
                if (!(input as C.ObjectFileBase).PerformCompilation)
                {
                    continue;
                }
                commandLine.Add(input.GeneratedPaths[C.ObjectFile.Key].ToStringQuoteIfNecessary());
            }

            // then all options
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            CommandLineProcessor.Processor.Execute(context, sender.Tool as Bam.Core.ICommandLineTool, commandLine);
        }
    }
#endif
}

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
namespace C
{
    public sealed class NativeLinker :
        ILinkingPolicy
    {
        private static bool
        DeferredEvaluationRequiresBuild(
            ConsoleApplication sender,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> libraries)
        {
            var exeWriteTime = System.IO.File.GetLastWriteTime(sender.GeneratedPaths[C.ConsoleApplication.Key].Parse());
            foreach (var library in libraries)
            {
                if ((library.ReasonToExecute != null) && (library.ReasonToExecute.Reason == Bam.Core.ExecuteReasoning.EReason.DeferredEvaluation))
                {
                    // Note: on Windows, this is checking the IMPORT library path, so if the API of the library has not changed
                    // then it's likely this library is older than the DLL, thus not re-linking this binary, which is the correc thing to do
                    var libraryFileWriteTime = System.IO.File.GetLastWriteTime((sender.Tool as C.LinkerTool).GetLibraryPath(library as CModule).Parse());
                    if (libraryFileWriteTime > exeWriteTime)
                    {
                        return true;
                    }
                }
            }
            foreach (var input in objectFiles)
            {
                if ((input.ReasonToExecute != null) && (input.ReasonToExecute.Reason == Bam.Core.ExecuteReasoning.EReason.DeferredEvaluation))
                {
                    var objectFileWriteTime = System.IO.File.GetLastWriteTime(input.GeneratedPaths[C.ObjectFile.Key].Parse());
                    if (objectFileWriteTime > exeWriteTime)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void
        ILinkingPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> libraries)
        {
            if (sender.ReasonToExecute.Reason == Bam.Core.ExecuteReasoning.EReason.DeferredEvaluation)
            {
                if (!DeferredEvaluationRequiresBuild(sender, objectFiles, libraries))
                {
                    return;
                }
            }

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
            if (!System.IO.Directory.Exists(executableDir))
            {
                System.IO.Directory.CreateDirectory(executableDir);
            }

            var commandLine = new Bam.Core.StringArray();

            // first object files
            foreach (var input in objectFiles)
            {
                if (!(input as C.ObjectFile).PerformCompilation)
                {
                    continue;
                }
                commandLine.Add(input.GeneratedPaths[C.ObjectFile.Key].ToString());
            }

            // then all options
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLine);

            CommandLineProcessor.Processor.Execute(context, sender.Tool as Bam.Core.ICommandLineTool, commandLine);
        }
    }
}

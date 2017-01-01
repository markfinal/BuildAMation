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
using Bam.Core;
namespace C
{
    public sealed class MakeFileLinker :
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

            var commandLineArgs = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(commandLineArgs);

            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(executablePath);
            string objExt = null; // try to get the object file extension from a compiled source file
            foreach (var module in objectFiles)
            {
                if (null == objExt)
                {
                    objExt = module.Tool.Macros["objext"].Parse();
                }
                if (!(module as C.ObjectFile).PerformCompilation)
                {
                    continue;
                }
                rule.AddPrerequisite(module, C.ObjectFile.Key);
            }
            foreach (var module in libraries)
            {
                if (module is StaticLibrary)
                {
                    rule.AddPrerequisite(module, C.StaticLibrary.Key);
                }
                else if (module is IDynamicLibrary)
                {
                    if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                    {
                        rule.AddPrerequisite(module, C.DynamicLibrary.ImportLibraryKey);
                    }
                    else
                    {
                        rule.AddPrerequisite(module, C.DynamicLibrary.Key);
                    }
                }
                else if (module is CSDKModule)
                {
                    continue;
                }
                else if (module is OSXFramework)
                {
                    continue;
                }
                else
                {
                    throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
                }
            }

            var tool = sender.Tool as Bam.Core.ICommandLineTool;
            var commands = new System.Text.StringBuilder();
            // if there were no object files, you probably intended to use all prerequisites anyway
            var filter = (null != objExt) ? System.String.Format("$(filter %{0},$^)", objExt) : "$^";
            commands.AppendFormat("{0} {1} {2} {3}",
                CommandLineProcessor.Processor.StringifyTool(tool),
                filter,
                commandLineArgs.ToString(' '),
                CommandLineProcessor.Processor.TerminatingArgs(tool));
            rule.AddShellCommand(commands.ToString());

            var executableDir = System.IO.Path.GetDirectoryName(executablePath.ToString());
            meta.CommonMetaData.Directories.AddUnique(executableDir);
            meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);
        }
    }
}

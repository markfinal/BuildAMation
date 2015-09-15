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
using Bam.Core;
namespace C
{
    public sealed class MakeFileLinker :
        ILinkingPolicy
    {
        private static string
        GetLibraryPath(Bam.Core.Module module)
        {
            if (module is C.StaticLibrary)
            {
                return module.GeneratedPaths[C.StaticLibrary.Key].ToString();
            }
            else if (module is C.DynamicLibrary)
            {
                if (module.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows))
                {
                    return module.GeneratedPaths[C.DynamicLibrary.ImportLibraryKey].ToString();
                }
                else
                {
                    return module.GeneratedPaths[C.DynamicLibrary.Key].ToString();
                }
            }
            else if (module is C.CSDKModule)
            {
                // collection of libraries, none in particular
                return null;
            }
            else if (module is C.HeaderLibrary)
            {
                // no library
                return null;
            }
            else if (module is ExternalFramework)
            {
                // dealt with elsewhere
                return null;
            }
            else
            {
                throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
            }
        }

        void
        ILinkingPolicy.Link(
            ConsoleApplication sender,
            Bam.Core.ExecutionContext context,
            Bam.Core.TokenizedString executablePath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> objectFiles,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> headers,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> libraries,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.Module> frameworks)
        {
            // TODO: modify to use ProcessLibraryDependency
            var linker = sender.Settings as C.ICommonLinkerSettings;
            // TODO: could the lib search paths be in the staticlibrary base class as a patch?
            foreach (var library in libraries)
            {
                var fullLibraryPath = GetLibraryPath(library);
                if (null == fullLibraryPath)
                {
                    continue;
                }
                var dir = System.IO.Path.GetDirectoryName(fullLibraryPath);
                linker.LibraryPaths.AddUnique(Bam.Core.TokenizedString.Create(dir, null));
            }

            var commandLineArgs = new Bam.Core.StringArray();
            (sender.Settings as CommandLineProcessor.IConvertToCommandLine).Convert(sender, commandLineArgs);

            var meta = new MakeFileBuilder.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(executablePath);
            foreach (var module in objectFiles)
            {
                rule.AddPrerequisite(module, C.ObjectFile.Key);
            }
            foreach (var module in libraries)
            {
                if (module is StaticLibrary)
                {
                    rule.AddPrerequisite(module, C.StaticLibrary.Key);
                }
                else if (module is DynamicLibrary)
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
                else if (module is ExternalFramework)
                {
                    continue;
                }
                else
                {
                    throw new Bam.Core.Exception("Unknown module library type: {0}", module.GetType());
                }
            }

            var tool = sender.Tool as Bam.Core.PreBuiltTool;
            var commands = new System.Text.StringBuilder();
            commands.AppendFormat(tool.Executable.ContainsSpace ? "\"{0}\" $^ {1}" : "{0} $^ {1}", tool.Executable, commandLineArgs.ToString(' '));
            rule.AddShellCommand(commands.ToString());

            var executableDir = System.IO.Path.GetDirectoryName(executablePath.ToString());
            meta.CommonMetaData.Directories.AddUnique(executableDir);
            meta.CommonMetaData.ExtendEnvironmentVariables(tool.EnvironmentVariables);
        }
    }
}

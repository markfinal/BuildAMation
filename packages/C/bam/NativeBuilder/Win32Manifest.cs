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
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            C.Win32Manifest moduleToBuild,
            out bool success)
        {
            var binaryLoc = moduleToBuild.BinaryFileLocation;
            var binaryLocPath = binaryLoc.GetSinglePath();
            if (!System.IO.File.Exists(binaryLocPath))
            {
                throw new Bam.Core.Exception("Binary file '{0}' does not exist", binaryLocPath);
            }

            var asModule = moduleToBuild as Bam.Core.BaseModule;
            var options = asModule.Options;

            var compilerOptions = options as C.Win32ManifestOptionCollection;

            // don't do dependency checking, as it's an in-place operation
            // TODO: should really do dependency checking on whether the manifest is newer than the executable

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var target = asModule.OwningNode.Target;

            var commandLineBuilder = new Bam.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            var compilerTool = target.Toolset.Tool(typeof(C.IWinManifestTool)) as C.IWinManifestTool;

            // add input path
            commandLineBuilder.Add(System.String.Format("-manifest {0}.manifest", binaryLocPath));
            commandLineBuilder.Add("-nologo");
            // add output path
            var binaryModule = moduleToBuild.OwningNode.ExternalDependents[0].Module;
            if (binaryModule is C.DynamicLibrary)
            {
                commandLineBuilder.Add(System.String.Format("-outputresource:{0};2", binaryLocPath));
            }
            else
            {
                commandLineBuilder.Add(System.String.Format("-outputresource:{0};1", binaryLocPath));
            }

            var exitCode = CommandLineProcessor.Processor.Execute(asModule.OwningNode, compilerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}

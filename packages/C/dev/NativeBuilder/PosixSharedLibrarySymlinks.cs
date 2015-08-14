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
        private static bool
        MakeSymlink(
            Bam.Core.StringArray commandLineBuilder,
            C.PosixSharedLibrarySymlinks moduleToBuild,
            C.IPosixSharedLibrarySymlinksTool symlinkTool,
            string workingDir,
            Bam.Core.LocationKey keyToSymlink)
        {
            var symlinkCommandLineBuilder = new Bam.Core.StringArray(commandLineBuilder);

            var majorSymlinkFile = moduleToBuild.Locations[keyToSymlink];
            var majorSymlinkFileLeafname = System.IO.Path.GetFileName(majorSymlinkFile.GetSingleRawPath());
            symlinkCommandLineBuilder.Add(majorSymlinkFileLeafname);

            var exitCode = CommandLineProcessor.Processor.Execute(moduleToBuild.OwningNode, symlinkTool, symlinkCommandLineBuilder, null, workingDir);
            var success = (0 == exitCode);
            return success;
        }

        public object
        Build(
            C.PosixSharedLibrarySymlinks moduleToBuild,
            out bool success)
        {
            var realSharedLibraryLoc = moduleToBuild.RealSharedLibraryFileLocation;
            var realSharedLibraryPath = realSharedLibraryLoc.GetSingleRawPath();
            if (!System.IO.File.Exists(realSharedLibraryPath))
            {
                throw new Bam.Core.Exception("Real shared library '{0}' does not exist", realSharedLibraryPath);
            }

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var target = moduleToBuild.OwningNode.Target;
            var creationOptions = moduleToBuild.Options as C.PosixSharedLibrarySymlinksOptionCollection;

            var commandLineBuilder = new Bam.Core.StringArray();
            if (creationOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = creationOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            var symlinkTool = target.Toolset.Tool(typeof(C.IPosixSharedLibrarySymlinksTool)) as C.IPosixSharedLibrarySymlinksTool;
            var workingDir = moduleToBuild.Locations[C.PosixSharedLibrarySymlinks.OutputDir].GetSingleRawPath();

            commandLineBuilder.Add("-s");
            commandLineBuilder.Add("-f"); // TODO: temporary while dependency checking is not active
            var realSharedLibraryLeafname = System.IO.Path.GetFileName(realSharedLibraryPath);
            commandLineBuilder.Add(realSharedLibraryLeafname);

            // create symlink for major version (soname)
            if (!MakeSymlink(commandLineBuilder, moduleToBuild, symlinkTool, workingDir, C.PosixSharedLibrarySymlinks.MajorVersionSymlink))
            {
                success = false;
                return null;
            }
            // create symlink for minor version
            if (!MakeSymlink(commandLineBuilder, moduleToBuild, symlinkTool, workingDir, C.PosixSharedLibrarySymlinks.MinorVersionSymlink))
            {
                success = false;
                return null;
            }
            // create symlink for linker version
            if (!MakeSymlink(commandLineBuilder, moduleToBuild, symlinkTool, workingDir, C.PosixSharedLibrarySymlinks.LinkerSymlink))
            {
                success = false;
                return null;
            }

            success = true;
            return null;
        }
    }
}

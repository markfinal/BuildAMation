#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
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

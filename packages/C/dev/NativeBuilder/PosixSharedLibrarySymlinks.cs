// <copyright file="PosixSharedLibrarySymlinks.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        private static bool
        MakeSymlink(
            Opus.Core.StringArray commandLineBuilder,
            C.PosixSharedLibrarySymlinks moduleToBuild,
            C.IPosixSharedLibrarySymlinksTool symlinkTool,
            string workingDir,
            Opus.Core.LocationKey keyToSymlink)
        {
            var symlinkCommandLineBuilder = new Opus.Core.StringArray(commandLineBuilder);

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
                throw new Opus.Core.Exception("Real shared library '{0}' does not exist", realSharedLibraryPath);
            }

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var target = moduleToBuild.OwningNode.Target;
            var creationOptions = moduleToBuild.Options as C.PosixSharedLibrarySymlinksOptionCollection;

            var commandLineBuilder = new Opus.Core.StringArray();
            if (creationOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = creationOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
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

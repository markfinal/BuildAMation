// <copyright file="PosixSharedLibrarySymlinks.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.PosixSharedLibrarySymlinks moduleToBuild, out bool success)
        {
            var realSharedLibraryLoc = moduleToBuild.RealSharedLibraryFileLocation;
            var realSharedLibraryPath = realSharedLibraryLoc.GetSingleRawPath();
            Opus.Core.Log.MessageAll(realSharedLibraryPath);
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

            // create symlink for major version (soname)
            commandLineBuilder.Add("-s");
            var realSharedLibraryLeafname = System.IO.Path.GetFileName(realSharedLibraryPath);
            commandLineBuilder.Add(realSharedLibraryLeafname);
            var majorSymlinkFile = moduleToBuild.Locations[C.PosixSharedLibrarySymlinks.MajorVersionSymlink];
            var majorSymlinkFileLeafname = System.IO.Path.GetFileName(majorSymlinkFile.GetSingleRawPath());
            commandLineBuilder.Add(majorSymlinkFileLeafname);

            var exitCode = CommandLineProcessor.Processor.Execute(moduleToBuild.OwningNode, symlinkTool, commandLineBuilder, null, workingDir);
            success = (0 == exitCode);

            return null;
        }
    }
}
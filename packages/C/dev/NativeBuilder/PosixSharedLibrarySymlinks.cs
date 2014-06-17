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
            // TODO: this needs to move to the option collection code
            var linkerOptions = moduleToBuild.OwningNode.ExternalDependents[0].Module.Options as C.ILinkerOptions;
            var realSharedLibraryLeafname = System.IO.Path.GetFileName(realSharedLibraryPath);
            var splitName = realSharedLibraryLeafname.Split('.');
            var majorSymlinkLeafname = System.String.Format("{0}.{1}.{2}", splitName[0], splitName[1], linkerOptions.MajorVersion);

            commandLineBuilder.Add("-s");
            commandLineBuilder.Add(realSharedLibraryLeafname);
            commandLineBuilder.Add(majorSymlinkLeafname);

            var exitCode = CommandLineProcessor.Processor.Execute(moduleToBuild.OwningNode, symlinkTool, commandLineBuilder, null, workingDir);
            success = (0 == exitCode);

            return null;
        }
    }
}
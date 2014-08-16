// <copyright file="Win32Resource.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            C.Win32Resource moduleToBuild,
            out bool success)
        {
            var resourceLoc = moduleToBuild.ResourceFileLocation;
            var resourceFilePath = resourceLoc.GetSinglePath();
            if (!System.IO.File.Exists(resourceFilePath))
            {
                throw new Bam.Core.Exception("Resource file '{0}' does not exist", resourceFilePath);
            }

            var resourceFileModule = moduleToBuild as Bam.Core.BaseModule;
            var resourceFileOptions = resourceFileModule.Options;

            var compilerOptions = resourceFileOptions as C.Win32ResourceCompilerOptionCollection;

            // dependency checking, source against output files
            {
                var inputFiles = new Bam.Core.LocationArray();
                inputFiles.Add(resourceLoc);
                var outputFiles = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Bam.Core.Log.DebugMessage("'{0}' is up-to-date", resourceFileModule.OwningNode.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var target = resourceFileModule.OwningNode.Target;

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

            var compilerTool = target.Toolset.Tool(typeof(C.IWinResourceCompilerTool)) as C.IWinResourceCompilerTool;

            // add output path
            commandLineBuilder.Add(System.String.Format("{0}{1}",
                                                        compilerTool.OutputFileSwitch,
                                                        moduleToBuild.Locations[C.Win32Resource.OutputFile].GetSinglePath()));

            // add input path
            commandLineBuilder.Add(System.String.Format("{0}{1}", compilerTool.InputFileSwitch, resourceFilePath));

            var exitCode = CommandLineProcessor.Processor.Execute(resourceFileModule.OwningNode, compilerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}

// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public partial class NativeBuilder
    {
        public object
        Build(
            QtCommon.MocFile moduleToBuild,
            out System.Boolean success)
        {
            var mocFileModule = moduleToBuild as Bam.Core.BaseModule;
            var node = mocFileModule.OwningNode;
            var target = node.Target;
            var mocFileOptions = mocFileModule.Options;
            var toolOptions = mocFileOptions as QtCommon.MocOptionCollection;

            var sourceLocation = moduleToBuild.SourceFileLocation;
            var sourceFilePath = sourceLocation.GetSinglePath();
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Bam.Core.Exception("Moc source file '{0}' does not exist", sourceFilePath);
            }

#if OPUS_ENABLE_FILE_HASHING
            DependencyGenerator.FileHashGeneration.FileProcessQueue.Enqueue(sourceFilePath);
#endif

            // dependency checking
            {
                var outputLocations = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
                var doesSourceFileNeedRebuilding = IsSourceTimeStampNewer(outputLocations, sourceLocation);
                if (FileRebuildStatus.UpToDate == doesSourceFileNeedRebuilding)
                {
                    Bam.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }

#if OPUS_ENABLE_FILE_HASHING
                if (FileRebuildStatus.AlwaysBuild != doesSourceFileNeedRebuilding)
                {
                    if (!DependencyGenerator.FileHashGeneration.HaveFileHashesChanged(inputFiles))
                    {
                        Bam.Core.Log.DebugMessage("'{0}' time stamps changed but contents unchanged", node.UniqueModuleName);
                        success = true;
                        return null;
                    }
                }
#endif
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var commandLineBuilder = new Bam.Core.StringArray();
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Moc options does not support command line translation");
            }

            commandLineBuilder.Add(System.String.Format("-o {0}", moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSinglePath()));
            commandLineBuilder.Add(sourceFilePath);

            var tool = target.Toolset.Tool(typeof(QtCommon.IMocTool));
            var exitCode = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}

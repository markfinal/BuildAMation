// <copyright file="MocFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public partial class NativeBuilder
    {
        public object Build(QtCommon.MocFile moduleToBuild, out System.Boolean success)
        {
            Opus.Core.BaseModule mocFileModule = moduleToBuild as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = mocFileModule.OwningNode;
            Opus.Core.Target target = node.Target;
            Opus.Core.BaseOptionCollection mocFileOptions = mocFileModule.Options;
            QtCommon.MocOptionCollection toolOptions = mocFileOptions as QtCommon.MocOptionCollection;

            string sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Moc source file '{0}' does not exist", sourceFilePath);
            }

#if OPUS_ENABLE_FILE_HASHING
            DependencyGenerator.FileHashGeneration.FileProcessQueue.Enqueue(sourceFilePath);
#endif

            // dependency checking
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.Add(sourceFilePath);

                Opus.Core.StringArray outputFiles = mocFileOptions.OutputPaths.Paths;
                FileRebuildStatus doesSourceFileNeedRebuilding = IsSourceTimeStampNewer(outputFiles, sourceFilePath);
                if (FileRebuildStatus.UpToDate == doesSourceFileNeedRebuilding)
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }

#if OPUS_ENABLE_FILE_HASHING
                if (FileRebuildStatus.AlwaysBuild != doesSourceFileNeedRebuilding)
                {
                    if (!DependencyGenerator.FileHashGeneration.HaveFileHashesChanged(inputFiles))
                    {
                        Opus.Core.Log.DebugMessage("'{0}' time stamps changed but contents unchanged", node.UniqueModuleName);
                        success = true;
                        return null;
                    }
                }
#endif
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var commandLineBuilder = new Opus.Core.StringArray();
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Moc options does not support command line translation");
            }

            if (sourceFilePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", sourceFilePath));
            }
            else
            {
                commandLineBuilder.Add(sourceFilePath);
            }

            Opus.Core.ITool tool = target.Toolset.Tool(typeof(QtCommon.IMocTool));
            int exitCode = CommandLineProcessor.Processor.Execute(node, tool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}
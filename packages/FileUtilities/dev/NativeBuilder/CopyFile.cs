// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(FileUtilities.CopyFile copyFile, out bool success)
        {
            string sourceFilePath = copyFile.SourceFile.AbsolutePath;
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Source file '{0}' does not exist", sourceFilePath);
            }

            Opus.Core.BaseOptionCollection baseOptions = copyFile.Options;
            string copiedFilePath = baseOptions.OutputPaths[FileUtilities.OutputFileFlags.CopiedFile];

            Opus.Core.DependencyNode node = copyFile.OwningNode;

            // dependency checking
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray(
                    sourceFilePath
                );
                Opus.Core.StringArray outputFiles = baseOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            if (baseOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = baseOptions as CommandLineProcessor.ICommandLineSupport;
                Opus.Core.DirectoryCollection directoriesToCreate = commandLineOption.DirectoriesToCreate();
                foreach (string directoryPath in directoriesToCreate)
                {
                    NativeBuilder.MakeDirectory(directoryPath);
                }
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            bool allowOverwrite = true;
            try
            {
                System.IO.File.Copy(sourceFilePath, copiedFilePath, allowOverwrite);
            }
            catch (System.IO.IOException ex)
            {
                throw new Opus.Core.Exception(ex.Message);
            }

            Opus.Core.Log.Info("Copied '{0}' to '{1}'", sourceFilePath, copiedFilePath);

            success = true;
            return null;
        }
    }
}

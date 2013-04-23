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

            // TODO: can this ever throw an exception?
            bool allowOverwrite = true;
            System.IO.File.Copy(sourceFilePath, copiedFilePath, allowOverwrite);

            success = true;
            return null;
        }
    }
}

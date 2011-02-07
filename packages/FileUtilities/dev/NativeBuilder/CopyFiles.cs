// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(FileUtilities.CopyFiles copyFiles, Opus.Core.DependencyNode node, out bool success)
        {
            System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
            Opus.Core.StringArray sourceFiles = new Opus.Core.StringArray();
            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                sourceModule.Options.FilterOutputPaths(sourceOutputPaths, sourceFiles);
            }

            string destinationDirectory;
            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            switch (copyFiles.DirectoryChoiceFlags)
            {
                case FileUtilities.EDirectoryChoice.TargetBuildDirectory:
                    {
                        //destinationDirectory = destinationModule.Options.
                        destinationDirectory = System.IO.Path.GetDirectoryName(destinationModule.Options.OutputPaths[C.OutputFileFlags.Executable]);
                    }
                    break;

                default:
                    throw new Opus.Core.Exception("Undefined directory choice flags");
            }

            FileUtilities.CopyFilesTool tool = new FileUtilities.CopyFilesTool();
            string executablePath = tool.Executable(node.Target);

            int returnValue = -1;
            foreach (string sourcePath in sourceFiles)
            {
                System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
                if (Opus.Core.OSUtilities.IsWindowsHosting)
                {
                    commandLineBuilder.Append("/c COPY ");
                }
                commandLineBuilder.AppendFormat("\"{0}\" \"{1}\"", sourcePath, destinationDirectory);

                returnValue = CommandLineProcessor.Processor.Execute(node, tool, executablePath, commandLineBuilder);
                if (0 != returnValue)
                {
                    break;
                }
            }

            success = (0 != returnValue);
            return null;
        }
    }
}

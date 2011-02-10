// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(FileUtilities.CopyFiles copyFiles, Opus.Core.DependencyNode node, out bool success)
        {
#if true
            System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
            Opus.Core.StringArray sourceFiles = new Opus.Core.StringArray();
            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                sourceModule.Options.FilterOutputPaths(sourceOutputPaths, sourceFiles);
            }
            if (null != copyFiles.SourceFiles)
            {
                foreach (string path in copyFiles.SourceFiles)
                {
                    sourceFiles.Add(path);
                }
            }
            if (0 == sourceFiles.Count)
            {
                Opus.Core.Log.DebugMessage("No files to copy");
                success = true;
                return null;
            }

            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            string destinationDirectory = null;
            if (null != destinationModule)
            {
                Opus.Core.StringArray destinationPaths = new Opus.Core.StringArray();
                destinationModule.Options.FilterOutputPaths(copyFiles.DirectoryOutputFlags, destinationPaths);
                destinationDirectory = System.IO.Path.GetDirectoryName(destinationPaths[0]);
            }
            else
            {
                destinationDirectory = copyFiles.DestinationDirectory;
            }

            string makeFile = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFile));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFile);

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (Opus.Core.OSUtilities.IsWindowsHosting)
            {
                commandLineBuilder.Append("/c COPY ");
            }
            commandLineBuilder.AppendFormat("\"{0}\" \"{1}\"", sourceFiles[0], destinationDirectory);

            FileUtilities.CopyFilesTool tool = new FileUtilities.CopyFilesTool();

            Opus.Core.StringArray commandLines = new Opus.Core.StringArray();
            commandLines.Add("\"" + tool.Executable(node.Target) + "\" " + commandLineBuilder.ToString() + " " + sourceFiles[0]);

            MakeFileBuilderRecipe recipe = new MakeFileBuilderRecipe(node, sourceFiles, null, commandLines, this.topLevelMakeFilePath);

            string makeFileTargetName = null;
            string makeFileVariableName = null;
            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFile))
            {
                recipe.Write(makeFileWriter, C.OutputFileFlags.ObjectFile);
                makeFileTargetName = recipe.TargetName;
                makeFileVariableName = recipe.VariableName;
            }

            success = true;

            MakeFileData returnData = new MakeFileData(makeFile, makeFileTargetName, makeFileVariableName, tool.EnvironmentPaths(node.Target));
            return returnData;
#else
            System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
            Opus.Core.StringArray sourceFiles = new Opus.Core.StringArray();
            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                sourceModule.Options.FilterOutputPaths(sourceOutputPaths, sourceFiles);
            }
            if (null != copyFiles.SourceFiles)
            {
                foreach (string path in copyFiles.SourceFiles)
                {
                    sourceFiles.Add(path);
                }
            }
            if (0 == sourceFiles.Count)
            {
                Opus.Core.Log.DebugMessage("No files to copy");
                success = true;
                return null;
            }

            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            string destinationDirectory = null;
            if (null != destinationModule)
            {
                Opus.Core.StringArray destinationPaths = new Opus.Core.StringArray();
                destinationModule.Options.FilterOutputPaths(copyFiles.DirectoryOutputFlags, destinationPaths);
                destinationDirectory = System.IO.Path.GetDirectoryName(destinationPaths[0]);
            }
            else
            {
                destinationDirectory = copyFiles.DestinationDirectory;
                if (copyFiles.CreateDirectory)
                {
                    NativeBuilder.MakeDirectory(destinationDirectory);
                }
            }

            FileUtilities.CopyFilesTool tool = new FileUtilities.CopyFilesTool();
            string executablePath = tool.Executable(node.Target);

            int returnValue = -1;
            foreach (string sourcePath in sourceFiles)
            {
                string destinationFile = System.IO.Path.Combine(destinationDirectory, System.IO.Path.GetFileName(sourcePath));

                bool requiresBuilding = NativeBuilder.RequiresBuilding(destinationFile, sourcePath);
                if (!requiresBuilding)
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    returnValue = 0;
                    continue;
                }

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

            success = (0 == returnValue);
            return null;
#endif
        }
    }
}

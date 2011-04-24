// <copyright file="CopyFiles.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>FileUtilities package</summary>
// <author>Mark Final</author>
namespace QtCreatorBuilder
{
    public sealed partial class QtCreatorBuilder
    {
        public object Build(FileUtilities.CopyFiles copyFiles, Opus.Core.DependencyNode node, out bool success)
        {
#if true
            System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
            System.Collections.Generic.List<NodeData> sourceFileDataArray = new System.Collections.Generic.List<NodeData>();
            Opus.Core.StringArray sourceFiles = null;
            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                if (sourceModule.Options.OutputPaths.Has(sourceOutputPaths))
                {
                    if (sourceModule.OwningNode.Data != null)
                    {
                        NodeData data = sourceModule.OwningNode.Data as NodeData;
                        sourceFileDataArray.Add(data);
                    }
                }
            }
            if (null != copyFiles.SourceFiles)
            {
                sourceFiles = new Opus.Core.StringArray();
                foreach (string path in copyFiles.SourceFiles)
                {
                    sourceFiles.Add(path);
                }
            }
            if (0 == sourceFileDataArray.Count && null == sourceFiles)
            {
                Opus.Core.Log.DebugMessage("No files to copy");
                success = true;
                return null;
            }

            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            string destinationDirectory = null;
            System.Enum destinationOutputFlags;
            if (null != destinationModule)
            {
                Opus.Core.StringArray destinationPaths = new Opus.Core.StringArray();
                destinationModule.Options.FilterOutputPaths(copyFiles.DirectoryOutputFlags, destinationPaths);
                destinationOutputFlags = copyFiles.DirectoryOutputFlags;
                destinationDirectory = System.IO.Path.GetDirectoryName(destinationPaths[0]);
            }
            else
            {
                destinationOutputFlags = sourceOutputPaths;
                destinationDirectory = copyFiles.DestinationDirectory;
            }

            FileUtilities.CopyFilesTool tool = new FileUtilities.CopyFilesTool();
            string toolExecutablePath = tool.Executable(node.Target);

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (copyFiles.Options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = copyFiles.Options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, node.Target);
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            foreach (NodeData sourceNode in sourceFileDataArray)
            {
                string proFilePathName = sourceNode.ProFilePathName;
                using (System.IO.TextWriter proFileWriter = new System.IO.StreamWriter(proFilePathName, true))
                {
                    proFileWriter.WriteLine("# Write '{0}' to '{1}'", "$(DESTDIR_TARGET)", destinationDirectory);
                    if (sourceNode.HasPostLinks)
                    {
                        proFileWriter.WriteLine("QMAKE_POST_LINK += &&");
                    }
                    proFileWriter.WriteLine("win32:QMAKE_POST_LINK += {0} {1} {2} {3}", toolExecutablePath, commandLineBuilder.ToString(' '), "$(DESTDIR_TARGET)", destinationDirectory);
                    proFileWriter.WriteLine("!win32:QMAKE_POST_LINK += {0} {1} {2} {3}", toolExecutablePath, commandLineBuilder.ToString(' '), "$$DESTDIR/$(TARGET0)", destinationDirectory);
                    sourceNode.HasPostLinks = true;
                }
            }

            success = true;
            return null;
#else
            System.Enum sourceOutputPaths = copyFiles.SourceOutputFlags;
            System.Collections.Generic.List<MakeFileData> sourceFileDataArray = new System.Collections.Generic.List<MakeFileData>();
            Opus.Core.StringArray sourceFiles = null;
            foreach (Opus.Core.IModule sourceModule in copyFiles.SourceModules)
            {
                if (sourceModule.Options.OutputPaths.Has(sourceOutputPaths))
                {
                    if (sourceModule.OwningNode.Data != null)
                    {
                        MakeFileData data = sourceModule.OwningNode.Data as MakeFileData;
                        sourceFileDataArray.Add(data);
                    }
                }
            }
            if (null != copyFiles.SourceFiles)
            {
                sourceFiles = new Opus.Core.StringArray();
                foreach (string path in copyFiles.SourceFiles)
                {
                    sourceFiles.Add(path);
                }
            }
            if (0 == sourceFileDataArray.Count && null == sourceFiles)
            {
                Opus.Core.Log.DebugMessage("No files to copy");
                success = true;
                return null;
            }

            Opus.Core.IModule destinationModule = copyFiles.DestinationModule;
            string destinationDirectory = null;
            System.Enum destinationOutputFlags;
            if (null != destinationModule)
            {
                Opus.Core.StringArray destinationPaths = new Opus.Core.StringArray();
                destinationModule.Options.FilterOutputPaths(copyFiles.DirectoryOutputFlags, destinationPaths);
                destinationOutputFlags = copyFiles.DirectoryOutputFlags;
                destinationDirectory = System.IO.Path.GetDirectoryName(destinationPaths[0]);
            }
            else
            {
                destinationOutputFlags = sourceOutputPaths;
                destinationDirectory = copyFiles.DestinationDirectory;
            }

            FileUtilities.CopyFilesTool tool = new FileUtilities.CopyFilesTool();
            string toolExecutablePath = tool.Executable(node.Target);

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (copyFiles.Options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = copyFiles.Options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, node.Target);
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            string recipe = System.String.Format("\"{0}\" {1}$< $@", toolExecutablePath, commandLineBuilder.ToString(' '));
            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            Opus.Core.DirectoryCollection directoriesToCreate = new Opus.Core.DirectoryCollection();
            directoriesToCreate.AddAbsoluteDirectory(destinationDirectory, false);

            string makeFilePath = QtCreatorBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile path : '{0}'", makeFilePath);

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            Opus.Core.StringArray destinationPathNames = new Opus.Core.StringArray();
            foreach (MakeFileData data in sourceFileDataArray)
            {
                Opus.Core.StringArray variableDictionary = data.VariableDictionary[sourceOutputPaths];
                foreach (string variable in variableDictionary)
                {
                    string destinationPathName = System.String.Format("{0}{1}$(notdir $({2}))", destinationDirectory, System.IO.Path.DirectorySeparatorChar, variable);
                    if (destinationPathNames.Contains(destinationPathName))
                    {
                        Opus.Core.Log.DebugMessage("Target pathname '{0}' has already been defined", destinationPathName);
                        continue;
                    }

                    Opus.Core.OutputPaths outputPaths = new Opus.Core.OutputPaths();
                    outputPaths[destinationOutputFlags] = destinationPathName;

                    MakeFileVariableDictionary singleEntry = new MakeFileVariableDictionary();
                    singleEntry.Add(sourceOutputPaths, new Opus.Core.StringArray(variable));

                    MakeFileRule rule = new MakeFileRule(outputPaths, destinationOutputFlags, node.UniqueModuleName, directoriesToCreate, singleEntry, null, recipes);
                    makeFile.RuleArray.Add(rule);

                    destinationPathNames.Add(destinationPathName);
                }
            }
            if (null != sourceFiles)
            {
                foreach (string sourceFile in sourceFiles)
                {
                    string destinationPathName = System.String.Format("{0}{1}$(notdir {2})", destinationDirectory, System.IO.Path.DirectorySeparatorChar, sourceFile);

                    Opus.Core.OutputPaths outputPaths = new Opus.Core.OutputPaths();
                    outputPaths[destinationOutputFlags] = destinationPathName;

                    MakeFileRule rule = new MakeFileRule(outputPaths, destinationOutputFlags, node.UniqueModuleName, directoriesToCreate, null, new Opus.Core.StringArray(sourceFile), recipes);
                    makeFile.RuleArray.Add(rule);
                }
            }

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            success = true;

            MakeFileData nodeData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, null);
            return nodeData;
#endif
        }
    }
}

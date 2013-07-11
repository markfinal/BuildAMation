// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.ObjectFile moduleToBuild, out bool success)
        {
            string sourceFilePath = moduleToBuild.SourceFile.AbsolutePath;
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Source file '{0}' does not exist", sourceFilePath);
            }

#if OPUS_ENABLE_FILE_HASHING
            DependencyGenerator.FileHashGeneration.FileProcessQueue.Enqueue(sourceFilePath);
#endif

            Opus.Core.BaseModule objectFileModule = moduleToBuild as Opus.Core.BaseModule;
            Opus.Core.BaseOptionCollection objectFileOptions = objectFileModule.Options;
            Opus.Core.DependencyNode node = objectFileModule.OwningNode;

            C.CompilerOptionCollection compilerOptions = objectFileOptions as C.CompilerOptionCollection;

            string depFilePath = DependencyGenerator.IncludeDependencyGeneration.HeaderDependencyPathName(sourceFilePath, compilerOptions.OutputDirectoryPath);

            bool headerDependencyGeneration = (bool)Opus.Core.State.Get("C", "HeaderDependencyGeneration");

            // dependency checking, source against output files
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.Add(sourceFilePath);
                Opus.Core.StringArray outputFiles = compilerOptions.OutputPaths.Paths;
                FileRebuildStatus doesSourceFileNeedRebuilding = IsSourceTimeStampNewer(outputFiles, sourceFilePath);
                if (FileRebuildStatus.UpToDate == doesSourceFileNeedRebuilding)
                {
                    // now try the header dependencies
                    if (headerDependencyGeneration && System.IO.File.Exists(depFilePath))
                    {
                        using (System.IO.TextReader depFileReader = new System.IO.StreamReader(depFilePath))
                        {
                            string deps = depFileReader.ReadToEnd();
                            Opus.Core.StringArray depsArray = new Opus.Core.StringArray(deps.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries));
#if OPUS_ENABLE_FILE_HASHING
                            DependencyGenerator.FileHashGeneration.FileProcessQueue.Enqueue(depsArray);
#endif
                            if (!RequiresBuilding(outputFiles, depsArray))
                            {
                                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                                success = true;
                                return null;
                            }

                            inputFiles.AddRange(depsArray);
                        }
                    }
                    else
                    {
                        Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                        success = true;
                        return null;
                    }
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

            var target = node.Target;

            var commandLineBuilder = new Opus.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);

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

            var moduleToolAttributes = moduleToBuild.GetType().GetCustomAttributes(typeof(Opus.Core.ModuleToolAssignmentAttribute), true);
            System.Type toolType = (moduleToolAttributes[0] as Opus.Core.ModuleToolAssignmentAttribute).ToolType;
            Opus.Core.ITool toolInterface = target.Toolset.Tool(toolType);

            if (headerDependencyGeneration)
            {
                DependencyGenerator.IncludeDependencyGeneration.Data dependencyData = new DependencyGenerator.IncludeDependencyGeneration.Data();
                dependencyData.sourcePath = sourceFilePath;
                dependencyData.depFilePath = depFilePath;

                var cOptions = objectFileOptions as C.ICCompilerOptions;
                Opus.Core.StringArray includePaths = cOptions.IncludePaths.ToStringArray();
                dependencyData.includePaths = new Opus.Core.StringArray();
                foreach (string path in includePaths)
                {
                    if (path == ".")
                    {
                        dependencyData.includePaths.Add(System.IO.Path.GetDirectoryName(sourceFilePath));
                    }
                    else
                    {
                        dependencyData.includePaths.Add(path);
                    }
                }

                DependencyGenerator.IncludeDependencyGeneration.FileProcessQueue.Enqueue(dependencyData);
            }

            if (sourceFilePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", sourceFilePath));
            }
            else
            {
                commandLineBuilder.Add(sourceFilePath);
            }

            int exitCode = CommandLineProcessor.Processor.Execute(node, toolInterface, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}
// <copyright file="CObjectFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(C.ObjectFile objectFile, out bool success)
        {
            string sourceFilePath = objectFile.SourceFile.AbsolutePath;
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception(System.String.Format("Source file '{0}' does not exist", sourceFilePath));
            }

#if OPUS_ENABLE_FILE_HASHING
            DependencyGenerator.FileHashGeneration.FileProcessQueue.Enqueue(sourceFilePath);
#endif

            Opus.Core.IModule objectFileModule = objectFile as Opus.Core.IModule;
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

            Opus.Core.Target target = node.Target;

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

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

            // NEW STYLE
            var moduleToolAttributes = objectFile.GetType().GetCustomAttributes(typeof(Opus.Core.ModuleToolAssignmentAttribute), true);
            System.Type toolType = (moduleToolAttributes[0] as Opus.Core.ModuleToolAssignmentAttribute).ToolchainType;
            Opus.Core.ITool toolInterface = null;
            if (typeof(C.Compiler) == toolType)
            {
                toolInterface = C.CCompilerFactory.GetInstance(target);
            }
            else if (typeof(C.CxxCompiler) == toolType)
            {
                toolInterface = C.CxxCompilerFactory.GetInstance(target);
            }
            else
            {
                throw new Opus.Core.Exception(System.String.Format("Unrecognized compiler tool type, '{0}'", toolType.ToString()));
            }

            if (headerDependencyGeneration)
            {
                DependencyGenerator.IncludeDependencyGeneration.Data dependencyData = new DependencyGenerator.IncludeDependencyGeneration.Data();
                dependencyData.sourcePath = sourceFilePath;
                dependencyData.depFilePath = depFilePath;

                Opus.Core.StringArray includeSwitches = (toolInterface as C.ICompiler).IncludePathCompilerSwitches;
                Opus.Core.StringArray includePaths = new Opus.Core.StringArray();
                // TODO: this can be simplified to just use the optioncollection
                foreach (string option in commandLineBuilder)
                {
                    string foundSwitch = null;
                    foreach (string includeSwitch in includeSwitches)
                    {
                        if (option.StartsWith(includeSwitch))
                        {
                            foundSwitch = includeSwitch;
                            break;
                        }
                    }

                    if (foundSwitch != null)
                    {
                        string path = option.Substring(foundSwitch.Length); // strip the option switch prefix
                        if ("." == path)
                        {
                            path = System.IO.Path.GetDirectoryName(sourceFilePath);
                        }
                        else if (path.StartsWith("\""))
                        {
                            path = path.Substring(1, path.Length - 2); // strip quotes
                        }
                        includePaths.Add(path);
                    }
                }
                dependencyData.includePaths = includePaths;

                DependencyGenerator.IncludeDependencyGeneration.FileProcessQueue.Enqueue(dependencyData);
            }

            // NEW STYLE
#if true
            string executablePath = toolInterface.Executable(target);
#else
            string executablePath;
            C.IToolchainOptions toolchainOptions = (objectFileOptions as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executablePath = compilerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executablePath = compilerTool.Executable(target);
            }
#endif

            if (sourceFilePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", sourceFilePath));
            }
            else
            {
                commandLineBuilder.Add(sourceFilePath);
            }

            int exitCode = CommandLineProcessor.Processor.Execute(node, toolInterface, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}
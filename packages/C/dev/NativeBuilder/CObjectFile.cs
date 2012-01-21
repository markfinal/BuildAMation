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

            C.CompilerOptionCollection compilerOptions = objectFile.Options as C.CompilerOptionCollection;

            string depFilePath = DependencyChecker.IncludeDependencyGeneration.HeaderDependencyPathName(sourceFilePath, compilerOptions.OutputDirectoryPath);

            // dependency checking, source against output files
            {
                Opus.Core.StringArray inputFiles = new Opus.Core.StringArray();
                inputFiles.Add(sourceFilePath);
                Opus.Core.StringArray outputFiles = compilerOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, inputFiles))
                {
                    // now try the header dependencies
                    if (Opus.Core.State.Get("C", "HeaderDependencyGeneration", true) &&
                        System.IO.File.Exists(depFilePath))
                    {
                        using (System.IO.TextReader depFileReader = new System.IO.StreamReader(depFilePath))
                        {
                            string deps = depFileReader.ReadToEnd();
                            Opus.Core.StringArray depsArray = new Opus.Core.StringArray(deps.Split('\n'));
                            if (!RequiresBuilding(outputFiles, depsArray))
                            {
                                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", objectFile.OwningNode.UniqueModuleName);
                                success = true;
                                return null;
                            }
                        }
                    }
                    else
                    {
                        Opus.Core.Log.DebugMessage("'{0}' is up-to-date", objectFile.OwningNode.UniqueModuleName);
                        success = true;
                        return null;
                    }
                }
            }

            Opus.Core.Target target = objectFile.OwningNode.Target;

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

            C.Compiler compilerInstance = C.CompilerFactory.GetTargetInstance(target, C.ClassNames.CCompilerTool);
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;

            if (Opus.Core.State.Get("C", "HeaderDependencyGeneration", true))
            {
                DependencyChecker.IncludeDependencyGeneration.DependencyQueue.Data dependencyData = new DependencyChecker.IncludeDependencyGeneration.DependencyQueue.Data();
                dependencyData.sourcePath = sourceFilePath;
                dependencyData.depFilePath = depFilePath;

                Opus.Core.StringArray includeSwitches = compilerInstance.IncludePathCompilerSwitches;
                Opus.Core.StringArray includePaths = new Opus.Core.StringArray();
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

                DependencyChecker.IncludeDependencyGeneration.FileProcessQueue.Enqueue(dependencyData);
            }

            string executablePath;
            C.IToolchainOptions toolchainOptions = (objectFile.Options as C.ICCompilerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executablePath = compilerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executablePath = compilerTool.Executable(target);
            }

            if (sourceFilePath.Contains(" "))
            {
                commandLineBuilder.Add(System.String.Format("\"{0}\"", sourceFilePath));
            }
            else
            {
                commandLineBuilder.Add(sourceFilePath);
            }

            int exitCode = CommandLineProcessor.Processor.Execute(objectFile.OwningNode, compilerTool, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}
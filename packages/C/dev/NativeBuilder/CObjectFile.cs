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
            var sourceLoc = moduleToBuild.SourceFileLocation;
            var sourceFilePath = sourceLoc.GetSinglePath();
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Opus.Core.Exception("Source file '{0}' does not exist", sourceFilePath);
            }

#if OPUS_ENABLE_FILE_HASHING
            DependencyGenerator.FileHashGeneration.FileProcessQueue.Enqueue(sourceFilePath);
#endif

            var objectFileModule = moduleToBuild as Opus.Core.BaseModule;
            var objectFileOptions = objectFileModule.Options;
            var node = objectFileModule.OwningNode;

            var compilerOptions = objectFileOptions as C.CompilerOptionCollection;

            var depFilePath = DependencyGenerator.IncludeDependencyGeneration.HeaderDependencyPathName(sourceFilePath, moduleToBuild.Locations[C.ObjectFile.OutputDir]);

            var headerDependencyGeneration = (bool)Opus.Core.State.Get("C", "HeaderDependencyGeneration");

            // dependency checking, source against output files
            {
                var inputFiles = new Opus.Core.LocationArray();
                inputFiles.Add(sourceLoc);
                var outputFiles = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.File, Opus.Core.Location.EExists.WillExist);
                var doesSourceFileNeedRebuilding = IsSourceTimeStampNewer(outputFiles, sourceLoc);
                if (FileRebuildStatus.UpToDate == doesSourceFileNeedRebuilding)
                {
                    // now try the header dependencies
                    if (headerDependencyGeneration && System.IO.File.Exists(depFilePath))
                    {
                        using (var depFileReader = new System.IO.StreamReader(depFilePath))
                        {
                            var deps = depFileReader.ReadToEnd();
                            var splitDeps = deps.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
                            var depLocArray = new Opus.Core.LocationArray();
                            foreach (var depPath in splitDeps)
                            {
                                depLocArray.Add(Opus.Core.FileLocation.Get(depPath));
                            }
#if OPUS_ENABLE_FILE_HASHING
                            DependencyGenerator.FileHashGeneration.FileProcessQueue.Enqueue(depLocArray);
#endif
                            if (!RequiresBuilding(outputFiles, depLocArray))
                            {
                                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                                success = true;
                                return null;
                            }

                            inputFiles.AddRange(depLocArray);
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

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var target = node.Target;

            var commandLineBuilder = new Opus.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            var moduleToolAttributes = moduleToBuild.GetType().GetCustomAttributes(typeof(Opus.Core.ModuleToolAssignmentAttribute), true);
            var toolType = (moduleToolAttributes[0] as Opus.Core.ModuleToolAssignmentAttribute).ToolType;
            var toolInterface = target.Toolset.Tool(toolType);

            if (headerDependencyGeneration)
            {
                var dependencyData = new DependencyGenerator.IncludeDependencyGeneration.Data();
                dependencyData.sourcePath = sourceFilePath;
                dependencyData.depFilePath = depFilePath;

                var cOptions = objectFileOptions as C.ICCompilerOptions;
                var includePaths = cOptions.IncludePaths.ToStringArray();
                dependencyData.includePaths = new Opus.Core.StringArray();
                foreach (var path in includePaths)
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

            commandLineBuilder.Add(sourceFilePath);

            var exitCode = CommandLineProcessor.Processor.Execute(node, toolInterface, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}
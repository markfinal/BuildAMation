#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            C.ObjectFile moduleToBuild,
            out bool success)
        {
            var sourceLoc = moduleToBuild.SourceFileLocation;
            var sourceFilePath = sourceLoc.GetSinglePath();
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Bam.Core.Exception("Source file '{0}' does not exist", sourceFilePath);
            }

#if OPUS_ENABLE_FILE_HASHING
            DependencyGenerator.FileHashGeneration.FileProcessQueue.Enqueue(sourceFilePath);
#endif

            var objectFileModule = moduleToBuild as Bam.Core.BaseModule;
            var objectFileOptions = objectFileModule.Options;
            var node = objectFileModule.OwningNode;

            var compilerOptions = objectFileOptions as C.CompilerOptionCollection;

            var depFilePath = DependencyGenerator.IncludeDependencyGeneration.HeaderDependencyPathName(sourceFilePath, moduleToBuild.Locations[C.ObjectFile.OutputDir]);

            var headerDependencyGeneration = (bool)Bam.Core.State.Get("C", "HeaderDependencyGeneration");

            // dependency checking, source against output files
            {
                var inputFiles = new Bam.Core.LocationArray();
                inputFiles.Add(sourceLoc);
                var outputFiles = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
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
                            var depLocArray = new Bam.Core.LocationArray();
                            foreach (var depPath in splitDeps)
                            {
                                depLocArray.Add(Bam.Core.FileLocation.Get(depPath));
                            }
#if OPUS_ENABLE_FILE_HASHING
                            DependencyGenerator.FileHashGeneration.FileProcessQueue.Enqueue(depLocArray);
#endif
                            if (!RequiresBuilding(outputFiles, depLocArray))
                            {
                                Bam.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                                success = true;
                                return null;
                            }

                            inputFiles.AddRange(depLocArray);
                        }
                    }
                    else
                    {
                        Bam.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                        success = true;
                        return null;
                    }
                }

#if OPUS_ENABLE_FILE_HASHING
                if (FileRebuildStatus.AlwaysBuild != doesSourceFileNeedRebuilding)
                {
                    if (!DependencyGenerator.FileHashGeneration.HaveFileHashesChanged(inputFiles))
                    {
                        Bam.Core.Log.DebugMessage("'{0}' time stamps changed but contents unchanged", node.UniqueModuleName);
                        success = true;
                        return null;
                    }
                }
#endif
            }

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var target = node.Target;

            var commandLineBuilder = new Bam.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            var moduleToolAttributes = moduleToBuild.GetType().GetCustomAttributes(typeof(Bam.Core.ModuleToolAssignmentAttribute), true);
            var toolType = (moduleToolAttributes[0] as Bam.Core.ModuleToolAssignmentAttribute).ToolType;
            var toolInterface = target.Toolset.Tool(toolType);

            if (headerDependencyGeneration)
            {
                var dependencyData = new DependencyGenerator.IncludeDependencyGeneration.Data();
                dependencyData.sourcePath = sourceFilePath;
                dependencyData.depFilePath = depFilePath;

                var cOptions = objectFileOptions as C.ICCompilerOptions;
                var includePaths = cOptions.IncludePaths.ToStringArray();
                dependencyData.includePaths = new Bam.Core.StringArray();
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

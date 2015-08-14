#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace C
{
namespace V2
{
    public sealed class NativeCompilation :
        ICompilationPolicy
    {
        void
        ICompilationPolicy.Compile(
            ObjectFile sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.TokenizedString objectFilePath,
            Bam.Core.V2.Module source)
        {
            sender.MetaData = new Bam.Core.StringArray();
            var interfaceType = Bam.Core.State.ScriptAssembly.GetType("CommandLineProcessor.V2.IConvertToCommandLine");
            if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
            {
                var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                map.InterfaceMethods[0].Invoke(sender.Settings, new[] { sender, sender.MetaData });
            }

            var objectFileDir = System.IO.Path.GetDirectoryName(objectFilePath.ToString());
            if (!System.IO.Directory.Exists(objectFileDir))
            {
                System.IO.Directory.CreateDirectory(objectFileDir);
            }

            /*var exitStatus = */CommandLineProcessor.V2.Processor.Execute(context, sender.Tool, sender.MetaData as Bam.Core.StringArray);
        }
    }
}
}
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

#if BAM_ENABLE_FILE_HASHING
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
                                // handle dependencies that no longer exist, or have been renamed
                                if (!System.IO.File.Exists(depPath))
                                {
                                    Bam.Core.Log.DebugMessage("Implicit dependency '{0}' no longer exists for node {1}", depPath, node.UniqueModuleName);
                                    continue;
                                }
                                depLocArray.Add(Bam.Core.FileLocation.Get(depPath));
                            }
#if BAM_ENABLE_FILE_HASHING
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

#if BAM_ENABLE_FILE_HASHING
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

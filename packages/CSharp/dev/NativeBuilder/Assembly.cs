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
namespace NativeBuilder
{
    public partial class NativeBuilder
    {
        public object
        Build(
            CSharp.Assembly moduleToBuild,
            out System.Boolean success)
        {
            var assemblyModule = moduleToBuild as Bam.Core.BaseModule;
            var node = assemblyModule.OwningNode;
            var target = node.Target;
            var assemblyOptions = assemblyModule.Options;
            var options = assemblyOptions as CSharp.IOptions;

            if (node.ExternalDependents != null)
            {
                var keyFilters = new Bam.Core.Array<Bam.Core.LocationKey>(
                    CSharp.Assembly.OutputFile
                    );
                var dependentAssemblies = new Bam.Core.LocationArray();
                node.ExternalDependents.FilterOutputLocations(keyFilters, dependentAssemblies);

                foreach (var loc in dependentAssemblies)
                {
                    options.References.Add(loc.GetSinglePath());
                }
            }

            var sourceFiles = new Bam.Core.StringArray();
            var fields = moduleToBuild.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // C# files
                {
                    var sourceFileAttributes = field.GetCustomAttributes(typeof(Bam.Core.SourceFilesAttribute), false);
                    if (null != sourceFileAttributes && sourceFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Bam.Core.Location)
                        {
                            var file = sourceField as Bam.Core.Location;
                            var absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Bam.Core.Exception("Source file '{0}' does not exist", absolutePath);
                            }

                            sourceFiles.Add(absolutePath);
                        }
                        else if (sourceField is Bam.Core.FileCollection)
                        {
                            var sourceCollection = sourceField as Bam.Core.FileCollection;
                            // TODO: convert to var
                            foreach (Bam.Core.Location location in sourceCollection)
                            {
                                var absolutePath = location.AbsolutePath;
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Bam.Core.Exception("Source file '{0}' does not exist", absolutePath);
                                }

                                sourceFiles.Add(absolutePath);
                            }
                        }
                        else
                        {
                            throw new Bam.Core.Exception("Field '{0}' of '{1}' should be of type Bam.Core.File or Bam.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }

                // WPF application definition .xaml file
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.ApplicationDefinitionAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Bam.Core.Location)
                        {
                            var file = sourceField as Bam.Core.Location;
                            var absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Bam.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                            }

                            var csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Bam.Core.Exception("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath);
                            }

                            sourceFiles.Add(csPath);
                        }
                        else if (sourceField is Bam.Core.FileCollection)
                        {
                            var sourceCollection = sourceField as Bam.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Bam.Core.Exception("There can be only one application definition");
                            }

                            // TODO: convert to var
                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Bam.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                                }

                                var csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Bam.Core.Exception("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath);
                                }

                                sourceFiles.Add(csPath);
                            }
                        }
                        else
                        {
                            throw new Bam.Core.Exception("Field '{0}' of '{1}' should be of type Bam.Core.File or Bam.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }

                // WPF page .xaml files
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.PagesAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Bam.Core.Location)
                        {
                            var file = sourceField as Bam.Core.Location;
                            var absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Bam.Core.Exception("Page file '{0}' does not exist", absolutePath);
                            }

                            var csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Bam.Core.Exception("Associated source file '{0}' to page file '{1}' does not exist", csPath, absolutePath);
                            }

                            sourceFiles.Add(csPath);
                        }
                        else if (sourceField is Bam.Core.FileCollection)
                        {
                            var sourceCollection = sourceField as Bam.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Bam.Core.Exception("There can be only one page file");
                            }

                            // TODO: convert to var
                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Bam.Core.Exception("Page file '{0}' does not exist", absolutePath);
                                }

                                var csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Bam.Core.Exception("Associated source file '{0}' to page file '{1}' does not exist", csPath, absolutePath);
                                }

                                sourceFiles.Add(csPath);
                            }
                        }
                        else
                        {
                            throw new Bam.Core.Exception("Field '{0}' of '{1}' should be of type Bam.Core.File or Bam.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }
            }

            if (0 == sourceFiles.Count)
            {
                throw new Bam.Core.Exception("There were no source files specified for the module '{0}'", node.ModuleName);
            }

            // dependency checking
            {
                var inputLocations = new Bam.Core.LocationArray();
                foreach (var source in sourceFiles)
                {
                    inputLocations.Add(Bam.Core.FileLocation.Get(source));
                }
                var outputLocations = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
                if (!RequiresBuilding(outputLocations, inputLocations))
                {
                    Bam.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var commandLineBuilder = new Bam.Core.StringArray();
            if (options is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            foreach (var source in sourceFiles)
            {
                if (source.Contains(" "))
                {
                    commandLineBuilder.Add(System.String.Format("\"{0}\"", source));
                }
                else
                {
                    commandLineBuilder.Add(source);
                }
            }

            var compilerTool = target.Toolset.Tool(typeof(CSharp.ICSharpCompilerTool));

            var exitCode = CommandLineProcessor.Processor.Execute(node, compilerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}

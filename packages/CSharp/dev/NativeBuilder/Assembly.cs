// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public partial class NativeBuilder
    {
        public object Build(CSharp.Assembly moduleToBuild, out System.Boolean success)
        {
            var assemblyModule = moduleToBuild as Opus.Core.BaseModule;
            var node = assemblyModule.OwningNode;
            var target = node.Target;
            var assemblyOptions = assemblyModule.Options;
            var options = assemblyOptions as CSharp.IOptions;

            if (node.ExternalDependents != null)
            {
#if true
                var keyFilters = new Opus.Core.Array<Opus.Core.LocationKey>(
                    CSharp.Assembly.OutputFile
                    );
                var dependentAssemblies = new Opus.Core.LocationArray();
                node.ExternalDependents.FilterOutputLocations(keyFilters, dependentAssemblies);

                foreach (var loc in dependentAssemblies)
                {
                    options.References.Add(loc.GetSinglePath());
                }
#else
                Opus.Core.StringArray dependentAssemblies = new Opus.Core.StringArray();
                node.ExternalDependents.FilterOutputPaths(CSharp.OutputFileFlags.AssemblyFile, dependentAssemblies);
                options.References.AddRange(dependentAssemblies);
#endif
            }

            var sourceFiles = new Opus.Core.StringArray();
            var fields = moduleToBuild.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // C# files
                {
                    var sourceFileAttributes = field.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                    if (null != sourceFileAttributes && sourceFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Opus.Core.Location)
                        {
                            var file = sourceField as Opus.Core.Location;
                            var absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Source file '{0}' does not exist", absolutePath);
                            }

                            sourceFiles.Add(absolutePath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            var sourceCollection = sourceField as Opus.Core.FileCollection;
                            // TODO: convert to var
                            foreach (Opus.Core.Location location in sourceCollection)
                            {
                                var absolutePath = location.AbsolutePath;
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception("Source file '{0}' does not exist", absolutePath);
                                }

                                sourceFiles.Add(absolutePath);
                            }
                        }
                        else
                        {
                            throw new Opus.Core.Exception("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }

                // WPF application definition .xaml file
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.ApplicationDefinitionAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Opus.Core.Location)
                        {
                            var file = sourceField as Opus.Core.Location;
                            var absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                            }

                            var csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Opus.Core.Exception("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath);
                            }

                            sourceFiles.Add(csPath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            var sourceCollection = sourceField as Opus.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Opus.Core.Exception("There can be only one application definition");
                            }

                            // TODO: convert to var
                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                                }

                                var csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Opus.Core.Exception("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath);
                                }

                                sourceFiles.Add(csPath);
                            }
                        }
                        else
                        {
                            throw new Opus.Core.Exception("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }

                // WPF page .xaml files
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.PagesAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Opus.Core.Location)
                        {
                            var file = sourceField as Opus.Core.Location;
                            var absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Page file '{0}' does not exist", absolutePath);
                            }

                            var csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Opus.Core.Exception("Associated source file '{0}' to page file '{1}' does not exist", csPath, absolutePath);
                            }

                            sourceFiles.Add(csPath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            var sourceCollection = sourceField as Opus.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Opus.Core.Exception("There can be only one page file");
                            }

                            // TODO: convert to var
                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception("Page file '{0}' does not exist", absolutePath);
                                }

                                var csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Opus.Core.Exception("Associated source file '{0}' to page file '{1}' does not exist", csPath, absolutePath);
                                }

                                sourceFiles.Add(csPath);
                            }
                        }
                        else
                        {
                            throw new Opus.Core.Exception("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString());
                        }
                    }
                }
            }

            if (0 == sourceFiles.Count)
            {
                throw new Opus.Core.Exception("There were no source files specified for the module '{0}'", node.ModuleName);
            }

            // dependency checking
            {
#if true
                var inputLocations = new Opus.Core.LocationArray();
                foreach (var source in sourceFiles)
                {
                    inputLocations.Add(Opus.Core.FileLocation.Get(source));
                }
                var outputLocations = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.File, Opus.Core.Location.EExists.WillExist);
                if (!RequiresBuilding(outputLocations, inputLocations))
#else
                Opus.Core.StringArray outputFiles = assemblyOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, sourceFiles))
#endif
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            var commandLineBuilder = new Opus.Core.StringArray();
            if (options is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
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

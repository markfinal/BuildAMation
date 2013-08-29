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
            Opus.Core.BaseModule assemblyModule = moduleToBuild as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = assemblyModule.OwningNode;
            Opus.Core.Target target = node.Target;
            Opus.Core.BaseOptionCollection assemblyOptions = assemblyModule.Options;
            CSharp.IOptions options = assemblyOptions as CSharp.IOptions;

            if (node.ExternalDependents != null)
            {
                Opus.Core.StringArray dependentAssemblies = new Opus.Core.StringArray();
                node.ExternalDependents.FilterOutputPaths(CSharp.OutputFileFlags.AssemblyFile, dependentAssemblies);

                options.References.AddRange(dependentAssemblies);
            }

            Opus.Core.StringArray sourceFiles = new Opus.Core.StringArray();
            var fields = moduleToBuild.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // C# files
                {
                    var sourceFileAttributes = field.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                    if (null != sourceFileAttributes && sourceFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(moduleToBuild);
                        if (sourceField is Opus.Core.File)
                        {
                            Opus.Core.File file = sourceField as Opus.Core.File;
                            string absolutePath = file.AbsolutePath;
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Source file '{0}' does not exist", absolutePath);
                            }

                            sourceFiles.Add(absolutePath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            Opus.Core.FileCollection sourceCollection = sourceField as Opus.Core.FileCollection;
                            foreach (string absolutePath in sourceCollection)
                            {
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
                        if (sourceField is Opus.Core.File)
                        {
                            Opus.Core.File file = sourceField as Opus.Core.File;
                            string absolutePath = file.AbsolutePath;
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                            }

                            string csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Opus.Core.Exception("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath);
                            }

                            sourceFiles.Add(csPath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            Opus.Core.FileCollection sourceCollection = sourceField as Opus.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Opus.Core.Exception("There can be only one application definition");
                            }

                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception("Application definition file '{0}' does not exist", absolutePath);
                                }

                                string csPath = absolutePath + ".cs";
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
                        if (sourceField is Opus.Core.File)
                        {
                            Opus.Core.File file = sourceField as Opus.Core.File;
                            string absolutePath = file.AbsolutePath;
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Page file '{0}' does not exist", absolutePath);
                            }

                            string csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Opus.Core.Exception("Associated source file '{0}' to page file '{1}' does not exist", csPath, absolutePath);
                            }

                            sourceFiles.Add(csPath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            Opus.Core.FileCollection sourceCollection = sourceField as Opus.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Opus.Core.Exception("There can be only one page file");
                            }

                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception("Page file '{0}' does not exist", absolutePath);
                                }

                                string csPath = absolutePath + ".cs";
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
                Opus.Core.StringArray outputFiles = assemblyOptions.OutputPaths.Paths;
                if (!RequiresBuilding(outputFiles, sourceFiles))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            var commandLineBuilder = new Opus.Core.StringArray();
            if (options is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = options as CommandLineProcessor.ICommandLineSupport;
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

            foreach (string source in sourceFiles)
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

            Opus.Core.ITool compilerTool = target.Toolset.Tool(typeof(CSharp.ICSharpCompilerTool));

            int exitCode = CommandLineProcessor.Processor.Execute(node, compilerTool, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}

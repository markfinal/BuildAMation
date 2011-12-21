// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public partial class NativeBuilder
    {
        public object Build(CSharp.Assembly assembly, out System.Boolean success)
        {
            Opus.Core.DependencyNode node = assembly.OwningNode;
            Opus.Core.Target target = node.Target;
            CSharp.OptionCollection options = assembly.Options as CSharp.OptionCollection;

            if (node.ExternalDependents != null)
            {
                Opus.Core.StringArray dependentAssemblies = new Opus.Core.StringArray();
                node.ExternalDependents.FilterOutputPaths(CSharp.OutputFileFlags.AssemblyFile, dependentAssemblies);

                options.References.AddRange(dependentAssemblies);
            }

            Opus.Core.StringArray sourceFiles = new Opus.Core.StringArray();
            var fields = assembly.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                // C# files
                {
                    var sourceFileAttributes = field.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                    if (null != sourceFileAttributes && sourceFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(assembly);
                        if (sourceField is Opus.Core.File)
                        {
                            Opus.Core.File file = sourceField as Opus.Core.File;
                            if (!file.IsValid)
                            {
                                Opus.Core.Log.DebugMessage("Field '{0}' has an invalid path set", field.Name);
                                continue;
                            }

                            string absolutePath = file.AbsolutePath;
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception(System.String.Format("Source file '{0}' does not exist", absolutePath), false);
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
                                    throw new Opus.Core.Exception(System.String.Format("Source file '{0}' does not exist", absolutePath), false);
                                }

                                sourceFiles.Add(absolutePath);
                            }
                        }
                        else
                        {
                            throw new Opus.Core.Exception(System.String.Format("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString()), false);
                        }
                    }
                }

                // WPF application definition .xaml file
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.ApplicationDefinitionAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(assembly);
                        if (sourceField is Opus.Core.File)
                        {
                            Opus.Core.File file = sourceField as Opus.Core.File;
                            if (!file.IsValid)
                            {
                                Opus.Core.Log.DebugMessage("Field '{0}' has an invalid path set", field.Name);
                                continue;
                            }

                            string absolutePath = file.AbsolutePath;
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception(System.String.Format("Application definition file '{0}' does not exist", absolutePath), false);
                            }

                            string csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Opus.Core.Exception(System.String.Format("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath), false);
                            }

                            sourceFiles.Add(csPath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            Opus.Core.FileCollection sourceCollection = sourceField as Opus.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Opus.Core.Exception("There can be only one application definition", false);
                            }

                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception(System.String.Format("Application definition file '{0}' does not exist", absolutePath), false);
                                }

                                string csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Opus.Core.Exception(System.String.Format("Associated source file '{0}' to application definition file '{1}' does not exist", csPath, absolutePath), false);
                                }

                                sourceFiles.Add(csPath);
                            }
                        }
                        else
                        {
                            throw new Opus.Core.Exception(System.String.Format("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString()), false);
                        }
                    }
                }

                // WPF page .xaml files
                {
                    var xamlFileAttributes = field.GetCustomAttributes(typeof(CSharp.PagesAttribute), false);
                    if (null != xamlFileAttributes && xamlFileAttributes.Length > 0)
                    {
                        var sourceField = field.GetValue(assembly);
                        if (sourceField is Opus.Core.File)
                        {
                            Opus.Core.File file = sourceField as Opus.Core.File;
                            if (!file.IsValid)
                            {
                                Opus.Core.Log.DebugMessage("Field '{0}' has an invalid path set", field.Name);
                                continue;
                            }

                            string absolutePath = file.AbsolutePath;
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception(System.String.Format("Page file '{0}' does not exist", absolutePath), false);
                            }

                            string csPath = absolutePath + ".cs";
                            if (!System.IO.File.Exists(csPath))
                            {
                                throw new Opus.Core.Exception(System.String.Format("Associated source file '{0}' to page file '{1}' does not exist", csPath, absolutePath), false);
                            }

                            sourceFiles.Add(csPath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            Opus.Core.FileCollection sourceCollection = sourceField as Opus.Core.FileCollection;
                            if (sourceCollection.Count != 1)
                            {
                                throw new Opus.Core.Exception("There can be only one page file", false);
                            }

                            foreach (string absolutePath in sourceCollection)
                            {
                                if (!System.IO.File.Exists(absolutePath))
                                {
                                    throw new Opus.Core.Exception(System.String.Format("Page file '{0}' does not exist", absolutePath), false);
                                }

                                string csPath = absolutePath + ".cs";
                                if (!System.IO.File.Exists(csPath))
                                {
                                    throw new Opus.Core.Exception(System.String.Format("Associated source file '{0}' to page file '{1}' does not exist", csPath, absolutePath), false);
                                }

                                sourceFiles.Add(csPath);
                            }
                        }
                        else
                        {
                            throw new Opus.Core.Exception(System.String.Format("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString()), false);
                        }
                    }
                }
            }

            if (0 == sourceFiles.Count)
            {
                throw new Opus.Core.Exception(System.String.Format("There were no source files specified for the module '{0}'", node.ModuleName), false);
            }

            Opus.Core.StringArray outputFiles = assembly.Options.OutputPaths.Paths;
            if (!RequiresBuilding(outputFiles, sourceFiles))
            {
                Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                success = true;
                return null;
            }

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            if (options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = options as CommandLineProcessor.ICommandLineSupport;
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

            CSharp.Csc compilerInstance = CSharp.CscFactory.GetTargetInstance(target);
            string executablePath = compilerInstance.Executable(target);
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;

            int exitCode = CommandLineProcessor.Processor.Execute(node, compilerTool, executablePath, commandLineBuilder);
            success = (0 == exitCode);

            return null;
        }
    }
}

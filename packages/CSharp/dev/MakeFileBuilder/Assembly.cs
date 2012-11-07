// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object Build(CSharp.Assembly assembly, out System.Boolean success)
        {
            Opus.Core.IModule assemblyModule = assembly as Opus.Core.IModule;
            Opus.Core.DependencyNode node = assemblyModule.OwningNode;
            Opus.Core.Target target = node.Target;
            Opus.Core.BaseOptionCollection assemblyOptions = assemblyModule.Options;
            CSharp.OptionCollection options = assemblyOptions as CSharp.OptionCollection;

            MakeFileVariableDictionary inputVariables = new MakeFileVariableDictionary();
            System.Collections.Generic.List<MakeFileData> dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (node.ExternalDependents != null)
            {
                foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    if (null != dependentNode.Data)
                    {
                        Opus.Core.StringArray assemblyPaths = new Opus.Core.StringArray();
                        dependentNode.FilterOutputPaths(CSharp.OutputFileFlags.AssemblyFile, assemblyPaths);
                        (options as CSharp.IOptions).References.AddRange(assemblyPaths);

                        MakeFileData data = dependentNode.Data as MakeFileData;
                        inputVariables.Add(CSharp.OutputFileFlags.AssemblyFile, data.VariableDictionary[CSharp.OutputFileFlags.AssemblyFile]);
                        dataArray.Add(data);
                    }
                }
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

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                directoriesToCreate = commandLineOption.DirectoriesToCreate();
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

            // NEW STYLE
#if true
            Opus.Core.IToolset toolset = target.Toolset;
            Opus.Core.ITool compilerInstance = toolset.Tool(typeof(CSharp.ICSharpCompilerTool));
#else
            CSharp.Csc compilerInstance = CSharp.CscFactory.GetTargetInstance(target);
#endif
            string executablePath = compilerInstance.Executable(target);

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            if (executablePath.Contains(" "))
            {
                recipes.Add(System.String.Format("\"{0}\" {1}", executablePath, commandLineBuilder.ToString(' ')));
            }
            else
            {
                recipes.Add(System.String.Format("{0} {1}", executablePath, commandLineBuilder.ToString(' ')));
            }

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            MakeFileRule rule = new MakeFileRule(assemblyOptions.OutputPaths, CSharp.OutputFileFlags.AssemblyFile, node.UniqueModuleName, directoriesToCreate, inputVariables, null, recipes);
            makeFile.RuleArray.Add(rule);

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePath);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            success = true;
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;
            Opus.Core.StringArray environmentPaths = null;
            if (compilerTool is Opus.Core.IToolEnvironmentPaths)
            {
                environmentPaths = (compilerTool as Opus.Core.IToolEnvironmentPaths).Paths(target);
            }
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (compilerTool is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (compilerTool as Opus.Core.IToolEnvironmentVariables).Variables(target);
            }
            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, environmentPaths, environment);
            return returnData;
        }
    }
}

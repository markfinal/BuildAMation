// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object Build(CSharp.Assembly moduleToBuild, out System.Boolean success)
        {
            Opus.Core.BaseModule assemblyModule = moduleToBuild as Opus.Core.BaseModule;
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
                            string absolutePath = file.GetSinglePath();
                            if (!System.IO.File.Exists(absolutePath))
                            {
                                throw new Opus.Core.Exception("Source file '{0}' does not exist", absolutePath);
                            }

                            sourceFiles.Add(absolutePath);
                        }
                        else if (sourceField is Opus.Core.FileCollection)
                        {
                            Opus.Core.FileCollection sourceCollection = sourceField as Opus.Core.FileCollection;
                            foreach (Opus.Core.Location location in sourceCollection)
                            {
                                string absolutePath = location.GetSinglePath();
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
                            string absolutePath = file.GetSinglePath();
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
                        if (sourceField is Opus.Core.Location)
                        {
                            var file = sourceField as Opus.Core.Location;
                            string absolutePath = file.GetSinglePath();
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

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);

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

            Opus.Core.ITool compilerInstance = target.Toolset.Tool(typeof(CSharp.ICSharpCompilerTool));
            string executablePath = compilerInstance.Executable((Opus.Core.BaseTarget)target);

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
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (compilerTool is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (compilerTool as Opus.Core.IToolEnvironmentVariables).Variables((Opus.Core.BaseTarget)target);
            }
            MakeFileData returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, environment);
            return returnData;
        }
    }
}

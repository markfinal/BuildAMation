// <copyright file="Assembly.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>CSharp package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object Build(CSharp.Assembly assembly, Opus.Core.DependencyNode node, out System.Boolean success)
        {
            Opus.Core.Target target = node.Target;
            CSharp.OptionCollection options = assembly.Options as CSharp.OptionCollection;

            Opus.Core.StringArray inputVariables = new Opus.Core.StringArray();
            System.Collections.Generic.List<MakeFileData> dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (node.ExternalDependents != null)
            {
                foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    if (null != dependentNode.Data)
                    {
                        Opus.Core.StringArray assemblyPaths = new Opus.Core.StringArray();
                        dependentNode.FilterOutputPaths(CSharp.OutputFileFlags.AssemblyFile, assemblyPaths);
                        options.References.AddRange(assemblyPaths);

                        MakeFileData data = dependentNode.Data as MakeFileData;
                        inputVariables.Add(data.Variable);
                        dataArray.Add(data);
                    }
                }
            }

            Opus.Core.StringArray sourceFiles = new Opus.Core.StringArray();
            var fields = assembly.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                var sourceFileAttributes = field.GetCustomAttributes(typeof(Opus.Core.SourceFilesAttribute), false);
                if (null != sourceFileAttributes && sourceFileAttributes.Length > 0)
                {
                    var sourceField = field.GetValue(assembly);
                    if (sourceField is Opus.Core.File)
                    {
                        string absolutePath = (sourceField as Opus.Core.File).AbsolutePath;
                        if (!System.IO.File.Exists(absolutePath))
                        {
                            throw new Opus.Core.Exception(System.String.Format("Source file '{0}' does not exist", absolutePath), false);
                        }

                        if (Opus.Core.OSUtilities.IsWindowsHosting)
                        {
                            // TODO: Win32 csc is fussy about directory separators
                            // remove this when paths are constructed better
                            absolutePath = absolutePath.Replace('/', '\\');
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

                            string correctedPath = absolutePath;
                            if (Opus.Core.OSUtilities.IsWindowsHosting)
                            {
                                // TODO: Win32 csc is fussy about directory separators
                                // remove this when paths are constructed better
                                correctedPath = correctedPath.Replace('/', '\\');
                            }
                            sourceFiles.Add(correctedPath);
                        }
                    }
                    else
                    {
                        throw new Opus.Core.Exception(System.String.Format("Field '{0}' of '{1}' should be of type Opus.Core.File or Opus.Core.FileCollection, not '{2}'", field.Name, node.ModuleName, sourceField.GetType().ToString()), false);
                    }
                }
            }

            if (0 == sourceFiles.Count)
            {
                throw new Opus.Core.Exception(System.String.Format("There were no source files specified for the module '{0}'", node.ModuleName), false);
            }

            System.Text.StringBuilder commandLineBuilder = new System.Text.StringBuilder();
            if (options is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
            }

            foreach (string source in sourceFiles)
            {
                commandLineBuilder.AppendFormat("\"{0}\" ", source);
            }

            CSharp.Csc compilerInstance = CSharp.CscFactory.GetTargetInstance(target);
            string executablePath = compilerInstance.Executable(target);

            Opus.Core.StringArray commandLines = new Opus.Core.StringArray();
            commandLines.Add(System.String.Format("\"{0}\" {1}", executablePath, commandLineBuilder.ToString()));

            MakeFileBuilderRecipe recipe = new MakeFileBuilderRecipe(node, sourceFiles, inputVariables, commandLines, this.topLevelMakeFilePath);

            foreach (MakeFileData data in dataArray)
            {
                if (!data.Included)
                {
                    string relativeDataFile = Opus.Core.RelativePathUtilities.GetPath(data.File, this.topLevelMakeFilePath, "$(CURDIR)");
                    recipe.Includes.Add(relativeDataFile);
                    data.Included = true;
                }
            }

            string makeFile = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFile));
            Opus.Core.Log.DebugMessage("Makefile : '{0}'", makeFile);

            string makeFileTargetName = null;
            string makeFileVariableName = null;
            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFile))
            {
                recipe.Write(makeFileWriter, CSharp.OutputFileFlags.AssemblyFile);
                makeFileTargetName = recipe.TargetName;
                makeFileVariableName = recipe.VariableName;
            }

            success = true;
            Opus.Core.ITool compilerTool = compilerInstance as Opus.Core.ITool;
            MakeFileData returnData = new MakeFileData(makeFile, makeFileTargetName, makeFileVariableName, compilerTool.EnvironmentPaths(target));
            return returnData;
        }
    }
}

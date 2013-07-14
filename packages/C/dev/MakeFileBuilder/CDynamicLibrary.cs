// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.DynamicLibrary moduleToBuild, out bool success)
        {
            var dynamicLibraryModule = moduleToBuild as Opus.Core.BaseModule;
            var node = dynamicLibraryModule.OwningNode;
            var target = node.Target;

            // dependents
            var inputVariables = new MakeFileVariableDictionary();
            var dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (null != node.Children)
            {
                foreach (var childNode in node.Children)
                {
                    if (null != childNode.Data)
                    {
                        var data = childNode.Data as MakeFileData;
                        inputVariables.Append(data.VariableDictionary);
                        dataArray.Add(data);
                    }
                }
            }
            if (null != node.ExternalDependents)
            {
                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (null != dependentNode.Data)
                    {
                        var data = dependentNode.Data as MakeFileData;
                        inputVariables.Append(data.VariableDictionary);
                        dataArray.Add(data);
                    }
                }
            }

            var dynamicLibraryOptions = dynamicLibraryModule.Options;

            var commandLineBuilder = new Opus.Core.StringArray();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (dynamicLibraryOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = dynamicLibraryOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);

                directoriesToCreate = commandLineOption.DirectoriesToCreate();
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            var toolset = target.Toolset;
            var linkerTool = toolset.Tool(typeof(C.ILinkerTool)) as C.ILinkerTool;
            var executable = linkerTool.Executable((Opus.Core.BaseTarget)target);

            var recipeBuilder = new System.Text.StringBuilder();
            if (executable.Contains(" "))
            {
                recipeBuilder.AppendFormat("\"{0}\"", executable);
            }
            else
            {
                recipeBuilder.Append(executable);
            }

            var dependentLibraryCommandLine = new Opus.Core.StringArray();
            {
                var compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;

                recipeBuilder.AppendFormat(" {0} $(filter %{1},$^) ", commandLineBuilder.ToString(' '), compilerTool.ObjectFileSuffix);

                if (toolset.HasTool(typeof(C.IWinResourceCompilerTool)))
                {
                    var winResourceCompilerTool = toolset.Tool(typeof(C.IWinResourceCompilerTool)) as C.IWinResourceCompilerTool;
                    recipeBuilder.AppendFormat("$(filter %{0},$^) ", winResourceCompilerTool.CompiledResourceSuffix);
                }

                // TODO: don't want to access the archiver tool here really, as creating
                // an application does not require one
                // although we do need to know where static libraries are written
                // perhaps the ILinkerTool can have a duplicate of the static library suffix?
                var archiverTool = toolset.Tool(typeof(C.IArchiverTool)) as C.IArchiverTool;

                var dependentLibraries = new Opus.Core.StringArray();
                dependentLibraries.Add(System.String.Format("$(filter %{0},$^)", archiverTool.StaticLibrarySuffix));
                if (linkerTool is C.IWinImportLibrary)
                {
                    dependentLibraries.Add(System.String.Format("$(filter %{0},$^)", (linkerTool as C.IWinImportLibrary).ImportLibrarySuffix));
                }
                C.LinkerUtilities.AppendLibrariesToCommandLine(dependentLibraryCommandLine, linkerTool, dynamicLibraryOptions as C.ILinkerOptions, dependentLibraries);
            }

            recipeBuilder.Append(dependentLibraryCommandLine.ToString(' '));
            var recipe = recipeBuilder.ToString();
            // replace primary target with $@
            var primaryOutput = C.OutputFileFlags.Executable;
            recipe = recipe.Replace(dynamicLibraryOptions.OutputPaths[primaryOutput], "$@");
            var instanceName = MakeFile.InstanceName(node);
            // TODO: due to the foreach causing this exception in Mono
            // '(System.InvalidCastException) Cannot cast from source type to destination type.'
            if (Opus.Core.State.RunningMono)
            {
                foreach (System.Enum key in dynamicLibraryOptions.OutputPaths.Types)
                {
                    if (!key.Equals(primaryOutput))
                    {
                        var variableName = System.String.Format("{0}_{1}_Variable", instanceName, key.ToString());
                        recipe = recipe.Replace(dynamicLibraryOptions.OutputPaths[key], System.String.Format("$({0})", variableName));
                    }
                }
            }
            else
            {
                foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> outputPath in dynamicLibraryOptions.OutputPaths)
                {
                    if (!outputPath.Key.Equals(primaryOutput))
                    {
                        var variableName = System.String.Format("{0}_{1}_Variable", instanceName, outputPath.Key.ToString());
                        recipe = recipe.Replace(dynamicLibraryOptions.OutputPaths[outputPath.Key], System.String.Format("$({0})", variableName));
                    }
                }
            }

            var recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(dynamicLibraryOptions.OutputPaths, C.OutputFileFlags.Executable, node.UniqueModuleName, directoriesToCreate, inputVariables, null, recipes);
            makeFile.RuleArray.Add(rule);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var exportedTargets = makeFile.ExportedTargets;
            var exportedVariables = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (linkerTool is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (linkerTool as Opus.Core.IToolEnvironmentVariables).Variables((Opus.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, exportedTargets, exportedVariables, environment);
            success = true;
            return returnData;
        }
    }
}
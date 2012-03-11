// <copyright file="CDynamicLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.DynamicLibrary dynamicLibrary, out bool success)
        {
            Opus.Core.IModule dynamicLibraryModule = dynamicLibrary as Opus.Core.IModule;
            Opus.Core.DependencyNode node = dynamicLibraryModule.OwningNode;
            Opus.Core.Target target = node.Target;
            C.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target);
            C.Linker linkerInstance = C.LinkerFactory.GetTargetInstance(target);
            Opus.Core.ITool linkerTool = linkerInstance as Opus.Core.ITool;

            // dependents
            MakeFileVariableDictionary inputVariables = new MakeFileVariableDictionary();
            System.Collections.Generic.List<MakeFileData> dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (null != node.Children)
            {
                foreach (Opus.Core.DependencyNode childNode in node.Children)
                {
                    if (null != childNode.Data)
                    {
                        MakeFileData data = childNode.Data as MakeFileData;
                        inputVariables.Append(data.VariableDictionary);
                        dataArray.Add(data);
                    }
                }
            }
            if (null != node.ExternalDependents)
            {
                foreach (Opus.Core.DependencyNode dependentNode in node.ExternalDependents)
                {
                    if (null != dependentNode.Data)
                    {
                        MakeFileData data = dependentNode.Data as MakeFileData;
                        inputVariables.Append(data.VariableDictionary);
                        dataArray.Add(data);
                    }
                }
            }

            Opus.Core.BaseOptionCollection dynamicLibraryOptions = dynamicLibraryModule.Options;

            string executable;
            C.IToolchainOptions toolchainOptions = (dynamicLibraryOptions as C.ILinkerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executable = linkerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executable = linkerTool.Executable(target);
            }

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (dynamicLibraryOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = dynamicLibraryOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                directoriesToCreate = commandLineOption.DirectoriesToCreate();
            }
            else
            {
                throw new Opus.Core.Exception("Linker options does not support command line translation");
            }

            System.Text.StringBuilder recipeBuilder = new System.Text.StringBuilder();
            if (executable.Contains(" "))
            {
                recipeBuilder.AppendFormat("\"{0}\"", executable);
            }
            else
            {
                recipeBuilder.Append(executable);
            }
            recipeBuilder.AppendFormat(" {0} $(filter %{1},$^) ", commandLineBuilder.ToString(' '), toolchain.ObjectFileSuffix);
            Opus.Core.StringArray dependentLibraries = new Opus.Core.StringArray();
            dependentLibraries.Add(System.String.Format("$(filter %{0},$^)", toolchain.StaticLibrarySuffix));
            if (toolchain.StaticLibrarySuffix != toolchain.StaticImportLibrarySuffix)
            {
                dependentLibraries.Add(System.String.Format("$(filter %{0},$^)", toolchain.StaticImportLibrarySuffix));
            }
            Opus.Core.StringArray dependentLibraryCommandLine = new Opus.Core.StringArray();
            linkerInstance.AppendLibrariesToCommandLine(dependentLibraryCommandLine, dynamicLibraryOptions as C.ILinkerOptions, dependentLibraries);
            recipeBuilder.Append(dependentLibraryCommandLine.ToString(' '));
            string recipe = recipeBuilder.ToString();
            // replace primary target with $@
            C.OutputFileFlags primaryOutput = C.OutputFileFlags.Executable;
            recipe = recipe.Replace(dynamicLibraryOptions.OutputPaths[primaryOutput], "$@");
            string instanceName = MakeFile.InstanceName(node);
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> outputPath in dynamicLibraryOptions.OutputPaths)
            {
                if (!outputPath.Key.Equals(primaryOutput))
                {
                    string variableName = System.String.Format("{0}_{1}_Variable", instanceName, outputPath.Key.ToString());
                    recipe = recipe.Replace(dynamicLibraryOptions.OutputPaths[outputPath.Key], System.String.Format("$({0})", variableName));
                }
            }

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            MakeFileRule rule = new MakeFileRule(dynamicLibraryOptions.OutputPaths, C.OutputFileFlags.Executable, node.UniqueModuleName, directoriesToCreate, inputVariables, null, recipes);
            makeFile.RuleArray.Add(rule);

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            MakeFileTargetDictionary exportedTargets = makeFile.ExportedTargets;
            MakeFileVariableDictionary exportedVariables = makeFile.ExportedVariables;
            Opus.Core.StringArray environmentPaths = null;
            if (linkerTool is Opus.Core.IToolEnvironmentPaths)
            {
                environmentPaths = (linkerTool as Opus.Core.IToolEnvironmentPaths).Paths(target);
            }
            MakeFileData returnData = new MakeFileData(makeFilePath, exportedTargets, exportedVariables, environmentPaths);
            success = true;
            return returnData;
        }
    }
}
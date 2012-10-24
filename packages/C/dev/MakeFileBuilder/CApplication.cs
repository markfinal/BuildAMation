// <copyright file="CApplication.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.Application application, out bool success)
        {
            Opus.Core.IModule applicationModule = application as Opus.Core.IModule;
            Opus.Core.DependencyNode node = applicationModule.OwningNode;
            Opus.Core.Target target = node.Target;
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

            Opus.Core.BaseOptionCollection applicationOptions = applicationModule.Options;
            
            // NEW STYLE
#if true
            string executable = linkerTool.Executable(target);
#else
            string executable;
            C.IToolchainOptions toolchainOptions = (applicationOptions as C.ILinkerOptions).ToolchainOptionCollection as C.IToolchainOptions;
            if (toolchainOptions.IsCPlusPlus)
            {
                executable = linkerInstance.ExecutableCPlusPlus(target);
            }
            else
            {
                executable = linkerTool.Executable(target);
            }
#endif

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (applicationOptions is CommandLineProcessor.ICommandLineSupport)
            {
                CommandLineProcessor.ICommandLineSupport commandLineOption = applicationOptions as CommandLineProcessor.ICommandLineSupport;
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

            // NEW STYLE
#if true
            Opus.Core.IToolsetInfo toolsetInfo = Opus.Core.State.Get("ToolsetInfo", target.Toolchain) as Opus.Core.IToolsetInfo;
            if (null == toolsetInfo)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information for '{0}' is missing", target.Toolchain), false);
            }

            C.ICompilerInfo compilerInfo = toolsetInfo as C.ICompilerInfo;
            if (null == compilerInfo)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information '{0}' does not implement the '{1}' interface for toolchain '{2}'", toolsetInfo.GetType().ToString(), typeof(C.ICompilerInfo).ToString(), target.Toolchain), false);
            }

            recipeBuilder.AppendFormat(" {0} $(filter %{1},$^) ", commandLineBuilder.ToString(' '), compilerInfo.ObjectFileSuffix);
            if (toolsetInfo is C.IWinResourceCompilerInfo)
            {
                C.IWinResourceCompilerInfo win32ResourceCompilerInfo = toolsetInfo as C.IWinResourceCompilerInfo;
                recipeBuilder.AppendFormat("$(filter %{0},$^) ", win32ResourceCompilerInfo.CompiledResourceSuffix);
            }

            C.IArchiverInfo archiverInfo = toolsetInfo as C.IArchiverInfo;
            if (null == archiverInfo)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information '{0}' does not implement the '{1}' interface for toolchain '{2}'", toolsetInfo.GetType().ToString(), typeof(C.IArchiverInfo).ToString(), target.Toolchain), false);
            }

            C.ILinkerInfo linkerInfo = toolsetInfo as C.ILinkerInfo;
            if (null == linkerInfo)
            {
                throw new Opus.Core.Exception(System.String.Format("Toolset information '{0}' does not implement the '{1}' interface for toolchain '{2}'", toolsetInfo.GetType().ToString(), typeof(C.ILinkerInfo).ToString(), target.Toolchain), false);
            }

            Opus.Core.StringArray dependentLibraries = new Opus.Core.StringArray();
            dependentLibraries.Add(System.String.Format("$(filter %{0},$^)", archiverInfo.StaticLibrarySuffix));
            // TODO: ratify that import libraries are a Windows only creation
            if (archiverInfo.StaticLibrarySuffix != linkerInfo.ImportLibrarySuffix)
            {
                dependentLibraries.Add(System.String.Format("$(filter %{0},$^)", linkerInfo.ImportLibrarySuffix));
            }
#else
            C.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target);
            recipeBuilder.AppendFormat(" {0} $(filter %{1},$^) ", commandLineBuilder.ToString(' '), toolchain.ObjectFileSuffix);
            if (toolchain.Win32CompiledResourceSuffix.Length > 0)
            {
                recipeBuilder.AppendFormat("$(filter %{0},$^) ", toolchain.Win32CompiledResourceSuffix);
            }
            Opus.Core.StringArray dependentLibraries = new Opus.Core.StringArray();
            dependentLibraries.Add(System.String.Format("$(filter %{0},$^)", toolchain.StaticLibrarySuffix));
            if (toolchain.StaticLibrarySuffix != toolchain.StaticImportLibrarySuffix)
            {
                dependentLibraries.Add(System.String.Format("$(filter %{0},$^)", toolchain.StaticImportLibrarySuffix));
            }
#endif
            Opus.Core.StringArray dependentLibraryCommandLine = new Opus.Core.StringArray();
            linkerInstance.AppendLibrariesToCommandLine(dependentLibraryCommandLine, applicationOptions as C.ILinkerOptions, dependentLibraries);
            recipeBuilder.Append(dependentLibraryCommandLine.ToString(' '));
            string recipe = recipeBuilder.ToString();
            // replace primary target with $@
            C.OutputFileFlags primaryOutput = C.OutputFileFlags.Executable;
            recipe = recipe.Replace(applicationOptions.OutputPaths[primaryOutput], "$@");
            string instanceName = MakeFile.InstanceName(node);
            foreach (System.Collections.Generic.KeyValuePair<System.Enum, string> outputPath in applicationOptions.OutputPaths)
            {
                if (!outputPath.Key.Equals(primaryOutput))
                {
                    string variableName = System.String.Format("{0}_{1}_Variable", instanceName, outputPath.Key.ToString());
                    recipe = recipe.Replace(applicationOptions.OutputPaths[outputPath.Key], System.String.Format("$({0})", variableName));
                }
            }

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            MakeFileRule rule = new MakeFileRule(applicationOptions.OutputPaths, primaryOutput, node.UniqueModuleName, directoriesToCreate, inputVariables, null, recipes);
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
// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, out bool success)
        {
            Opus.Core.BaseModule staticLibraryModule = staticLibrary as Opus.Core.BaseModule;
            Opus.Core.DependencyNode node = staticLibraryModule.OwningNode;
            Opus.Core.Target target = node.Target;

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

            Opus.Core.BaseOptionCollection staticLibraryOptions = staticLibraryModule.Options;

            Opus.Core.IToolset toolset = target.Toolset;
            Opus.Core.ITool archiverTool = toolset.Tool(typeof(C.IArchiverTool));
            string executable = archiverTool.Executable((Opus.Core.BaseTarget)target);

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (staticLibraryOptions is CommandLineProcessor.ICommandLineSupport)
            {
                // TODO: pass in a map of path translations, e.g. outputfile > $@
                CommandLineProcessor.ICommandLineSupport commandLineOption = staticLibraryOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                directoriesToCreate = commandLineOption.DirectoriesToCreate();
            }
            else
            {
                throw new Opus.Core.Exception("Archiver options does not support command line translation");
            }

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            string recipe = null;
            if (executable.Contains(" "))
            {
                recipe += System.String.Format("\"{0}\"", executable);
            }
            else
            {
                recipe += executable;
            }

            {
                C.ICompilerTool compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;

                recipe += System.String.Format(" {0} $(filter %{1},$^)", commandLineBuilder.ToString(' '), compilerTool.ObjectFileSuffix);
            }

            // replace primary target with $@
            C.OutputFileFlags primaryOutput = C.OutputFileFlags.StaticLibrary;
            recipe = recipe.Replace(staticLibraryOptions.OutputPaths[primaryOutput], "$@");

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            MakeFileRule rule = new MakeFileRule(staticLibraryOptions.OutputPaths, primaryOutput, node.UniqueModuleName, directoriesToCreate, inputVariables, null, recipes);
            makeFile.RuleArray.Add(rule);

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            MakeFileTargetDictionary exportedTargetDictionary = makeFile.ExportedTargets;
            MakeFileVariableDictionary exportedVariableDictionary = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Opus.Core.StringArray> environment = null;
            if (archiverTool is Opus.Core.IToolEnvironmentVariables)
            {
                environment = (archiverTool as Opus.Core.IToolEnvironmentVariables).Variables(target);
            }
            MakeFileData returnData = new MakeFileData(makeFilePath, exportedTargetDictionary, exportedVariableDictionary, environment);
            success = true;
            return returnData;
        }
    }
}
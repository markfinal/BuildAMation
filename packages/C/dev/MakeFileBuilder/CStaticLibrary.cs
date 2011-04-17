// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(C.StaticLibrary staticLibrary, Opus.Core.DependencyNode node, out bool success)
        {
            Opus.Core.Target target = node.Target;
            C.Toolchain toolchain = C.ToolchainFactory.GetTargetInstance(target);
            C.Archiver archiverInstance = C.ArchiverFactory.GetTargetInstance(target);
            Opus.Core.ITool archiverTool = archiverInstance as Opus.Core.ITool;

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

            string executable = archiverTool.Executable(target);

            Opus.Core.StringArray commandLineBuilder = new Opus.Core.StringArray();
            Opus.Core.DirectoryCollection directoriesToCreate = null;
            if (staticLibrary.Options is CommandLineProcessor.ICommandLineSupport)
            {
                // TODO: pass in a map of path translations, e.g. outputfile > $@
                CommandLineProcessor.ICommandLineSupport commandLineOption = staticLibrary.Options as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target);

                directoriesToCreate = commandLineOption.DirectoriesToCreate();
            }
            else
            {
                throw new Opus.Core.Exception("Archiver options does not support command line translation");
            }

            MakeFile makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            string recipe = System.String.Format("\"{0}\" {1}$(filter %{2},$^)", executable, commandLineBuilder.ToString(' '), toolchain.ObjectFileExtension);
            // replace primary target with $@
            C.OutputFileFlags primaryOutput = C.OutputFileFlags.StaticLibrary;
            recipe = recipe.Replace(staticLibrary.Options.OutputPaths[primaryOutput], "$@");

            Opus.Core.StringArray recipes = new Opus.Core.StringArray();
            recipes.Add(recipe);

            MakeFileRule rule = new MakeFileRule(staticLibrary.Options.OutputPaths, primaryOutput, node.UniqueModuleName, directoriesToCreate, inputVariables, null, recipes);
            makeFile.RuleArray.Add(rule);

            string makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            success = true;
            MakeFileTargetDictionary exportedTargetDictionary = makeFile.ExportedTargets;
            MakeFileVariableDictionary exportedVariableDictionary = makeFile.ExportedVariables;
            MakeFileData returnData = new MakeFileData(makeFilePath, exportedTargetDictionary, exportedVariableDictionary, archiverTool.EnvironmentPaths(target));
            return returnData;
        }
    }
}
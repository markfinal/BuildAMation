// <copyright file="CStaticLibrary.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            C.StaticLibrary moduleToBuild,
            out bool success)
        {
            var staticLibraryModule = moduleToBuild as Bam.Core.BaseModule;
            var node = staticLibraryModule.OwningNode;
            var target = node.Target;

            // dependents
            var inputVariables = new MakeFileVariableDictionary();
            var dataArray = new System.Collections.Generic.List<MakeFileData>();
            if (null != node.Children)
            {
                var keysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.ObjectFile.OutputFile
                );

                foreach (var childNode in node.Children)
                {
                    if (null == childNode.Data)
                    {
                        continue;
                    }
                    var data = childNode.Data as MakeFileData;
                    inputVariables.Append(data.VariableDictionary.Filter(keysToFilter));
                    dataArray.Add(data);
                }
            }
            if (null != node.ExternalDependents)
            {
                var keysToFilter = new Bam.Core.Array<Bam.Core.LocationKey>(
                    C.ObjectFile.OutputFile
                );

                foreach (var dependentNode in node.ExternalDependents)
                {
                    if (null == dependentNode.Data)
                    {
                        continue;
                    }
                    var data = dependentNode.Data as MakeFileData;
                    inputVariables.Append(data.VariableDictionary.Filter(keysToFilter));
                    dataArray.Add(data);
                }
            }

            var staticLibraryOptions = staticLibraryModule.Options;

            var toolset = target.Toolset;
            var archiverTool = toolset.Tool(typeof(C.IArchiverTool));
            var executable = archiverTool.Executable((Bam.Core.BaseTarget)target);

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);

            var commandLineBuilder = new Bam.Core.StringArray();
            if (staticLibraryOptions is CommandLineProcessor.ICommandLineSupport)
            {
                // TODO: pass in a map of path translations, e.g. outputfile > $@
                var commandLineOption = staticLibraryOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Archiver options does not support command line translation");
            }

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

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
                var compilerTool = toolset.Tool(typeof(C.ICompilerTool)) as C.ICompilerTool;

                recipe += System.String.Format(" {0} $(filter %{1},$^)", commandLineBuilder.ToString(' '), compilerTool.ObjectFileSuffix);
            }

            // replace primary target with $@
            var outputPath = moduleToBuild.Locations[C.StaticLibrary.OutputFileLocKey].GetSinglePath();
            recipe = recipe.Replace(outputPath, "$@");

            var recipes = new Bam.Core.StringArray();
            recipes.Add(recipe);

            var rule = new MakeFileRule(
                moduleToBuild,
                C.StaticLibrary.OutputFileLocKey,
                node.UniqueModuleName,
                dirsToCreate,
                inputVariables,
                null,
                recipes);

            var toolOutputLocKeys = (archiverTool as Bam.Core.ITool).OutputLocationKeys(moduleToBuild);
            var outputFileLocations = moduleToBuild.Locations.Keys(Bam.Core.ScaffoldLocation.ETypeHint.File, Bam.Core.Location.EExists.WillExist);
            var outputFileLocationsOfInterest = outputFileLocations.Intersect(toolOutputLocKeys);
            rule.OutputLocationKeys = outputFileLocationsOfInterest;

            makeFile.RuleArray.Add(rule);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var exportedTargetDictionary = makeFile.ExportedTargets;
            var exportedVariableDictionary = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment = null;
            if (archiverTool is Bam.Core.IToolEnvironmentVariables)
            {
                environment = (archiverTool as Bam.Core.IToolEnvironmentVariables).Variables((Bam.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, exportedTargetDictionary, exportedVariableDictionary, environment);
            success = true;
            return returnData;
        }
    }
}

#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace C
{
namespace V2
{
    public sealed class MakeFileLibrarian :
        ILibrarianPolicy
    {
        void
        ILibrarianPolicy.Archive(
            StaticLibrary sender,
            string libraryPath,
            System.Collections.ObjectModel.ReadOnlyCollection<Bam.Core.V2.Module> inputs)
        {
            var interfaceType = Bam.Core.State.ScriptAssembly.GetType("CommandLineProcessor.V2.IConvertToCommandLine");
            if (interfaceType.IsAssignableFrom(sender.Settings.GetType()))
            {
                var map = sender.Settings.GetType().GetInterfaceMap(interfaceType);
                map.InterfaceMethods[0].Invoke(sender.Settings, new[] { sender });
            }

            var meta = new MakeFileBuilder.V2.MakeFileMeta(sender);
            meta.Target = libraryPath;
            foreach (var module in inputs)
            {
                if (module is Bam.Core.V2.IModuleGroup)
                {
                    foreach (var child in module.Children)
                    {
                        meta.Prequisities.Add(child, C.V2.ObjectFile.Key);
                    }
                }
                else
                {
                    meta.Prequisities.Add(module, C.V2.ObjectFile.Key);
                }
            }
            meta.Rules.Add(sender.CommandLine.ToString(' '));
            sender.MetaData = meta;
        }
    }
}
}
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

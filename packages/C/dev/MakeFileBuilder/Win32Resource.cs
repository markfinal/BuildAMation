#region License
// Copyright 2010-2014 Mark Final
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
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            C.Win32Resource moduleToBuild,
            out bool success)
        {
            var resourceFilePath = moduleToBuild.ResourceFileLocation.GetSinglePath();
            if (!System.IO.File.Exists(resourceFilePath))
            {
                throw new Bam.Core.Exception("Resource file '{0}' does not exist", resourceFilePath);
            }

            var inputFiles = new Bam.Core.StringArray();
            inputFiles.Add(resourceFilePath);

            var resourceFileModule = moduleToBuild as Bam.Core.BaseModule;
            var resourceFileOptions = resourceFileModule.Options;

            var compilerOptions = resourceFileOptions as C.Win32ResourceCompilerOptionCollection;

            var node = resourceFileModule.OwningNode;
            var target = node.Target;

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);

            var commandLineBuilder = new Bam.Core.StringArray();
            if (compilerOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = compilerOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            var toolset = target.Toolset;
            var compilerTool = toolset.Tool(typeof(C.IWinResourceCompilerTool)) as C.IWinResourceCompilerTool;

            // add output path
            var outputPath = moduleToBuild.Locations[C.Win32Resource.OutputFile].GetSinglePath();
            commandLineBuilder.Add(System.String.Format("{0}{1}",
                                                        compilerTool.OutputFileSwitch,
                                                        outputPath));

            var executablePath = compilerTool.Executable((Bam.Core.BaseTarget)target);

            string recipe = null;
            if (executablePath.Contains(" "))
            {
                recipe += System.String.Format("\"{0}\"", executablePath);
            }
            else
            {
                recipe += executablePath;
            }
            recipe += System.String.Format(" {0} $<", commandLineBuilder.ToString(' '));
            // replace target with $@
            recipe = recipe.Replace(outputPath, "$@");

            var recipes = new Bam.Core.StringArray();
            recipes.Add(recipe);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                C.Win32Resource.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                inputFiles,recipes);
            rule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(C.Win32Resource.OutputFile);
            makeFile.RuleArray.Add(rule);

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var targetDictionary = makeFile.ExportedTargets;
            var variableDictionary = makeFile.ExportedVariables;
            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment = null;
            if (compilerTool is Bam.Core.IToolEnvironmentVariables)
            {
                environment = (compilerTool as Bam.Core.IToolEnvironmentVariables).Variables((Bam.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, targetDictionary, variableDictionary, environment);
            success = true;
            return returnData;
        }
    }
}

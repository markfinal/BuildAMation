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
        private static Bam.Core.StringArray
        MakeSymlinkRecipe(
            Bam.Core.StringArray commandLineBuilder,
            C.IPosixSharedLibrarySymlinksTool tool,
            C.PosixSharedLibrarySymlinks moduleToBuild,
            string workingDirectory,
            Bam.Core.LocationKey keyToSymlink)
        {
            var recipeBuilder = new System.Text.StringBuilder();
            recipeBuilder.AppendFormat("{0} ", tool.Executable((Bam.Core.BaseTarget)moduleToBuild.OwningNode.Target));
            recipeBuilder.Append(commandLineBuilder.ToString());

            var symlinkFile = moduleToBuild.Locations[keyToSymlink];
            var symlinkFileLeafname = System.IO.Path.GetFileName(symlinkFile.GetSingleRawPath());
            recipeBuilder.AppendFormat(" {0}", symlinkFileLeafname);

            var recipe = recipeBuilder.ToString();

            var recipes = new Bam.Core.StringArray();
            recipes.Add(System.String.Format("cd {0} && {1}", workingDirectory, recipe));
            return recipes;
        }

        public object
        Build(
            C.PosixSharedLibrarySymlinks moduleToBuild,
            out bool success)
        {
            var realSharedLibraryLoc = moduleToBuild.RealSharedLibraryFileLocation;
            var realSharedLibraryPath = realSharedLibraryLoc.GetSingleRawPath();

            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);

            var node = moduleToBuild.OwningNode;

            var dependentVariables = new MakeFileVariableDictionary();
            foreach (var dependent in node.ExternalDependents)
            {
                var dependentData = dependent.Data as MakeFileData;
                if (null == dependentData)
                {
                    continue;
                }
                dependentVariables.Append(dependentData.VariableDictionary);
            }

            var target = moduleToBuild.OwningNode.Target;
            var creationOptions = moduleToBuild.Options as C.PosixSharedLibrarySymlinksOptionCollection;

            var commandLineBuilder = new Bam.Core.StringArray();
            if (creationOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = creationOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Compiler options does not support command line translation");
            }

            var symlinkTool = target.Toolset.Tool(typeof(C.IPosixSharedLibrarySymlinksTool)) as C.IPosixSharedLibrarySymlinksTool;
            var workingDir = moduleToBuild.Locations[C.PosixSharedLibrarySymlinks.OutputDir].GetSingleRawPath();

            commandLineBuilder.Add("-s");
            commandLineBuilder.Add("-f"); // TODO: temporary while dependency checking is not active
            var realSharedLibraryLeafname = System.IO.Path.GetFileName(realSharedLibraryPath);
            commandLineBuilder.Add(realSharedLibraryLeafname);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var majorVersionRule = new MakeFileRule(
                moduleToBuild,
                C.PosixSharedLibrarySymlinks.MajorVersionSymlink,
                node.UniqueModuleName,
                dirsToCreate,
                dependentVariables,
                null,
                MakeSymlinkRecipe(commandLineBuilder, symlinkTool, moduleToBuild, workingDir, C.PosixSharedLibrarySymlinks.MajorVersionSymlink));
            majorVersionRule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(C.PosixSharedLibrarySymlinks.MajorVersionSymlink);
            makeFile.RuleArray.Add(majorVersionRule);

            var minorVersionRule = new MakeFileRule(
                moduleToBuild,
                C.PosixSharedLibrarySymlinks.MinorVersionSymlink,
                node.UniqueModuleName,
                dirsToCreate,
                dependentVariables,
                null,
                MakeSymlinkRecipe(commandLineBuilder, symlinkTool, moduleToBuild, workingDir, C.PosixSharedLibrarySymlinks.MinorVersionSymlink));
            minorVersionRule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(C.PosixSharedLibrarySymlinks.MinorVersionSymlink);
            makeFile.RuleArray.Add(minorVersionRule);

            var linkerSymlinkRule = new MakeFileRule(
                moduleToBuild,
                C.PosixSharedLibrarySymlinks.LinkerSymlink,
                node.UniqueModuleName,
                dirsToCreate,
                dependentVariables,
                null,
                MakeSymlinkRecipe(commandLineBuilder, symlinkTool, moduleToBuild, workingDir, C.PosixSharedLibrarySymlinks.LinkerSymlink));
            linkerSymlinkRule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(C.PosixSharedLibrarySymlinks.LinkerSymlink);
            makeFile.RuleArray.Add(linkerSymlinkRule);

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));

            using (var makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            var exportedTargets = makeFile.ExportedTargets;
            var exportedVariables = makeFile.ExportedVariables;
            var returnData = new MakeFileData(makeFilePath, exportedTargets, exportedVariables, null);
            success = true;
            return returnData;
        }
    }
}

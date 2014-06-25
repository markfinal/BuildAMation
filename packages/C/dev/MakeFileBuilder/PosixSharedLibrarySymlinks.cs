// <copyright file="PosixSharedLibrarySymlinks.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        private static Opus.Core.StringArray
        MakeSymlinkRecipe(
            Opus.Core.StringArray commandLineBuilder,
            C.IPosixSharedLibrarySymlinksTool tool,
            C.PosixSharedLibrarySymlinks moduleToBuild,
            string workingDirectory,
            Opus.Core.LocationKey keyToSymlink)
        {
            var recipeBuilder = new System.Text.StringBuilder();
            recipeBuilder.AppendFormat("{0} ", tool.Executable((Opus.Core.BaseTarget)moduleToBuild.OwningNode.Target));
            recipeBuilder.Append(commandLineBuilder.ToString());

            var symlinkFile = moduleToBuild.Locations[keyToSymlink];
            var symlinkFileLeafname = System.IO.Path.GetFileName(symlinkFile.GetSingleRawPath());
            recipeBuilder.AppendFormat(" {0}", symlinkFileLeafname);

            var recipe = recipeBuilder.ToString();

            var recipes = new Opus.Core.StringArray();
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

            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);

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

            var commandLineBuilder = new Opus.Core.StringArray();
            if (creationOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = creationOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Opus.Core.Exception("Compiler options does not support command line translation");
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
            makeFile.RuleArray.Add(majorVersionRule);

            var minorVersionRule = new MakeFileRule(
                moduleToBuild,
                C.PosixSharedLibrarySymlinks.MinorVersionSymlink,
                node.UniqueModuleName,
                dirsToCreate,
                dependentVariables,
                null,
                MakeSymlinkRecipe(commandLineBuilder, symlinkTool, moduleToBuild, workingDir, C.PosixSharedLibrarySymlinks.MinorVersionSymlink));
            makeFile.RuleArray.Add(minorVersionRule);

            var linkerSymlinkRule = new MakeFileRule(
                moduleToBuild,
                C.PosixSharedLibrarySymlinks.LinkerSymlink,
                node.UniqueModuleName,
                dirsToCreate,
                dependentVariables,
                null,
                MakeSymlinkRecipe(commandLineBuilder, symlinkTool, moduleToBuild, workingDir, C.PosixSharedLibrarySymlinks.LinkerSymlink));
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

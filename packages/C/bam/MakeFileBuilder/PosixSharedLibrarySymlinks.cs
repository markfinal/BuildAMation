#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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

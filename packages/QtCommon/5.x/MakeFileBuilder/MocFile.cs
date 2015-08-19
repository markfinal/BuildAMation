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
namespace QtCommon
{
namespace V2
{
    public sealed class MakeFileMocGeneration :
        IMocGenerationPolicy
    {
        void
        IMocGenerationPolicy.Moc(
            MocModule sender,
            Bam.Core.V2.ExecutionContext context,
            Bam.Core.V2.Tool mocCompiler,
            Bam.Core.V2.TokenizedString generatedMocSource,
            C.V2.HeaderFile source)
        {
            var meta = new MakeFileBuilder.V2.MakeFileMeta(sender);
            var rule = meta.AddRule();
            rule.AddTarget(generatedMocSource);
            rule.AddPrerequisite(source, C.V2.HeaderFile.Key);

            var mocOutputPath = generatedMocSource.Parse();
            var mocOutputDir = System.IO.Path.GetDirectoryName(mocOutputPath);
            var command = new System.Text.StringBuilder();
            command.AppendFormat(System.String.Format("{0} -o{1} {2}", mocCompiler.Executable, mocOutputPath, source.InputPath.Parse()));
            rule.AddShellCommand(command.ToString());

            meta.CommonMetaData.Directories.AddUnique(mocOutputDir);
        }
    }
}
}
namespace MakeFileBuilder
{
    public partial class MakeFileBuilder
    {
        public object
        Build(
            QtCommon.MocFile moduleToBuild,
            out System.Boolean success)
        {
            var mocFileModule = moduleToBuild as Bam.Core.BaseModule;
            var node = mocFileModule.OwningNode;
            var target = node.Target;
            var mocFileOptions = mocFileModule.Options;
            var toolOptions = mocFileOptions as QtCommon.MocOptionCollection;

            var sourceFilePath = moduleToBuild.SourceFileLocation.GetSinglePath();
            if (!System.IO.File.Exists(sourceFilePath))
            {
                throw new Bam.Core.Exception("Moc source file '{0}' does not exist", sourceFilePath);
            }

            var inputFiles = new Bam.Core.StringArray();
            inputFiles.Add(sourceFilePath);

            // at this point, we know the node outputs need building

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);

            var commandLineBuilder = new Bam.Core.StringArray();
            if (toolOptions is CommandLineProcessor.ICommandLineSupport)
            {
                var commandLineOption = toolOptions as CommandLineProcessor.ICommandLineSupport;
                commandLineOption.ToCommandLineArguments(commandLineBuilder, target, null);
            }
            else
            {
                throw new Bam.Core.Exception("Moc options does not support command line translation");
            }

            commandLineBuilder.Add(System.String.Format("-o {0}", moduleToBuild.Locations[QtCommon.MocFile.OutputFile].GetSinglePath()));
            commandLineBuilder.Add(sourceFilePath);

            var tool = target.Toolset.Tool(typeof(QtCommon.IMocTool));
            var toolExePath = tool.Executable((Bam.Core.BaseTarget)target);

            var recipes = new Bam.Core.StringArray();
            if (toolExePath.Contains(" "))
            {
                recipes.Add("\"" + toolExePath + "\" " + commandLineBuilder.ToString(' '));
            }
            else
            {
                recipes.Add(toolExePath + " " + commandLineBuilder.ToString(' '));
            }

            var makeFilePath = MakeFileBuilder.GetMakeFilePathName(node);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(makeFilePath));
            Bam.Core.Log.DebugMessage("Makefile : '{0}'", makeFilePath);

            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var rule = new MakeFileRule(
                moduleToBuild,
                QtCommon.MocFile.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                inputFiles,
                recipes);
            rule.OutputLocationKeys = new Bam.Core.Array<Bam.Core.LocationKey>(QtCommon.MocFile.OutputFile);
            makeFile.RuleArray.Add(rule);

            using (System.IO.TextWriter makeFileWriter = new System.IO.StreamWriter(makeFilePath))
            {
                makeFile.Write(makeFileWriter);
            }

            System.Collections.Generic.Dictionary<string, Bam.Core.StringArray> environment = null;
            if (tool is Bam.Core.IToolEnvironmentVariables)
            {
                environment = (tool as Bam.Core.IToolEnvironmentVariables).Variables((Bam.Core.BaseTarget)target);
            }
            var returnData = new MakeFileData(makeFilePath, makeFile.ExportedTargets, makeFile.ExportedVariables, environment);
            success = true;
            return returnData;
        }
    }
}

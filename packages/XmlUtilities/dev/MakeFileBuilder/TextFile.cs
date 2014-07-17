// <copyright file="TextFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object
        Build(
            XmlUtilities.TextFileModule moduleToBuild,
            out bool success)
        {
            var locationMap = moduleToBuild.Locations;
            var outputDir = locationMap[XmlUtilities.TextFileModule.OutputDir];
            var outputDirPath = outputDir.GetSingleRawPath();

            if (!System.IO.Directory.Exists(outputDirPath))
            {
                System.IO.Directory.CreateDirectory(outputDirPath);
            }

            var outputFileLoc = locationMap[XmlUtilities.TextFileModule.OutputFile];
            var outputFilePath = outputFileLoc.GetSingleRawPath();

            // write a script that can be invoked by the MakeFile to generate the output file
            var shellScriptLeafName = "writeTextFile.py";
            var shellScriptLoc = Opus.Core.FileLocation.Get(outputDir, shellScriptLeafName, Opus.Core.Location.EExists.WillExist);
            var shellScriptPath = shellScriptLoc.GetSingleRawPath();
            XmlUtilities.TextToPythonScript.Write(moduleToBuild.Content, shellScriptPath, outputFilePath);

            var node = moduleToBuild.OwningNode;
            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);

            var recipe = new Opus.Core.StringArray();
            recipe.Add(System.String.Format("$(shell python {0})", shellScriptPath));

            var rule = new MakeFileRule(
                moduleToBuild,
                XmlUtilities.TextFileModule.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                null,
                recipe);
            rule.OutputLocationKeys = new Opus.Core.Array<Opus.Core.LocationKey>(XmlUtilities.TextFileModule.OutputFile);
            makeFile.RuleArray.Add(rule);

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

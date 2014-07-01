// <copyright file="XmlWriter.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace MakeFileBuilder
{
    public sealed partial class MakeFileBuilder
    {
        public object Build(XmlUtilities.XmlModule moduleToBuild, out bool success)
        {
            var locationMap = moduleToBuild.Locations;
            var outputDir = locationMap[XmlUtilities.OSXPlistModule.OutputDir];
            var outputDirPath = outputDir.GetSingleRawPath();

            // serialize the XML to memory
            var settings = new System.Xml.XmlWriterSettings();
            settings.CheckCharacters = true;
            settings.CloseOutput = true;
            settings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
            settings.Indent = true;
            settings.IndentChars = "    ";
            settings.NewLineChars = "\n";
            settings.NewLineHandling = System.Xml.NewLineHandling.None;
            settings.NewLineOnAttributes = false;
            settings.OmitXmlDeclaration = false;
            settings.Encoding = new System.Text.UTF8Encoding(false); // do not write BOM

            var xmlString = new System.Text.StringBuilder();
            using (var xmlStream = System.Xml.XmlWriter.Create(xmlString, settings))
            {
                moduleToBuild.Document.WriteTo(xmlStream);
            }

            if (!System.IO.Directory.Exists(outputDirPath))
            {
                System.IO.Directory.CreateDirectory(outputDirPath);
            }

            var plistFileLoc = locationMap[XmlUtilities.XmlModule.OutputFile];
            var plistPath = plistFileLoc.GetSingleRawPath();

            // write a script that can be invoked by the MakeFile to generate the Info.plist
            var shellScriptLoc = Opus.Core.FileLocation.Get(outputDir, "writePList.py", Opus.Core.Location.EExists.WillExist);
            var shellScriptPath = shellScriptLoc.GetSingleRawPath();
            using (var writer = new System.IO.StreamWriter(shellScriptPath))
            {
                writer.WriteLine("#!usr/bin/python");

                writer.WriteLine(System.String.Format("with open('{0}', 'wt') as script:", plistPath));
                foreach (var line in xmlString.ToString().Split('\n'))
                {
                    writer.WriteLine("\tscript.write('{0}\\n')", line);
                }
            }

            var node = moduleToBuild.OwningNode;
            var makeFile = new MakeFile(node, this.topLevelMakeFilePath);

            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);

            var recipe = new Opus.Core.StringArray();
            recipe.Add(System.String.Format("$(shell python {0})", shellScriptPath));

            var rule = new MakeFileRule(
                moduleToBuild,
                XmlUtilities.XmlModule.OutputFile,
                node.UniqueModuleName,
                dirsToCreate,
                null,
                null,
                recipe);
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

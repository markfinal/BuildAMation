// <copyright file="TextFile.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object
        Build(
            XmlUtilities.TextFileModule moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;
            var locationMap = moduleToBuild.Locations;
            var outputDir = locationMap[XmlUtilities.TextFileModule.OutputDir];
            var outputDirPath = outputDir.GetSingleRawPath();

            if (!System.IO.Directory.Exists(outputDirPath))
            {
                System.IO.Directory.CreateDirectory(outputDirPath);
            }

            var outputFileLoc = locationMap[XmlUtilities.TextFileModule.OutputFile];
            var outputFilePath = outputFileLoc.GetSingleRawPath();

            var targetNode = node.ExternalDependents[0];
            var targetData = targetNode.Data as QMakeData;

            // write a script that can be invoked by QMake to generate the text file
            var shellScriptLeafName = "writeTextFile.py";
            var shellScriptLoc = Opus.Core.FileLocation.Get(outputDir, shellScriptLeafName, Opus.Core.Location.EExists.WillExist);
            var shellScriptPath = shellScriptLoc.GetSingleRawPath();
            XmlUtilities.TextToPythonScript.Write(moduleToBuild.Content, shellScriptPath, outputFilePath);

            if (null == targetData.CustomRules)
            {
                targetData.CustomRules = new Opus.Core.StringArray();
            }
            targetData.CustomRules.Add("writeTextFileTarget.target=" + outputFilePath.Replace('\\', '/'));
            targetData.CustomRules.Add("writeTextFileTarget.depends=FORCE");
            targetData.CustomRules.Add("writeTextFileTarget.commands=python " + shellScriptPath.Replace('\\', '/'));
            targetData.CustomRules.Add("PRE_TARGETDEPS+=" + outputFilePath.Replace('\\', '/'));
            targetData.CustomRules.Add("QMAKE_EXTRA_TARGETS+=writeTextFileTarget");

            success = true;
            return null;
        }
    }
}

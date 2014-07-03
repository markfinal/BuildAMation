// <copyright file="XmlWriter.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object Build(XmlUtilities.XmlModule moduleToBuild, out bool success)
        {
            var isPlist = moduleToBuild is XmlUtilities.OSXPlistModule;

            var node = moduleToBuild.OwningNode;
            var locationMap = moduleToBuild.Locations;
            var outputDir = locationMap[XmlUtilities.OSXPlistModule.OutputDir];
            var outputDirPath = outputDir.GetSingleRawPath();

            if (!System.IO.Directory.Exists(outputDirPath))
            {
                System.IO.Directory.CreateDirectory(outputDirPath);
            }

            var outputXMLLoc = locationMap[XmlUtilities.XmlModule.OutputFile];
            var outputXMLPath = outputXMLLoc.GetSingleRawPath();

            var targetNode = node.ExternalDependents[0];
            var targetData = targetNode.Data as QMakeData;

            // write a script that can be invoked by QMake to generate the XML file
            var shellScriptLeafName = isPlist ? "writePList.py" : "writeXMLFile.py";
            var shellScriptLoc = Opus.Core.FileLocation.Get(outputDir, shellScriptLeafName, Opus.Core.Location.EExists.WillExist);
            var shellScriptPath = shellScriptLoc.GetSingleRawPath();
            XmlUtilities.XmlDocumentToPythonScript.Write(moduleToBuild.Document, shellScriptPath, outputXMLPath);

            if (null == targetData.CustomRules)
            {
                targetData.CustomRules = new Opus.Core.StringArray();
            }
            targetData.CustomRules.Add("xmlTarget.target=" + outputXMLPath);
            targetData.CustomRules.Add("xmlTarget.depends=FORCE");
            targetData.CustomRules.Add("xmlTarget.commands=python " + shellScriptPath);
            targetData.CustomRules.Add("PRE_TARGETDEPS+=" + outputXMLPath);
            targetData.CustomRules.Add("QMAKE_EXTRA_TARGETS+=xmlTarget");

            // TODO: the Plist file is not always copied - find out why
            if (isPlist)
            {
                // TODO: I'd rather this be an explicit option
                if (null == targetData.CustomRules)
                {
                    targetData.CustomRules = new Opus.Core.StringArray();
                }

                targetData.CustomRules.Add("QMAKE_INFO_PLIST=" + outputXMLPath);
            }

            success = true;
            return null;
        }
    }
}

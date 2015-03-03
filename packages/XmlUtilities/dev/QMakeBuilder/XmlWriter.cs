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
namespace QMakeBuilder
{
    public sealed partial class QMakeBuilder
    {
        public object
        Build(
            XmlUtilities.XmlModule moduleToBuild,
            out bool success)
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
            var shellScriptLoc = Bam.Core.FileLocation.Get(outputDir, shellScriptLeafName, Bam.Core.Location.EExists.WillExist);
            var shellScriptPath = shellScriptLoc.GetSingleRawPath();
            XmlUtilities.XmlDocumentToPythonScript.Write(moduleToBuild.Document, shellScriptPath, outputXMLPath);

            if (null == targetData.CustomRules)
            {
                targetData.CustomRules = new Bam.Core.StringArray();
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
                    targetData.CustomRules = new Bam.Core.StringArray();
                }

                targetData.CustomRules.Add("QMAKE_INFO_PLIST=" + outputXMLPath);
            }

            success = true;
            return null;
        }
    }
}

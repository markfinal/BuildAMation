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
#endregion
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
            var shellScriptLoc = Bam.Core.FileLocation.Get(outputDir, shellScriptLeafName, Bam.Core.Location.EExists.WillExist);
            var shellScriptPath = shellScriptLoc.GetSingleRawPath();
            XmlUtilities.TextToPythonScript.Write(moduleToBuild.Content, shellScriptPath, outputFilePath);

            if (null == targetData.CustomRules)
            {
                targetData.CustomRules = new Bam.Core.StringArray();
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

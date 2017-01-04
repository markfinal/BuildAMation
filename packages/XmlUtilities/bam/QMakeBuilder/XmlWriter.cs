#region License
// Copyright (c) 2010-2017, Mark Final
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

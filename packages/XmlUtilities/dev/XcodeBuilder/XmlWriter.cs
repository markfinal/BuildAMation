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
namespace XcodeBuilder
{
    public sealed partial class XcodeBuilder
    {
        public object
        Build(
            XmlUtilities.XmlModule moduleToBuild,
            out bool success)
        {
            // TODO: might have to implement PList specific Build functions
            var isPList = moduleToBuild is XmlUtilities.OSXPlistModule;
            var node = moduleToBuild.OwningNode;
            var targetNode = node.ExternalDependents[0];
            var project = this.Workspace.GetProject(targetNode);
            var baseTarget = (Bam.Core.BaseTarget)targetNode.Target;
            var configuration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), targetNode.ModuleName);

            var xmlFileLoc = moduleToBuild.Locations[XmlUtilities.XmlModule.OutputFile];
            var xmlFilePath = xmlFileLoc.GetSingleRawPath();

            // serialize the XML to memory
            var settings = new System.Xml.XmlWriterSettings();
            settings.CheckCharacters = true;
            settings.CloseOutput = true;
            settings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
            settings.Indent = true;
            settings.IndentChars = new string(' ', 4);
            settings.NewLineChars = "\n";
            settings.NewLineHandling = System.Xml.NewLineHandling.None;
            settings.NewLineOnAttributes = false;
            settings.OmitXmlDeclaration = false;
            settings.Encoding = new System.Text.UTF8Encoding(false); // do not write BOM

            var xmlString = new System.Text.StringBuilder();
            using (var xmlWriter = System.Xml.XmlWriter.Create(xmlString, settings))
            {
                moduleToBuild.Document.WriteTo(xmlWriter);
                xmlWriter.WriteWhitespace(settings.NewLineChars);
            }

            string shellScriptName;
            if (isPList)
            {
                shellScriptName = "Writing PList for " + targetNode.UniqueModuleName;
            }
            else
            {
                shellScriptName = "Writing XML file for " + targetNode.UniqueModuleName;
            }

            var writeXMLShellScriptBuildPhase = project.ShellScriptBuildPhases.Get(shellScriptName, node.ModuleName);
            writeXMLShellScriptBuildPhase.OutputPaths.Add(xmlFilePath);
            writeXMLShellScriptBuildPhase.ShellScriptLines.Add(System.String.Format("if [ \\\"${{CONFIGURATION}}\\\" = \\\"{0}\\\" ]; then", configuration.Name));
            foreach (var line in xmlString.ToString().Split('\n'))
            {
                var escapedLine = line.Replace("\"", "\\\"");
                writeXMLShellScriptBuildPhase.ShellScriptLines.Add(System.String.Format("echo \\\"{0}\\\" >> $SCRIPT_OUTPUT_FILE_0", escapedLine));
            }
            writeXMLShellScriptBuildPhase.ShellScriptLines.Add("fi");

            if (isPList)
            {
                // because this is performed AFTER the application, we can directly add to the build phases
                var nativeTarget = targetNode.Data as PBXNativeTarget;
                nativeTarget.BuildPhases.Insert(0, writeXMLShellScriptBuildPhase);

                // add the Info.plist into the FileReferences
                var fileRef = project.FileReferences.Get(targetNode.ModuleName, PBXFileReference.EType.PList, xmlFilePath, project.RootUri);
                nativeTarget.Group.Children.AddUnique(fileRef);

                // add to the build configuration
                var baseTarget2 = (Bam.Core.BaseTarget)moduleToBuild.OwningNode.Target;
                var buildConfiguration = project.BuildConfigurations.Get(baseTarget2.ConfigurationName('='), targetNode.ModuleName);
                buildConfiguration.Options["INFOPLIST_FILE"].AddUnique(xmlFilePath);
            }

            success = true;
            return null;
        }
    }
}

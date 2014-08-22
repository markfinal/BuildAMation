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

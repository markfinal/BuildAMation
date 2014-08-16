// <copyright file="XmlWriter.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
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

            var xmlFileLoc = moduleToBuild.Locations[XmlUtilities.XmlModule.OutputFile];
            var xmlFilePath = xmlFileLoc.GetSingleRawPath();

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
            foreach (var line in xmlString.ToString().Split('\n'))
            {
                var escapedLine = line.Replace("\"", "\\\"");
                writeXMLShellScriptBuildPhase.ShellScriptLines.Add(System.String.Format("echo \\\"{0}\\\" >> $SCRIPT_OUTPUT_FILE_0", escapedLine));
            }

            if (isPList)
            {
                // because this is performed AFTER the application, we can directly add to the build phases
                var nativeTarget = targetNode.Data as PBXNativeTarget;
                nativeTarget.BuildPhases.Insert(0, writeXMLShellScriptBuildPhase);

                // add the Info.plist into the FileReferences
                var fileRef = project.FileReferences.Get(targetNode.ModuleName, PBXFileReference.EType.PList, xmlFilePath, project.RootUri);
                nativeTarget.Group.Children.AddUnique(fileRef);

                // add to the build configuration
                var baseTarget = (Bam.Core.BaseTarget)moduleToBuild.OwningNode.Target;
                var buildConfiguration = project.BuildConfigurations.Get(baseTarget.ConfigurationName('='), targetNode.ModuleName);
                buildConfiguration.Options["INFOPLIST_FILE"].AddUnique(xmlFilePath);
            }

            success = true;
            return null;
        }
    }
}

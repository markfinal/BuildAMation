// <copyright file="XmlWriter.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object Build(XmlUtilities.XmlModule xmlModule, out bool success)
        {
            Opus.Core.DependencyNode node = xmlModule.OwningNode;

            string xmlPath = xmlModule.Options.OutputPaths[XmlUtilities.OutputFileFlags.XmlFile];

            // dependency checking
            {
                Opus.Core.StringArray outputFiles = new Opus.Core.StringArray();
                outputFiles.Add(xmlPath);
                if (!RequiresBuilding(outputFiles, new Opus.Core.StringArray()))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Opus.Core.Log.Info("Writing XML file '{0}'", xmlPath);

            // serialize the XML to disk
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.CheckCharacters = true;
            settings.CloseOutput = true;
            settings.ConformanceLevel = System.Xml.ConformanceLevel.Auto;
            settings.Indent = true;
            settings.IndentChars = "    ";
            settings.NewLineChars = "\n";
            settings.NewLineHandling = System.Xml.NewLineHandling.None;
            settings.NewLineOnAttributes = false;
            settings.OmitXmlDeclaration = false;

            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(xmlPath, settings))
            {
                xmlModule.Document.WriteTo(writer);
            }

            success = true;
            return null;
        }
    }
}

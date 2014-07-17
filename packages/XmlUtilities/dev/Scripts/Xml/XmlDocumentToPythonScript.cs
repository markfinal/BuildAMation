// <copyright file="XmlDocumentToPythonScript.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public static class XmlDocumentToPythonScript
    {
        public static void
        Write(
            System.Xml.XmlDocument document,
            string pythonScriptPath,
            string pathToGeneratedFile)
        {
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
                document.WriteTo(xmlStream);
            }

            TextToPythonScript.Write(xmlString, pythonScriptPath, pathToGeneratedFile);
        }
    }
}

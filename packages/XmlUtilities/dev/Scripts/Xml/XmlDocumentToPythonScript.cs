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
            string xmlFilePath)
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

            using (var writer = new System.IO.StreamWriter(pythonScriptPath))
            {
                writer.WriteLine("#!usr/bin/python");

                writer.WriteLine(System.String.Format("with open('{0}', 'wt') as script:", xmlFilePath));
                foreach (var line in xmlString.ToString().Split('\n'))
                {
                    writer.WriteLine("\tscript.write('{0}\\n')", line);
                }
            }
        }
    }
}

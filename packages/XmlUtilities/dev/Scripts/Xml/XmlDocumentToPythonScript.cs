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
            var xmlString = XmlDocumentToStringBuilder.Write(document);
            TextToPythonScript.Write(xmlString, pythonScriptPath, pathToGeneratedFile);
        }
    }
}

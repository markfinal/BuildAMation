// <copyright file="TextToPythonScript.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    public static class TextToPythonScript
    {
        public static void
        Write(
            System.Text.StringBuilder content,
            string pythonScriptPath,
            string pathToGeneratedFile)
        {
            using (var writer = new System.IO.StreamWriter(pythonScriptPath))
            {
                writer.WriteLine("#!usr/bin/python");

                writer.WriteLine(System.String.Format("with open(r'{0}', 'wt') as script:", pathToGeneratedFile));
                foreach (var line in content.ToString().Split('\n'))
                {
                    writer.WriteLine("\tscript.write('{0}\\n')", line);
                }
            }
        }
    }
}

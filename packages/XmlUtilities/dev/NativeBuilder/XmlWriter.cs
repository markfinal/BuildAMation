// <copyright file="XmlWriter.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace NativeBuilder
{
    public sealed partial class NativeBuilder
    {
        public object
        Build(
            XmlUtilities.XmlModule moduleToBuild,
            out bool success)
        {
            var node = moduleToBuild.OwningNode;

            var xmlLocation = moduleToBuild.Locations[XmlUtilities.XmlModule.OutputFile];
            var xmlPath = xmlLocation.GetSinglePath();
            if (null == xmlPath)
            {
                throw new Bam.Core.Exception("XML output path was not set");
            }

            // dependency checking
            {
                var outputFiles = new Bam.Core.StringArray();
                outputFiles.Add(xmlPath);
                if (!RequiresBuilding(outputFiles, new Bam.Core.StringArray()))
                {
                    Bam.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Bam.Core.Log.Info("Writing XML file '{0}'", xmlPath);

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Bam.Core.ScaffoldLocation.ETypeHint.Directory, Bam.Core.Location.EExists.WillExist);
            foreach (var dir in dirsToCreate)
            {
                var dirPath = dir.GetSinglePath();
                NativeBuilder.MakeDirectory(dirPath);
            }

            // serialize the XML to disk
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

            using (var writer = System.Xml.XmlWriter.Create(xmlPath, settings))
            {
                moduleToBuild.Document.WriteTo(writer);
            }

            success = true;
            return null;
        }
    }
}

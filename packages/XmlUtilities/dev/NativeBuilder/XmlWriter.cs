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
                throw new Opus.Core.Exception("XML output path was not set");
            }

            // dependency checking
            {
                var outputFiles = new Opus.Core.StringArray();
                outputFiles.Add(xmlPath);
                if (!RequiresBuilding(outputFiles, new Opus.Core.StringArray()))
                {
                    Opus.Core.Log.DebugMessage("'{0}' is up-to-date", node.UniqueModuleName);
                    success = true;
                    return null;
                }
            }

            Opus.Core.Log.Info("Writing XML file '{0}'", xmlPath);

            // create all directories required
            var dirsToCreate = moduleToBuild.Locations.FilterByType(Opus.Core.ScaffoldLocation.ETypeHint.Directory, Opus.Core.Location.EExists.WillExist);
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

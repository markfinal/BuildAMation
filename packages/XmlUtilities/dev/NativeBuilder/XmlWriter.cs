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
            settings.IndentChars = new string(' ', 4);
            settings.NewLineChars = "\n";
            settings.NewLineHandling = System.Xml.NewLineHandling.None;
            settings.NewLineOnAttributes = false;
            settings.OmitXmlDeclaration = false;
            settings.Encoding = new System.Text.UTF8Encoding(false); // do not write BOM

            using (var xmlWriter = System.Xml.XmlWriter.Create(xmlPath, settings))
            {
                moduleToBuild.Document.WriteTo(xmlWriter);
                xmlWriter.WriteWhitespace(settings.NewLineChars);
            }

            success = true;
            return null;
        }
    }
}

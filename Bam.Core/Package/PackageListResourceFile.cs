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
namespace Bam.Core
{
    public static class PackageListResourceFile
    {
        public static string
        WriteResourceFile()
        {
            if (0 == State.PackageInfo.Count)
            {
                throw new Exception("Package has not been specified. Run Opus from the package directory.");
            }

            var mainPackage = State.PackageInfo.MainPackage;
            var tempDirectory = System.IO.Path.GetTempPath();
            var resourceFilePathName = System.IO.Path.Combine(tempDirectory, System.String.Format("{0}.{1}", mainPackage.Name, "PackageInfoResources.resources"));

            using (var writer = new System.Resources.ResourceWriter(resourceFilePathName))
            {
                foreach (var package in State.PackageInfo)
                {
                    var id = package.Identifier;
                    string name = id.ToString("_");
                    string value = id.Root.AbsolutePath;

                    writer.AddResource(name, value);
                }
            }

            Log.DebugMessage("Written package resource file to '{0}'", resourceFilePathName);

            return resourceFilePathName;
        }

        public static string
        WriteResXFile()
        {
            if (0 == State.PackageInfo.Count)
            {
                throw new Exception("Package has not been specified. Run Opus from the package directory.");
            }

            var mainPackage = State.PackageInfo.MainPackage;

            var OpusDirectory = mainPackage.OpusDirectory;
            if (!System.IO.Directory.Exists(OpusDirectory))
            {
                System.IO.Directory.CreateDirectory(OpusDirectory);
            }

            var resourceFilePathName = System.IO.Path.Combine(OpusDirectory, "PackageInfoResources.resx");

            var resourceFile = new System.Xml.XmlDocument();
            var root = resourceFile.CreateElement("root");
            resourceFile.AppendChild(root);

            {
                var mimeType = resourceFile.CreateElement("resheader");
                mimeType.SetAttribute("name", "resmimetype");
                mimeType.InnerText = "text/microsoft-resx";
                root.AppendChild(mimeType);
            }

            {
                var version = resourceFile.CreateElement("resheader");
                version.SetAttribute("name", "version");
                version.InnerText = "2.0";
                root.AppendChild(version);
            }

            {
                var reader = resourceFile.CreateElement("resheader");
                reader.SetAttribute("name", "reader");
                // TODO: this looks like the System.Windows.Forms.dll assembly
                reader.InnerText = "System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                root.AppendChild(reader);
            }

            {
                var writer = resourceFile.CreateElement("resheader");
                writer.SetAttribute("name", "writer");
                // TODO: this looks like the System.Windows.Forms.dll assembly
                writer.InnerText = "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                root.AppendChild(writer);
            }

            foreach (var package in State.PackageInfo)
            {
                var data = resourceFile.CreateElement("data");
                data.SetAttribute("name", package.Identifier.ToString("_"));
                var value = resourceFile.CreateElement("value");
                value.InnerText = package.Identifier.Root.AbsolutePath;
                data.AppendChild(value);
                root.AppendChild(data);
            }

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = true;
            using (var xmlWriter = System.Xml.XmlWriter.Create(resourceFilePathName, xmlWriterSettings))
            {
                resourceFile.WriteTo(xmlWriter);
            }

            return resourceFilePathName;
        }
    }
}

// <copyright file="PackageListResourceFile.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public static class PackageListResourceFile
    {
        public static string WriteResourceFile()
        {
            if (0 == Core.State.PackageInfo.Count)
            {
                throw new Core.Exception("Package has not been specified. Run Opus from the package directory.", false);
            }

            Core.PackageInformation mainPackage = Core.State.PackageInfo.MainPackage;
            string tempDirectory = System.IO.Path.GetTempPath();
            string resourceFilePathName = System.IO.Path.Combine(tempDirectory, System.String.Format("{0}.{1}", mainPackage.Name, "PackageInfoResources.resources"));

            using (System.Resources.IResourceWriter writer = new System.Resources.ResourceWriter(resourceFilePathName))
            {
                foreach (Core.PackageInformation package in Core.State.PackageInfo)
                {
                    string name = System.String.Format("{0}_{1}", package.Name, package.Version);
                    string value = package.Root;

                    writer.AddResource(name, value);
                }
            }

            Log.DebugMessage("Written package resource file to '{0}'", resourceFilePathName);

            return resourceFilePathName;
        }

        public static string WriteResXFile()
        {
            if (0 == Core.State.PackageInfo.Count)
            {
                throw new Core.Exception("Package has not been specified. Run Opus from the package directory.", false);
            }

            Core.PackageInformation mainPackage = Core.State.PackageInfo.MainPackage;

            string OpusDirectory = mainPackage.OpusDirectory;
            if (!System.IO.Directory.Exists(OpusDirectory))
            {
                System.IO.Directory.CreateDirectory(OpusDirectory);
            }

            string resourceFilePathName = System.IO.Path.Combine(OpusDirectory, "PackageInfoResources.resx");

            System.Xml.XmlDocument resourceFile = new System.Xml.XmlDocument();
            System.Xml.XmlElement root = resourceFile.CreateElement("root");
            resourceFile.AppendChild(root);

            {
                System.Xml.XmlElement mimeType = resourceFile.CreateElement("resheader");
                mimeType.SetAttribute("name", "resmimetype");
                mimeType.InnerText = "text/microsoft-resx";
                root.AppendChild(mimeType);
            }

            {
                System.Xml.XmlElement version = resourceFile.CreateElement("resheader");
                version.SetAttribute("name", "version");
                version.InnerText = "2.0";
                root.AppendChild(version);
            }

            {
                System.Xml.XmlElement reader = resourceFile.CreateElement("resheader");
                reader.SetAttribute("name", "reader");
                // TODO: this looks like the System.Windows.Forms.dll assembly
                reader.InnerText = "System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                root.AppendChild(reader);
            }

            {
                System.Xml.XmlElement writer = resourceFile.CreateElement("resheader");
                writer.SetAttribute("name", "writer");
                // TODO: this looks like the System.Windows.Forms.dll assembly
                writer.InnerText = "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                root.AppendChild(writer);
            }

            foreach (Core.PackageInformation package in Core.State.PackageInfo)
            {
                System.Xml.XmlElement data = resourceFile.CreateElement("data");
                data.SetAttribute("name", System.String.Format("{0}_{1}", package.Name, package.Version));
                System.Xml.XmlElement value = resourceFile.CreateElement("value");
                value.InnerText = package.Root;
                data.AppendChild(value);
                root.AppendChild(data);
            }

            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = true;
            using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(resourceFilePathName, xmlWriterSettings))
            {
                resourceFile.WriteTo(xmlWriter);
            }

            return resourceFilePathName;
        }
    }
}

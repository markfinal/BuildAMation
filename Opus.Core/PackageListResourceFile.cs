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

            Core.PackageInformation mainPackage = Core.State.PackageInfo[0];
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

            Core.PackageInformation mainPackage = Core.State.PackageInfo[0];

            string OpusDirectory = mainPackage.OpusDirectory;
            if (!System.IO.Directory.Exists(OpusDirectory))
            {
                System.IO.Directory.CreateDirectory(OpusDirectory);
            }

            string resourceFilePathName = System.IO.Path.Combine(OpusDirectory, "PackageInfoResources.resx");

            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = true;
            using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(resourceFilePathName, xmlWriterSettings))
            {
                xmlWriter.WriteStartElement("root");
                {
                    xmlWriter.WriteStartElement("resheader");
                    {
                        xmlWriter.WriteAttributeString("name", "resmimetype");
                        xmlWriter.WriteStartElement("value");
                        {
                            xmlWriter.WriteString("text/microsoft-resx");
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteStartElement("resheader");
                    {
                        xmlWriter.WriteAttributeString("name", "version");
                        xmlWriter.WriteStartElement("value");
                        {
                            xmlWriter.WriteString("2.0");
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteStartElement("resheader");
                    {
                        xmlWriter.WriteAttributeString("name", "reader");
                        xmlWriter.WriteStartElement("value");
                        {
                            // TODO: this looks like the System.Windows.Forms.dll assembly
                            xmlWriter.WriteString("System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteStartElement("resheader");
                    {
                        xmlWriter.WriteAttributeString("name", "writer");
                        xmlWriter.WriteStartElement("value");
                        {
                            // TODO: this looks like the System.Windows.Forms.dll assembly
                            xmlWriter.WriteString("System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                    }

                    foreach (Core.PackageInformation package in Core.State.PackageInfo)
                    {
                        xmlWriter.WriteStartElement("data");
                        {
                            xmlWriter.WriteAttributeString("name", System.String.Format("{0}_{1}", package.Name, package.Version));
                            xmlWriter.WriteStartElement("value");
                            {
                                xmlWriter.WriteString(package.Root);
                                xmlWriter.WriteEndElement();
                            }
                            xmlWriter.WriteEndElement();
                        }
                    }

                    xmlWriter.WriteEndElement();
                }
            }

            return resourceFilePathName;
        }
    }
}

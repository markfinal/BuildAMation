// <copyright file="WorkspaceSettings.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XcodeBuilder package</summary>
// <author>Mark Final</author>
namespace XcodeBuilder
{
    public class WorkspaceSettings
    {
        public
        WorkspaceSettings(
            Workspace workspace)
        {
            this.Path = workspace.BundlePath;
            this.Path = System.IO.Path.Combine(this.Path, "xcuserdata");
            this.Path = System.IO.Path.Combine(this.Path, System.Environment.GetEnvironmentVariable("USER") + ".xcuserdatad");
            this.Path = System.IO.Path.Combine(this.Path, "WorkspaceSettings.xcsettings");
            this.CreatePlist();
        }

        private void
        CreateKeyValuePair(
            System.Xml.XmlElement parent,
            string key,
            string value)
        {
            var keyEl = this.Document.CreateElement("key");
            keyEl.InnerText = key;
            var valueEl = this.Document.CreateElement("string");
            valueEl.InnerText = value;
            parent.AppendChild(keyEl);
            parent.AppendChild(valueEl);
        }

        private void
        CreatePlist()
        {
            this.Document = new System.Xml.XmlDocument();

            {
                var type = this.Document.CreateDocumentType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
                this.Document.AppendChild(type);
            }
            var plistEl = this.Document.CreateElement("plist");
            {
                var versionAttr = this.Document.CreateAttribute("version");
                versionAttr.Value = "1.0";
                plistEl.Attributes.Append(versionAttr);
            }

            var dictEl = this.Document.CreateElement("dict");
            plistEl.AppendChild(dictEl);
            this.Document.AppendChild(plistEl);

            // build and intermediate file locations
            CreateKeyValuePair(dictEl, "BuildLocationStyle", "CustomLocation");
            CreateKeyValuePair(dictEl, "CustomBuildIntermediatesPath", ".");
            CreateKeyValuePair(dictEl, "CustomBuildLocationType", "RelativeToWorkspace");
            CreateKeyValuePair(dictEl, "CustomBuildProductsPath", ".");

            // derived data
            CreateKeyValuePair(dictEl, "DerivedDataCustomLocation", "XcodeDerivedData");
            CreateKeyValuePair(dictEl, "DerivedDataLocationStyle", "WorkspaceRelativePath");
        }

        private string Path
        {
            get;
            set;
        }

        private System.Xml.XmlDocument Document
        {
            get;
            set;
        }

        public void
        Serialize()
        {
            // do not write a Byte-Ordering-Mark (BOM)
            var encoding = new System.Text.UTF8Encoding(false);

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.Path));
            using (var writer = new System.IO.StreamWriter(this.Path, false, encoding))
            {
                var settings = new System.Xml.XmlWriterSettings();
                settings.OmitXmlDeclaration = false;
                settings.NewLineChars = "\n";
                settings.Indent = true;
                using (var xmlWriter = System.Xml.XmlWriter.Create(writer, settings))
                {
                    this.Document.WriteTo(xmlWriter);
                }
            }
        }
    }
}

#region License
// Copyright (c) 2010-2017, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
namespace XcodeBuilder
{
    public class WorkspaceSettings
    {
        public
        WorkspaceSettings(
            string workspaceDir)
        {
            this.Path = workspaceDir;
            this.Path = System.IO.Path.Combine(this.Path, "xcuserdata");
            this.Path = System.IO.Path.Combine(this.Path, System.Environment.GetEnvironmentVariable("USER") + ".xcuserdatad");
            this.Path = System.IO.Path.Combine(this.Path, "WorkspaceSettings.xcsettings");
            this.CreatePlist();
        }

        private void
        CreateKeyValuePair(
            System.Xml.XmlDocument doc,
            System.Xml.XmlElement parent,
            string key,
            string value)
        {
            var keyEl = doc.CreateElement("key");
            keyEl.InnerText = key;
            var valueEl = doc.CreateElement("string");
            valueEl.InnerText = value;
            parent.AppendChild(keyEl);
            parent.AppendChild(valueEl);
        }

        private void
        CreatePlist()
        {
            var doc = new System.Xml.XmlDocument();
            // don't resolve any URLs, or if there is no internet, the process will pause for some time
            doc.XmlResolver = null;

            {
                var type = doc.CreateDocumentType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
                doc.AppendChild(type);
            }
            var plistEl = doc.CreateElement("plist");
            {
                var versionAttr = doc.CreateAttribute("version");
                versionAttr.Value = "1.0";
                plistEl.Attributes.Append(versionAttr);
            }

            var dictEl = doc.CreateElement("dict");
            plistEl.AppendChild(dictEl);
            doc.AppendChild(plistEl);

#if true
            // TODO: this seems to be the only way to get the target settings working
            CreateKeyValuePair(doc, dictEl, "BuildLocationStyle", "UseTargetSettings");
#else
            // build and intermediate file locations
            CreateKeyValuePair(doc, dictEl, "BuildLocationStyle", "CustomLocation");
            CreateKeyValuePair(doc, dictEl, "CustomBuildIntermediatesPath", "XcodeIntermediates"); // where xxx.build folders are stored
            CreateKeyValuePair(doc, dictEl, "CustomBuildLocationType", "RelativeToWorkspace");
            CreateKeyValuePair(doc, dictEl, "CustomBuildProductsPath", "."); // has to be the workspace folder, in order to write files to expected locations

            // derived data
            CreateKeyValuePair(doc, dictEl, "DerivedDataCustomLocation", "XcodeDerivedData");
            CreateKeyValuePair(doc, dictEl, "DerivedDataLocationStyle", "WorkspaceRelativePath");
#endif

            this.Document = doc;
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

            Bam.Core.IOWrapper.CreateDirectory(System.IO.Path.GetDirectoryName(this.Path));
            using (var writer = new System.IO.StreamWriter(this.Path, false, encoding))
            {
                var settings = new System.Xml.XmlWriterSettings();
                settings.OmitXmlDeclaration = false;
                settings.NewLineChars = "\n";
                settings.Indent = true;
                using (var xmlWriter = System.Xml.XmlWriter.Create(writer, settings))
                {
                    this.Document.WriteTo(xmlWriter);
                    xmlWriter.WriteWhitespace(settings.NewLineChars);
                }
            }
        }
    }
}

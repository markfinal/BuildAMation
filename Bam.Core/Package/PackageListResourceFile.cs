#region License
// Copyright (c) 2010-2015, Mark Final
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
namespace Bam.Core
{
    public static class PackageListResourceFile
    {
        public static string
        WriteResourceFile()
        {
            if (0 == State.PackageInfo.Count)
            {
                throw new Exception("Package has not been specified. Run 'bam' from the package directory.");
            }

            var mainPackage = State.PackageInfo.MainPackage;
            var tempDirectory = System.IO.Path.GetTempPath();
            var resourceFilePathName = System.IO.Path.Combine(tempDirectory, System.String.Format("{0}.{1}", mainPackage.Name, "PackageInfoResources.resources"));

            using (var writer = new System.Resources.ResourceWriter(resourceFilePathName))
            {
                // TODO: are these necessary now?
#if false
                foreach (var package in State.PackageInfo)
                {
                    var id = package.Identifier;
                    string name = id.ToString("_");
                    string value = id.Root.AbsolutePath;

                    writer.AddResource(name, value);
                }
#endif
            }

            Log.DebugMessage("Written package resource file to '{0}'", resourceFilePathName);

            return resourceFilePathName;
        }

        private static void
        AddResHeader(
            System.Xml.XmlDocument document,
            string name,
            string value,
            System.Xml.XmlElement parent)
        {
            var resHeader = document.CreateElement("resheader");
            parent.AppendChild(resHeader);

            resHeader.SetAttribute("name", name);
            resHeader.InnerText = value;
        }

        private static void
        AddData(
            System.Xml.XmlDocument document,
            string name,
            string value,
            System.Xml.XmlElement parent)
        {
            var data = document.CreateElement("data");
            parent.AppendChild(data);

            data.SetAttribute("name", name);
            var valueEl = document.CreateElement("value");
            valueEl.InnerText = value;
            data.AppendChild(valueEl);
        }

        public static string
        WriteResXFile()
        {
            if (0 == State.PackageInfo.Count)
            {
                throw new Exception("Package has not been specified. Run 'bam' from the package directory.");
            }

            var mainPackage = State.PackageInfo.MainPackage;

            var projectDirectory = mainPackage.ProjectDirectory;
            if (!System.IO.Directory.Exists(projectDirectory))
            {
                System.IO.Directory.CreateDirectory(projectDirectory);
            }

            var resourceFilePathName = System.IO.Path.Combine(projectDirectory, "PackageInfoResources.resx");

            var resourceFile = new System.Xml.XmlDocument();
            var root = resourceFile.CreateElement("root");
            resourceFile.AppendChild(root);

            AddResHeader(resourceFile, "resmimetype", "text/microsoft-resx", root);
            AddResHeader(resourceFile, "version", "2.0", root);
            // TODO: this looks like the System.Windows.Forms.dll assembly
            AddResHeader(resourceFile, "reader", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", root);
            AddResHeader(resourceFile, "writer", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", root);

            AddData(resourceFile, "BamInstallDir", State.ExecutableDirectory, root);
            AddData(resourceFile, "WorkingDir", mainPackage.Identifier.Path, root);

            // TODO: are these necessary now?
#if false
            foreach (var package in State.PackageInfo)
            {
                AddData(resourceFile, package.Identifier.ToString("_"), package.Identifier.Root.AbsolutePath, root);
            }
#endif

            var xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.OmitXmlDeclaration = true;
            using (var xmlWriter = System.Xml.XmlWriter.Create(resourceFilePathName, xmlWriterSettings))
            {
                resourceFile.WriteTo(xmlWriter);
                xmlWriter.WriteWhitespace(xmlWriterSettings.NewLineChars);
            }

            return resourceFilePathName;
        }
    }
}

#region License
// Copyright (c) 2010-2019, Mark Final
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
using System.Linq;
namespace Bam.Core
{
    /// <summary>
    /// Resource file containing data allowing a debug project to run standalone.
    /// </summary>
    public static class PackageListResourceFile
    {
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

        /// <summary>
        /// Write the resource file containing the data.
        /// </summary>
        /// <returns>The res X file.</returns>
        /// <param name="projectPath">Project path.</param>
        public static string
        WriteResXFile(
            string projectPath)
        {
            if (0 == Graph.Instance.Packages.Count())
            {
                throw new Exception("Package has not been specified. Run 'bam' from the package directory.");
            }

            var masterPackage = Graph.Instance.MasterPackage;

            var projectDirectory = System.IO.Path.GetDirectoryName(projectPath);
            IOWrapper.CreateDirectoryIfNotExists(projectDirectory);

            var resourceFilePathName = System.IO.Path.Combine(projectDirectory, "PackageInfoResources.resx");

            var resourceFile = new System.Xml.XmlDocument();
            var root = resourceFile.CreateElement("root");
            resourceFile.AppendChild(root);

            AddResHeader(resourceFile, "resmimetype", "text/microsoft-resx", root);
            AddResHeader(resourceFile, "version", "2.0", root);
            // TODO: this looks like the System.Windows.Forms.dll assembly
            AddResHeader(resourceFile, "reader", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", root);
            AddResHeader(resourceFile, "writer", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", root);

            AddData(resourceFile, "BamInstallDir", Core.Graph.Instance.ProcessState.ExecutableDirectory, root);
            // TODO: could be Core.Graph.Instance.ProcessState.WorkingDirectory?
            AddData(resourceFile, "WorkingDir", masterPackage.GetPackageDirectory(), root);

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

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
namespace XmlUtilities
{
    [Bam.Core.ModuleToolAssignment(typeof(IOSXPlistWriterTool))]
    public class OSXPlistModule :
        XmlModule
    {
        public
        OSXPlistModule() : base()
        {
            {
                var decl = this.Document.CreateXmlDeclaration("1.0", "UTF-8", null);
                this.Document.AppendChild(decl);
            }
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

            this.DictElement = dictEl;
        }

        public System.Xml.XmlElement DictElement
        {
            get;
            private set;
        }
    }
}

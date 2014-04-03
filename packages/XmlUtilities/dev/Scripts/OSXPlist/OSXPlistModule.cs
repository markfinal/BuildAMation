// <copyright file="OSXPlistModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(IOSXPlistWriterTool))]
    public class OSXPlistModule : XmlModule
    {
        public OSXPlistModule()
            : base()
        {
            {
                System.Xml.XmlDeclaration decl = this.Document.CreateXmlDeclaration("1.0", "UTF-8", null);
                this.Document.AppendChild(decl);
            }
            {
                System.Xml.XmlDocumentType type = this.Document.CreateDocumentType("plist", "-//Apple Computer//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
                this.Document.AppendChild(type);
            }
            System.Xml.XmlElement plistEl = this.Document.CreateElement("plist");
            {
                System.Xml.XmlAttribute versionAttr = this.Document.CreateAttribute("version");
                versionAttr.Value = "1.0";
                plistEl.Attributes.Append(versionAttr);
            }

            System.Xml.XmlElement dictEl = this.Document.CreateElement("dict");
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

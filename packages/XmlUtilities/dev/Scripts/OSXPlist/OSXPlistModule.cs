// <copyright file="OSXPlistModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(IOSXPlistWriterTool))]
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

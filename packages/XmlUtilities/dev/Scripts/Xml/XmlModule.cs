// <copyright file="XmlModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Opus.Core.ModuleToolAssignment(typeof(IXmlWriterTool))]
    public class XmlModule : Opus.Core.BaseModule
    {
        public static readonly Opus.Core.LocationKey XmlOutputFile = new Opus.Core.LocationKey("XmlFile");

        public XmlModule()
        {
            this.Document = new System.Xml.XmlDocument();
        }

        public System.Xml.XmlDocument Document
        {
            get;
            private set;
        }
    }
}

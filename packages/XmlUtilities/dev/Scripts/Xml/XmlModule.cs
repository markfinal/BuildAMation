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
        public static readonly Opus.Core.LocationKey OutputFile = new Opus.Core.LocationKey("XmlFile", Opus.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Opus.Core.LocationKey OutputDir = new Opus.Core.LocationKey("XmlFileDir", Opus.Core.ScaffoldLocation.ETypeHint.Directory);

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

// <copyright file="XmlModule.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Bam.Core.ModuleToolAssignment(typeof(IXmlWriterTool))]
    public class XmlModule :
        Bam.Core.BaseModule
    {
        public static readonly Bam.Core.LocationKey OutputFile = new Bam.Core.LocationKey("XmlFile", Bam.Core.ScaffoldLocation.ETypeHint.File);
        public static readonly Bam.Core.LocationKey OutputDir = new Bam.Core.LocationKey("XmlFileDir", Bam.Core.ScaffoldLocation.ETypeHint.Directory);

        public
        XmlModule()
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

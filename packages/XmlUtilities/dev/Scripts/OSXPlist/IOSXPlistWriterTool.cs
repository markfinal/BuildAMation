// <copyright file="IOSXPlistWriterTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalXmlWriterOptionsDelegateAttribute),
                                   typeof(ExportXmlWriterOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider("XmlUtilities")]
    public interface IOSXPlistWriterTool :
        Bam.Core.ITool
    {}
}

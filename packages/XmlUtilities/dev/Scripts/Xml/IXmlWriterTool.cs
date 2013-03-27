// <copyright file="IXmlWriterTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalXmlWriterOptionsDelegateAttribute),
                                   typeof(ExportXmlWriterOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("XmlUtilities")]
    public interface IXmlWriterTool : Opus.Core.ITool
    {
    }
}

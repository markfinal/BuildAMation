// <copyright file="ITextFileTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalTextFileOptionsDelegateAttribute),
                                   typeof(ExportTextFileOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider("XmlUtilities")]
    public interface ITextFileTool :
        Bam.Core.ITool
    {}
}

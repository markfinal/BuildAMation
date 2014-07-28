// <copyright file="ITextFileTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>XmlUtilities package</summary>
// <author>Mark Final</author>
namespace XmlUtilities
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalTextFileOptionsDelegateAttribute),
                                   typeof(ExportTextFileOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("XmlUtilities")]
    public interface ITextFileTool :
        Opus.Core.ITool
    {}
}

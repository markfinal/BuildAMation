// <copyright file="IThirdPartyTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalThirdpartyToolOptionsDelegateAttribute),
                                   typeof(ExportThirdpartyToolOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetThirdPartyToolset")]
    public interface IThirdPartyTool :
        Bam.Core.ITool
    {}
}

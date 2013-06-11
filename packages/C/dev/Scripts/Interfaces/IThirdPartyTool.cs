// <copyright file="IThirdPartyTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalThirdpartyToolOptionsDelegateAttribute),
                                   typeof(ExportThirdpartyToolOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetThirdPartyToolset")]
    public interface IThirdPartyTool : Opus.Core.ITool
    {
    }
}

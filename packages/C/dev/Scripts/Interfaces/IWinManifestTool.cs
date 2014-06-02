// <copyright file="IWinManifestTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalWin32ManifestToolOptionsDelegateAttribute),
                                   typeof(ExportWin32ManifestToolOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetWinManifestToolToolset")]
    public interface IWinManifestTool : Opus.Core.ITool
    {
    }
}

// <copyright file="IWinManifestTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalWin32ManifestToolOptionsDelegateAttribute),
                                   typeof(ExportWin32ManifestToolOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetWinManifestToolToolset")]
    public interface IWinManifestTool :
        Bam.Core.ITool
    {}
}

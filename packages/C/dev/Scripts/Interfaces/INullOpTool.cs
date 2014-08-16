// <copyright file="INullOpTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    // Note: No Bam.Core.LocalAndExportTypes attribute required here as there is no build action
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetNullOpToolset")]
    public interface INullOpTool :
        Bam.Core.ITool
    {}
}

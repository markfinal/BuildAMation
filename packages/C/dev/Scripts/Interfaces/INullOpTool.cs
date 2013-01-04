// <copyright file="INullOpTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    // Note: No Opus.Core.LocalAndExportTypes attribute required here as there is no build action
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetNullOpToolset")]
    public interface INullOpTool : Opus.Core.ITool
    {
    }
}

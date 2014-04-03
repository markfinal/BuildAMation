// <copyright file="IMocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    static class ToolsetProvider
    {
        static string GetToolsetName(System.Type toolType)
        {
            return "Qt";
        }
    }

    [Opus.Core.LocalAndExportTypes(typeof(LocalMocOptionsDelegateAttribute),
                                   typeof(ExportMocOptionsDelegateAttribute))]
    //[Opus.Core.AssignToolsetProvider("Qt")]
    [Opus.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetToolsetName")]
    public interface IMocTool : Opus.Core.ITool
    {
    }
}
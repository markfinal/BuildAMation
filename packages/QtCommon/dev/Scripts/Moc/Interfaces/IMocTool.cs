// <copyright file="IMocTool.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>QtCommon package</summary>
// <author>Mark Final</author>
namespace QtCommon
{
    static class ToolsetProvider
    {
        static string
        GetToolsetName(
            System.Type toolType)
        {
            return "Qt";
        }
    }

    [Bam.Core.LocalAndExportTypes(typeof(LocalMocOptionsDelegateAttribute),
                                   typeof(ExportMocOptionsDelegateAttribute))]
    //[Bam.Core.AssignToolsetProvider("Qt")]
    [Bam.Core.AssignToolsetProvider(typeof(ToolsetProvider), "GetToolsetName")]
    public interface IMocTool :
        Bam.Core.ITool
    {}
}

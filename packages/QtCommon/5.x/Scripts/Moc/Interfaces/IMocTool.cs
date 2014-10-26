#region License
// <copyright>
//  Mark Final
// </copyright>
// <author>Mark Final</author>
#endregion // License
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

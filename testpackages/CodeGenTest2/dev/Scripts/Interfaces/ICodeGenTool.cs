namespace CodeGenTest2
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalCodeGenOptionsDelegateAttribute),
                                   typeof(ExportCodeGenOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("CodeGenTest2")]
    public interface ICodeGenTool : Opus.Core.ITool
    {
    }
}
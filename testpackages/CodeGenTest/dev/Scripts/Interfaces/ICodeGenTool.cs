namespace CodeGenTest
{
    [Opus.Core.LocalAndExportTypes(typeof(LocalCodeGenOptionsDelegateAttribute),
                                   typeof(ExportCodeGenOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("CodeGenTest")]
    public interface ICodeGenTool :
        Opus.Core.ITool
    {}
}

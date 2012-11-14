namespace CodeGenTest
{
    [Opus.Core.LocalAndExportTypesAttribute(typeof(LocalCodeGenOptionsDelegateAttribute),
                                            typeof(ExportCodeGenOptionsDelegateAttribute))]
    [Opus.Core.AssignToolsetProvider("CodeGenTest")]
    public interface ICodeGenTool : Opus.Core.ITool
    {
    }
}
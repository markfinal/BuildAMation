namespace CodeGenTest
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalCodeGenOptionsDelegateAttribute),
                                   typeof(ExportCodeGenOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider("CodeGenTest")]
    public interface ICodeGenTool :
        Bam.Core.ITool
    {}
}

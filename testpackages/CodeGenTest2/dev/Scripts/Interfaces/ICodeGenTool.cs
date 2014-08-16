namespace CodeGenTest2
{
    [Bam.Core.LocalAndExportTypes(typeof(LocalCodeGenOptionsDelegateAttribute),
                                   typeof(ExportCodeGenOptionsDelegateAttribute))]
    [Bam.Core.AssignToolsetProvider("CodeGenTest2")]
    public interface ICodeGenTool :
        Bam.Core.ITool
    {}
}

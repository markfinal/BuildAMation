// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=ICodeGenOptions.cs -n=CodeGenTest -c=CodeGenOptionCollection -p -d -dd=../../../../packages/CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=PrivateData
namespace CodeGenTest
{
    public partial class CodeGenOptionCollection
    {
        #region ICodeGenOptions Option properties
        string ICodeGenOptions.OutputSourceDirectory
        {
            get
            {
                return this.GetReferenceTypeOption<string>("OutputSourceDirectory");
            }
            set
            {
                this.SetReferenceTypeOption<string>("OutputSourceDirectory", value);
                this.ProcessNamedSetHandler("OutputSourceDirectorySetHandler", this["OutputSourceDirectory"]);
            }
        }
        string ICodeGenOptions.OutputName
        {
            get
            {
                return this.GetReferenceTypeOption<string>("OutputName");
            }
            set
            {
                this.SetReferenceTypeOption<string>("OutputName", value);
                this.ProcessNamedSetHandler("OutputNameSetHandler", this["OutputName"]);
            }
        }
        #endregion
    }
}

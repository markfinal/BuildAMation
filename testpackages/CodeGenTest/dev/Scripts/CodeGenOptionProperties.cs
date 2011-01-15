// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=ICodeGenOptions.cs -o=CodeGenOptionProperties.cs -n=CodeGenTest -c=CodeGenOptions 
namespace CodeGenTest
{
    public partial class CodeGenOptions
    {
        public string OutputSourceDirectory
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
        public string OutputName
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
    }
}

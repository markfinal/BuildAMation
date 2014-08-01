// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line arguments:
//     -i=ICodeGenOptions.cs
//     -n=CodeGenTest2
//     -c=CodeGenOptionCollection
//     -p
//     -d
//     -dd=../../../../packages/CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=PrivateData

namespace CodeGenTest2
{
    public partial class CodeGenOptionCollection
    {
        #region ICodeGenOptions Option properties
        string ICodeGenOptions.OutputSourceDirectory
        {
            get
            {
                return this.GetReferenceTypeOption<string>("OutputSourceDirectory", this.SuperSetOptionCollection);
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
                return this.GetReferenceTypeOption<string>("OutputName", this.SuperSetOptionCollection);
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

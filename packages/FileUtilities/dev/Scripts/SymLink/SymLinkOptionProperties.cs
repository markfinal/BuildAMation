// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=ISymLinkOptions.cs -o=SymLinkOptionProperties.cs -n=FileUtilities -c=SymLinkOptionCollection 
namespace FileUtilities
{
    public partial class SymLinkOptionCollection
    {
        public string LinkDirectory
        {
            get
            {
                return this.GetReferenceTypeOption<string>("LinkDirectory");
            }
            set
            {
                this.SetReferenceTypeOption<string>("LinkDirectory", value);
                this.ProcessNamedSetHandler("LinkDirectorySetHandler", this["LinkDirectory"]);
            }
        }
        public string LinkName
        {
            get
            {
                return this.GetReferenceTypeOption<string>("LinkName");
            }
            set
            {
                this.SetReferenceTypeOption<string>("LinkName", value);
                this.ProcessNamedSetHandler("LinkNameSetHandler", this["LinkName"]);
            }
        }
        public EType Type
        {
            get
            {
                return this.GetValueTypeOption<EType>("Type");
            }
            set
            {
                this.SetValueTypeOption<EType>("Type", value);
                this.ProcessNamedSetHandler("TypeSetHandler", this["Type"]);
            }
        }
    }
}

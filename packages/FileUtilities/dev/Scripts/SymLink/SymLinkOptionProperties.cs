// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=ISymLinkOptions.cs -o=SymLinkOptionProperties.cs -n=FileUtilities -c=SymLinkOptionCollection 
namespace FileUtilities
{
    public partial class SymLinkOptionCollection
    {
        string ISymLinkOptions.LinkDirectory
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
        string ISymLinkOptions.LinkName
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
        EType ISymLinkOptions.Type
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

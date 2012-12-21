// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=ISymLinkOptions.cs -n=FileUtilities -c=SymLinkOptionCollection -p -d -dd=../../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=SymLinkPrivateData
namespace FileUtilities
{
    public partial class SymLinkOptionCollection
    {
        #region ISymLinkOptions Option properties
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
        #endregion
    }
}

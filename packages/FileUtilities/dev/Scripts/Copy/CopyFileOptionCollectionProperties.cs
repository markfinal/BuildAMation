// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=ICopyFileOptions.cs -n=FileUtilities -c=CopyFileOptionCollection -p -d -dd=../../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=PrivateData
namespace FileUtilities
{
    public partial class CopyFileOptionCollection
    {
        #region ICopyFileOptions Option properties
        string ICopyFileOptions.DestinationDirectory
        {
            get
            {
                return this.GetReferenceTypeOption<string>("DestinationDirectory");
            }
            set
            {
                this.SetReferenceTypeOption<string>("DestinationDirectory", value);
                this.ProcessNamedSetHandler("DestinationDirectorySetHandler", this["DestinationDirectory"]);
            }
        }
        string ICopyFileOptions.CommonBaseDirectory
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CommonBaseDirectory");
            }
            set
            {
                this.SetReferenceTypeOption<string>("CommonBaseDirectory", value);
                this.ProcessNamedSetHandler("CommonBaseDirectorySetHandler", this["CommonBaseDirectory"]);
            }
        }
        #endregion
    }
}

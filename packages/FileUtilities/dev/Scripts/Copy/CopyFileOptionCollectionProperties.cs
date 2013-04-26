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
        System.Type ICopyFileOptions.DestinationModuleType
        {
            get
            {
                return this.GetReferenceTypeOption<System.Type>("DestinationModuleType");
            }
            set
            {
                this.SetReferenceTypeOption<System.Type>("DestinationModuleType", value);
                this.ProcessNamedSetHandler("DestinationModuleTypeSetHandler", this["DestinationModuleType"]);
            }
        }
        System.Enum ICopyFileOptions.DestinationModuleOutputEnum
        {
            get
            {
                return this.GetReferenceTypeOption<System.Enum>("DestinationModuleOutputEnum");
            }
            set
            {
                this.SetReferenceTypeOption<System.Enum>("DestinationModuleOutputEnum", value);
                this.ProcessNamedSetHandler("DestinationModuleOutputEnumSetHandler", this["DestinationModuleOutputEnum"]);
            }
        }
        System.Type ICopyFileOptions.SourceModuleType
        {
            get
            {
                return this.GetReferenceTypeOption<System.Type>("SourceModuleType");
            }
            set
            {
                this.SetReferenceTypeOption<System.Type>("SourceModuleType", value);
                this.ProcessNamedSetHandler("SourceModuleTypeSetHandler", this["SourceModuleType"]);
            }
        }
        System.Enum ICopyFileOptions.SourceModuleOutputEnum
        {
            get
            {
                return this.GetReferenceTypeOption<System.Enum>("SourceModuleOutputEnum");
            }
            set
            {
                this.SetReferenceTypeOption<System.Enum>("SourceModuleOutputEnum", value);
                this.ProcessNamedSetHandler("SourceModuleOutputEnumSetHandler", this["SourceModuleOutputEnum"]);
            }
        }
        #endregion
    }
}

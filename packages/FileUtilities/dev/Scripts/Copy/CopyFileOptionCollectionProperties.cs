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
                return this.GetReferenceTypeOption<string>("DestinationDirectory", this.SuperSetOptionCollection);
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
                return this.GetReferenceTypeOption<string>("CommonBaseDirectory", this.SuperSetOptionCollection);
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
                return this.GetReferenceTypeOption<System.Type>("DestinationModuleType", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<System.Type>("DestinationModuleType", value);
                this.ProcessNamedSetHandler("DestinationModuleTypeSetHandler", this["DestinationModuleType"]);
            }
        }
        Opus.Core.LocationKey ICopyFileOptions.DestinationModuleOutputLocation
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.LocationKey>("DestinationModuleOutputLocation", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.LocationKey>("DestinationModuleOutputLocation", value);
                this.ProcessNamedSetHandler("DestinationModuleOutputLocationSetHandler", this["DestinationModuleOutputLocation"]);
            }
        }
        string ICopyFileOptions.DestinationRelativePath
        {
            get
            {
                return this.GetReferenceTypeOption<string>("DestinationRelativePath", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("DestinationRelativePath", value);
                this.ProcessNamedSetHandler("DestinationRelativePathSetHandler", this["DestinationRelativePath"]);
            }
        }
        System.Type ICopyFileOptions.SourceModuleType
        {
            get
            {
                return this.GetReferenceTypeOption<System.Type>("SourceModuleType", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<System.Type>("SourceModuleType", value);
                this.ProcessNamedSetHandler("SourceModuleTypeSetHandler", this["SourceModuleType"]);
            }
        }
        Opus.Core.LocationKey ICopyFileOptions.SourceModuleOutputLocation
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.LocationKey>("SourceModuleOutputLocation", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.LocationKey>("SourceModuleOutputLocation", value);
                this.ProcessNamedSetHandler("SourceModuleOutputLocationSetHandler", this["SourceModuleOutputLocation"]);
            }
        }
        #endregion
    }
}

// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=ISymlinkOptions.cs -n=FileUtilities -c=SymlinkOptionCollection -p -d -dd=../../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=PrivateData
namespace FileUtilities
{
    public partial class SymlinkOptionCollection
    {
        #region ISymlinkOptions Option properties
        string ISymlinkOptions.TargetName
        {
            get
            {
                return this.GetReferenceTypeOption<string>("TargetName", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("TargetName", value);
                this.ProcessNamedSetHandler("TargetNameSetHandler", this["TargetName"]);
            }
        }
        System.Type ISymlinkOptions.DestinationModuleType
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
        Opus.Core.LocationKey ISymlinkOptions.DestinationModuleOutputLocation
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
        System.Type ISymlinkOptions.SourceModuleType
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
        Opus.Core.LocationKey ISymlinkOptions.SourceModuleOutputLocation
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

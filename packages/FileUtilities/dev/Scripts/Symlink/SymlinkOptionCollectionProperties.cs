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
        System.Enum ISymlinkOptions.DestinationModuleOutputEnum
        {
            get
            {
                return this.GetReferenceTypeOption<System.Enum>("DestinationModuleOutputEnum", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<System.Enum>("DestinationModuleOutputEnum", value);
                this.ProcessNamedSetHandler("DestinationModuleOutputEnumSetHandler", this["DestinationModuleOutputEnum"]);
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
        System.Enum ISymlinkOptions.SourceModuleOutputEnum
        {
            get
            {
                return this.GetReferenceTypeOption<System.Enum>("SourceModuleOutputEnum", this.SuperSetOptionCollection);
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

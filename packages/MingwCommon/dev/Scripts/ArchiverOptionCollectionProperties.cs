// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=../../../C/dev/Scripts/IArchiverOptions.cs;IArchiverOptions.cs -n=MingwCommon -c=ArchiverOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=PrivateData
namespace MingwCommon
{
    public partial class ArchiverOptionCollection
    {
        #region C.IArchiverOptions Option properties
        C.EArchiverOutput C.IArchiverOptions.OutputType
        {
            get
            {
                return this.GetValueTypeOption<C.EArchiverOutput>("OutputType", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.EArchiverOutput>("OutputType", value);
                this.ProcessNamedSetHandler("OutputTypeSetHandler", this["OutputType"]);
            }
        }
        string C.IArchiverOptions.AdditionalOptions
        {
            get
            {
                return this.GetReferenceTypeOption<string>("AdditionalOptions", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("AdditionalOptions", value);
                this.ProcessNamedSetHandler("AdditionalOptionsSetHandler", this["AdditionalOptions"]);
            }
        }
        #endregion
        #region IArchiverOptions Option properties
        MingwCommon.EArchiverCommand IArchiverOptions.Command
        {
            get
            {
                return this.GetValueTypeOption<MingwCommon.EArchiverCommand>("Command", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<MingwCommon.EArchiverCommand>("Command", value);
                this.ProcessNamedSetHandler("CommandSetHandler", this["Command"]);
            }
        }
        bool IArchiverOptions.DoNotWarnIfLibraryCreated
        {
            get
            {
                return this.GetValueTypeOption<bool>("DoNotWarnIfLibraryCreated", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DoNotWarnIfLibraryCreated", value);
                this.ProcessNamedSetHandler("DoNotWarnIfLibraryCreatedSetHandler", this["DoNotWarnIfLibraryCreated"]);
            }
        }
        #endregion
    }
}

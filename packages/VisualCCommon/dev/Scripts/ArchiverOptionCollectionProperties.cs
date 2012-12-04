// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=..\..\..\C\dev\Scripts\IArchiverOptions.cs;IArchiverOptions.cs -n=VisualCCommon -c=ArchiverOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs;..\..\..\VisualStudioProcessor\dev\Scripts\VisualStudioDelegate.cs -pv=PrivateData
namespace VisualCCommon
{
    public partial class ArchiverOptionCollection
    {
        #region C.IArchiverOptions Option properties
        C.EArchiverOutput C.IArchiverOptions.OutputType
        {
            get
            {
                return this.GetValueTypeOption<C.EArchiverOutput>("OutputType");
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
                return this.GetReferenceTypeOption<string>("AdditionalOptions");
            }
            set
            {
                this.SetReferenceTypeOption<string>("AdditionalOptions", value);
                this.ProcessNamedSetHandler("AdditionalOptionsSetHandler", this["AdditionalOptions"]);
            }
        }
        #endregion
        #region IArchiverOptions Option properties
        bool IArchiverOptions.NoLogo
        {
            get
            {
                return this.GetValueTypeOption<bool>("NoLogo");
            }
            set
            {
                this.SetValueTypeOption<bool>("NoLogo", value);
                this.ProcessNamedSetHandler("NoLogoSetHandler", this["NoLogo"]);
            }
        }
        #endregion
    }
}

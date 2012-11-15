// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Debug\..\..\packages\C\dev\Scripts\IArchiverOptions.cs;D:\dev\Opus\trunk\bin\Debug\..\..\packages\VisualCCommon\dev\Scripts\IArchiverOptions.cs -o=ArchiverOptionProperties.cs -n=VisualCCommon -c=ArchiverOptionCollection 
namespace VisualCCommon
{
    public partial class ArchiverOptionCollection
    {
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
    }
}

// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\prototypes\Opus\dev\bin\Debug\..\..\packages\C\dev\Scripts\IArchiverOptions.cs;D:\dev\prototypes\Opus\dev\bin\Debug\..\..\packages\VisualCCommon\dev\Scripts\IArchiverOptions.cs -o=ArchiverOptionProperties.cs -n=VisualCCommon -c=ArchiverOptionCollection 
namespace VisualCCommon
{
    public partial class ArchiverOptionCollection
    {
        public C.ToolchainOptionCollection ToolchainOptionCollection
        {
            get
            {
                return this.GetReferenceTypeOption<C.ToolchainOptionCollection>("ToolchainOptionCollection");
            }
            set
            {
                this.SetReferenceTypeOption<C.ToolchainOptionCollection>("ToolchainOptionCollection", value);
                this.ProcessNamedSetHandler("ToolchainOptionCollectionSetHandler", this["ToolchainOptionCollection"]);
            }
        }
        public C.EArchiverOutput OutputType
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
        public bool NoLogo
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

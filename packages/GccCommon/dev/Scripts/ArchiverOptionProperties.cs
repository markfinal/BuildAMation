// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\GoogleCode\Opus\trunk\bin\Debug\..\..\packages\C\dev\Scripts\IArchiverOptions.cs;D:\dev\GoogleCode\Opus\trunk\bin\Debug\..\..\packages\GccCommon\dev\Scripts\IArchiverOptions.cs -o=ArchiverOptionProperties.cs -n=GccCommon -c=ArchiverOptionCollection 
namespace GccCommon
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
        public string AdditionalArguments
        {
            get
            {
                return this.GetReferenceTypeOption<string>("AdditionalArguments");
            }
            set
            {
                this.SetReferenceTypeOption<string>("AdditionalArguments", value);
                this.ProcessNamedSetHandler("AdditionalArgumentsSetHandler", this["AdditionalArguments"]);
            }
        }
        public GccCommon.EArchiverCommand Command
        {
            get
            {
                return this.GetValueTypeOption<GccCommon.EArchiverCommand>("Command");
            }
            set
            {
                this.SetValueTypeOption<GccCommon.EArchiverCommand>("Command", value);
                this.ProcessNamedSetHandler("CommandSetHandler", this["Command"]);
            }
        }
        public bool DoNotWarnIfLibraryCreated
        {
            get
            {
                return this.GetValueTypeOption<bool>("DoNotWarnIfLibraryCreated");
            }
            set
            {
                this.SetValueTypeOption<bool>("DoNotWarnIfLibraryCreated", value);
                this.ProcessNamedSetHandler("DoNotWarnIfLibraryCreatedSetHandler", this["DoNotWarnIfLibraryCreated"]);
            }
        }
    }
}

// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Debug\..\..\packages\C\dev\Scripts\IArchiverOptions.cs;D:\dev\Opus\trunk\bin\Debug\..\..\packages\GccCommon\dev\Scripts\IArchiverOptions.cs -o=ArchiverOptionProperties.cs -n=GccCommon -c=ArchiverOptionCollection 
namespace GccCommon
{
    public partial class ArchiverOptionCollection
    {
        C.ToolchainOptionCollection C.IArchiverOptions.ToolchainOptionCollection
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
        GccCommon.EArchiverCommand IArchiverOptions.Command
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
        bool IArchiverOptions.DoNotWarnIfLibraryCreated
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

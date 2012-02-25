// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=E:\dev\prototypes\Opus\dev\bin32\Debug\..\..\packages\Intel\4.6\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=Intel -c=CCompilerOptionCollection 
namespace Intel
{
    public partial class CCompilerOptionCollection
    {
        public Intel.EVisibility Visibility
        {
            get
            {
                return this.GetValueTypeOption<Intel.EVisibility>("Visibility");
            }
            set
            {
                this.SetValueTypeOption<Intel.EVisibility>("Visibility", value);
                this.ProcessNamedSetHandler("VisibilitySetHandler", this["Visibility"]);
            }
        }
    }
}

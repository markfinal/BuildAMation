// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=E:\dev\prototypes\Opus\dev\bin32\Debug\..\..\packages\Gcc\4.6\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=Gcc -c=CCompilerOptionCollection 
namespace Gcc
{
    public partial class CCompilerOptionCollection
    {
        public Gcc.EVisibility Visibility
        {
            get
            {
                return this.GetValueTypeOption<Gcc.EVisibility>("Visibility");
            }
            set
            {
                this.SetValueTypeOption<Gcc.EVisibility>("Visibility", value);
                this.ProcessNamedSetHandler("VisibilitySetHandler", this["Visibility"]);
            }
        }
    }
}

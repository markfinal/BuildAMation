// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=E:\dev\prototypes\Opus\dev\bin32\Debug\..\..\packages\ComposerXE\4.6\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=ComposerXE -c=CCompilerOptionCollection 
namespace ComposerXE
{
    public partial class CCompilerOptionCollection
    {
        public ComposerXE.EVisibility Visibility
        {
            get
            {
                return this.GetValueTypeOption<ComposerXE.EVisibility>("Visibility");
            }
            set
            {
                this.SetValueTypeOption<ComposerXE.EVisibility>("Visibility", value);
                this.ProcessNamedSetHandler("VisibilitySetHandler", this["Visibility"]);
            }
        }
    }
}

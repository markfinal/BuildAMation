// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Debug\..\..\packages\ComposerXE\12\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=ComposerXE -c=CCompilerOptionCollection 
namespace ComposerXE
{
    public partial class CCompilerOptionCollection
    {
        ComposerXE.EVisibility ICCompilerOptions.Visibility
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

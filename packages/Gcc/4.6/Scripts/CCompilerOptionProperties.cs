// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Debug\..\..\packages\Gcc\4.4\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=Gcc -c=CCompilerOptionCollection 
namespace Gcc
{
    public partial class CCompilerOptionCollection
    {
        Gcc.EVisibility ICCompilerOptions.Visibility
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

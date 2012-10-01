// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Debug\..\..\packages\Mingw\4.5.0\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=Mingw -c=CCompilerOptionCollection 
namespace Mingw
{
    public partial class CCompilerOptionCollection
    {
        Mingw.EVisibility ICCompilerOptions.Visibility
        {
            get
            {
                return this.GetValueTypeOption<Mingw.EVisibility>("Visibility");
            }
            set
            {
                this.SetValueTypeOption<Mingw.EVisibility>("Visibility", value);
                this.ProcessNamedSetHandler("VisibilitySetHandler", this["Visibility"]);
            }
        }
    }
}

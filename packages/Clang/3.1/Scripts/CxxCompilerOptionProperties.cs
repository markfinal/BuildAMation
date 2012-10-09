// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\branches\050dev\bin\Debug\..\..\packages\C\dev\Scripts\ICPlusPlusCompilerOptions.cs -o=CxxCompilerOptionProperties.cs -n=Clang -c=CxxCompilerOptionCollection 
namespace Clang
{
    public partial class CxxCompilerOptionCollection
    {
        C.CPlusPlus.EExceptionHandler C.ICPlusPlusCompilerOptions.ExceptionHandler
        {
            get
            {
                return this.GetValueTypeOption<C.CPlusPlus.EExceptionHandler>("ExceptionHandler");
            }
            set
            {
                this.SetValueTypeOption<C.CPlusPlus.EExceptionHandler>("ExceptionHandler", value);
                this.ProcessNamedSetHandler("ExceptionHandlerSetHandler", this["ExceptionHandler"]);
            }
        }
    }
}

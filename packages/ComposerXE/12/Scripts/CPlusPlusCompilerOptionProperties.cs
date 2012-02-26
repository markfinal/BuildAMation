// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Debug\..\..\packages\C\dev\Scripts\ICPlusPlusCompilerOptions.cs -o=CPlusPlusCompilerOptionProperties.cs -n=Intel -c=CPlusPlusCompilerOptionCollection 
namespace Intel
{
    public partial class CPlusPlusCompilerOptionCollection
    {
        public C.CPlusPlus.EExceptionHandler ExceptionHandler
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

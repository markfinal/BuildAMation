// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=..\..\..\C\dev\Scripts\ICPlusPlusCompilerOptions.cs -n=Clang -c=CxxCompilerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs -pv=PrivateData 
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

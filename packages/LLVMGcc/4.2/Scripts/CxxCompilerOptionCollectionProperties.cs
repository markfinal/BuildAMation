// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=../../../C/dev/Scripts/ICxxCompilerOptions.cs -n=LLVMGcc -c=CxxCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=GccCommon.PrivateData -e
namespace LLVMGcc
{
    public partial class CxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option properties
        C.Cxx.EExceptionHandler C.ICxxCompilerOptions.ExceptionHandler
        {
            get
            {
                return this.GetValueTypeOption<C.Cxx.EExceptionHandler>("ExceptionHandler");
            }
            set
            {
                this.SetValueTypeOption<C.Cxx.EExceptionHandler>("ExceptionHandler", value);
                this.ProcessNamedSetHandler("ExceptionHandlerSetHandler", this["ExceptionHandler"]);
            }
        }
        #endregion
    }
}

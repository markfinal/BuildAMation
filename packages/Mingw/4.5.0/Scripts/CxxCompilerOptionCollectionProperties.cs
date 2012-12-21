// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=..\..\..\C\dev\Scripts\ICxxCompilerOptions.cs -n=Mingw -c=CxxCompilerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs -pv=MingwCommon.PrivateData -e
namespace Mingw
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

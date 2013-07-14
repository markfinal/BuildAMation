// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=ICCompilerOptions.cs -n=Gcc -c=ObjCCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=GccCommon.PrivateData -e
namespace Gcc
{
    public partial class ObjCCompilerOptionCollection
    {
        #region ICCompilerOptions Option properties
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
        #endregion
    }
}

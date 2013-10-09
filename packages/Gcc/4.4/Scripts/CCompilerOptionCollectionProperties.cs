// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=ICCompilerOptions.cs -n=Gcc -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=GccCommon.PrivateData -e
namespace Gcc
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option properties
        Gcc.EVisibility ICCompilerOptions.Visibility
        {
            get
            {
                return this.GetValueTypeOption<Gcc.EVisibility>("Visibility", this.SuperSetOptionCollection);
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

// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=ICCompilerOptions.cs -n=Mingw -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=MingwCommon.PrivateData -e
namespace Mingw
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option properties
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
        #endregion
    }
}

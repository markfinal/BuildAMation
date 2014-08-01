// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line arguments:
//     -i=ICCompilerOptions.cs
//     -n=Mingw
//     -c=CCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=MingwCommon.PrivateData
//     -e

namespace Mingw
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option properties
        Mingw.EVisibility ICCompilerOptions.Visibility
        {
            get
            {
                return this.GetValueTypeOption<Mingw.EVisibility>("Visibility", this.SuperSetOptionCollection);
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

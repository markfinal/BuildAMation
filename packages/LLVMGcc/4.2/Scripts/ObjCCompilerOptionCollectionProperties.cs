// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line arguments:
//     -i=ICCompilerOptions.cs
//     -n=LLVMGcc
//     -c=ObjCCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs&../../../XcodeProjectProcessor/dev/Scripts/Delegate.cs
//     -pv=GccCommon.PrivateData
//     -e

namespace LLVMGcc
{
    public partial class ObjCCompilerOptionCollection
    {
        #region ICCompilerOptions Option properties
        LLVMGcc.EVisibility ICCompilerOptions.Visibility
        {
            get
            {
                return this.GetValueTypeOption<LLVMGcc.EVisibility>("Visibility", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<LLVMGcc.EVisibility>("Visibility", value);
                this.ProcessNamedSetHandler("VisibilitySetHandler", this["Visibility"]);
            }
        }
        #endregion
    }
}

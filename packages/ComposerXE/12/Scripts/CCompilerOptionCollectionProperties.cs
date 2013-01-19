// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=ICCompilerOptions.cs -n=ComposerXE -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=ComposerXECommon.PrivateData -e
namespace ComposerXE
{
    public partial class CCompilerOptionCollection
    {
        #region ICCompilerOptions Option properties
        ComposerXE.EVisibility ICCompilerOptions.Visibility
        {
            get
            {
                return this.GetValueTypeOption<ComposerXE.EVisibility>("Visibility");
            }
            set
            {
                this.SetValueTypeOption<ComposerXE.EVisibility>("Visibility", value);
                this.ProcessNamedSetHandler("VisibilitySetHandler", this["Visibility"]);
            }
        }
        #endregion
    }
}

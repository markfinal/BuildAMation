// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Debug\..\..\packages\C\dev\Scripts\IToolchainOptions.cs -o=ToolchainOptionProperties.cs -n=ComposerXECommon -c=ToolchainOptionCollection 
namespace ComposerXECommon
{
    public partial class ToolchainOptionCollection
    {
#if false
        bool C.IToolchainOptions.IsCPlusPlus
        {
            get
            {
                return this.GetValueTypeOption<bool>("IsCPlusPlus");
            }
            set
            {
                this.SetValueTypeOption<bool>("IsCPlusPlus", value);
                this.ProcessNamedSetHandler("IsCPlusPlusSetHandler", this["IsCPlusPlus"]);
            }
        }
        C.ECharacterSet C.IToolchainOptions.CharacterSet
        {
            get
            {
                return this.GetValueTypeOption<C.ECharacterSet>("CharacterSet");
            }
            set
            {
                this.SetValueTypeOption<C.ECharacterSet>("CharacterSet", value);
                this.ProcessNamedSetHandler("CharacterSetSetHandler", this["CharacterSet"]);
            }
        }
#endif
    }
}

// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\prototypes\Opus\dev\bin32\Debug\..\..\packages\CSharp\dev\Scripts\IOptions.cs -o=OptionProperties.cs -n=CSharp -c=OptionCollection 
namespace CSharp
{
    public partial class OptionCollection
    {
        public CSharp.ETarget Target
        {
            get
            {
                return this.GetValueTypeOption<CSharp.ETarget>("Target");
            }
            set
            {
                this.SetValueTypeOption<CSharp.ETarget>("Target", value);
                this.ProcessNamedSetHandler("TargetSetHandler", this["Target"]);
            }
        }
        public bool NoLogo
        {
            get
            {
                return this.GetValueTypeOption<bool>("NoLogo");
            }
            set
            {
                this.SetValueTypeOption<bool>("NoLogo", value);
                this.ProcessNamedSetHandler("NoLogoSetHandler", this["NoLogo"]);
            }
        }
        public CSharp.EPlatform Platform
        {
            get
            {
                return this.GetValueTypeOption<CSharp.EPlatform>("Platform");
            }
            set
            {
                this.SetValueTypeOption<CSharp.EPlatform>("Platform", value);
                this.ProcessNamedSetHandler("PlatformSetHandler", this["Platform"]);
            }
        }
        public CSharp.EDebugInformation DebugInformation
        {
            get
            {
                return this.GetValueTypeOption<CSharp.EDebugInformation>("DebugInformation");
            }
            set
            {
                this.SetValueTypeOption<CSharp.EDebugInformation>("DebugInformation", value);
                this.ProcessNamedSetHandler("DebugInformationSetHandler", this["DebugInformation"]);
            }
        }
        public bool Checked
        {
            get
            {
                return this.GetValueTypeOption<bool>("Checked");
            }
            set
            {
                this.SetValueTypeOption<bool>("Checked", value);
                this.ProcessNamedSetHandler("CheckedSetHandler", this["Checked"]);
            }
        }
        public bool Unsafe
        {
            get
            {
                return this.GetValueTypeOption<bool>("Unsafe");
            }
            set
            {
                this.SetValueTypeOption<bool>("Unsafe", value);
                this.ProcessNamedSetHandler("UnsafeSetHandler", this["Unsafe"]);
            }
        }
        public CSharp.EWarningLevel WarningLevel
        {
            get
            {
                return this.GetValueTypeOption<CSharp.EWarningLevel>("WarningLevel");
            }
            set
            {
                this.SetValueTypeOption<CSharp.EWarningLevel>("WarningLevel", value);
                this.ProcessNamedSetHandler("WarningLevelSetHandler", this["WarningLevel"]);
            }
        }
        public bool WarningsAsErrors
        {
            get
            {
                return this.GetValueTypeOption<bool>("WarningsAsErrors");
            }
            set
            {
                this.SetValueTypeOption<bool>("WarningsAsErrors", value);
                this.ProcessNamedSetHandler("WarningsAsErrorsSetHandler", this["WarningsAsErrors"]);
            }
        }
        public bool Optimize
        {
            get
            {
                return this.GetValueTypeOption<bool>("Optimize");
            }
            set
            {
                this.SetValueTypeOption<bool>("Optimize", value);
                this.ProcessNamedSetHandler("OptimizeSetHandler", this["Optimize"]);
            }
        }
        public Opus.Core.FileCollection References
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.FileCollection>("References");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.FileCollection>("References", value);
                this.ProcessNamedSetHandler("ReferencesSetHandler", this["References"]);
            }
        }
        public Opus.Core.FileCollection Modules
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.FileCollection>("Modules");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.FileCollection>("Modules", value);
                this.ProcessNamedSetHandler("ModulesSetHandler", this["Modules"]);
            }
        }
    }
}

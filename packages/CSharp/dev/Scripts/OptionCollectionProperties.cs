// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=IOptions.cs -n=CSharp -c=OptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs;..\..\..\VisualStudioProcessor\dev\Scripts\VisualStudioDelegate.cs -pv=PrivateData
namespace CSharp
{
    public partial class OptionCollection
    {
        #region IOptions Option properties
        CSharp.ETarget IOptions.Target
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
        bool IOptions.NoLogo
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
        CSharp.EPlatform IOptions.Platform
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
        CSharp.EDebugInformation IOptions.DebugInformation
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
        bool IOptions.Checked
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
        bool IOptions.Unsafe
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
        CSharp.EWarningLevel IOptions.WarningLevel
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
        bool IOptions.WarningsAsErrors
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
        bool IOptions.Optimize
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
        Opus.Core.FileCollection IOptions.References
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
        Opus.Core.FileCollection IOptions.Modules
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
        Opus.Core.StringArray IOptions.Defines
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.StringArray>("Defines");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.StringArray>("Defines", value);
                this.ProcessNamedSetHandler("DefinesSetHandler", this["Defines"]);
            }
        }
        #endregion
    }
}

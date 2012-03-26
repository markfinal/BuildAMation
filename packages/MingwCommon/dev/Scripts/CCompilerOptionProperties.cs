// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\Opus\trunk\bin\Release\..\..\packages\C\dev\Scripts\ICCompilerOptions.cs;D:\dev\Opus\trunk\bin\Release\..\..\packages\MingwCommon\dev\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=MingwCommon -c=CCompilerOptionCollection 
namespace MingwCommon
{
    public partial class CCompilerOptionCollection
    {
        public C.ToolchainOptionCollection ToolchainOptionCollection
        {
            get
            {
                return this.GetReferenceTypeOption<C.ToolchainOptionCollection>("ToolchainOptionCollection");
            }
            set
            {
                this.SetReferenceTypeOption<C.ToolchainOptionCollection>("ToolchainOptionCollection", value);
                this.ProcessNamedSetHandler("ToolchainOptionCollectionSetHandler", this["ToolchainOptionCollection"]);
            }
        }
        public C.DefineCollection Defines
        {
            get
            {
                return this.GetReferenceTypeOption<C.DefineCollection>("Defines");
            }
            set
            {
                this.SetReferenceTypeOption<C.DefineCollection>("Defines", value);
                this.ProcessNamedSetHandler("DefinesSetHandler", this["Defines"]);
            }
        }
        public Opus.Core.DirectoryCollection IncludePaths
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("IncludePaths");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.DirectoryCollection>("IncludePaths", value);
                this.ProcessNamedSetHandler("IncludePathsSetHandler", this["IncludePaths"]);
            }
        }
        public Opus.Core.DirectoryCollection SystemIncludePaths
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("SystemIncludePaths");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.DirectoryCollection>("SystemIncludePaths", value);
                this.ProcessNamedSetHandler("SystemIncludePathsSetHandler", this["SystemIncludePaths"]);
            }
        }
        public C.ECompilerOutput OutputType
        {
            get
            {
                return this.GetValueTypeOption<C.ECompilerOutput>("OutputType");
            }
            set
            {
                this.SetValueTypeOption<C.ECompilerOutput>("OutputType", value);
                this.ProcessNamedSetHandler("OutputTypeSetHandler", this["OutputType"]);
            }
        }
        public bool DebugSymbols
        {
            get
            {
                return this.GetValueTypeOption<bool>("DebugSymbols");
            }
            set
            {
                this.SetValueTypeOption<bool>("DebugSymbols", value);
                this.ProcessNamedSetHandler("DebugSymbolsSetHandler", this["DebugSymbols"]);
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
        public bool IgnoreStandardIncludePaths
        {
            get
            {
                return this.GetValueTypeOption<bool>("IgnoreStandardIncludePaths");
            }
            set
            {
                this.SetValueTypeOption<bool>("IgnoreStandardIncludePaths", value);
                this.ProcessNamedSetHandler("IgnoreStandardIncludePathsSetHandler", this["IgnoreStandardIncludePaths"]);
            }
        }
        public C.EOptimization Optimization
        {
            get
            {
                return this.GetValueTypeOption<C.EOptimization>("Optimization");
            }
            set
            {
                this.SetValueTypeOption<C.EOptimization>("Optimization", value);
                this.ProcessNamedSetHandler("OptimizationSetHandler", this["Optimization"]);
            }
        }
        public string CustomOptimization
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CustomOptimization");
            }
            set
            {
                this.SetReferenceTypeOption<string>("CustomOptimization", value);
                this.ProcessNamedSetHandler("CustomOptimizationSetHandler", this["CustomOptimization"]);
            }
        }
        public C.ETargetLanguage TargetLanguage
        {
            get
            {
                return this.GetValueTypeOption<C.ETargetLanguage>("TargetLanguage");
            }
            set
            {
                this.SetValueTypeOption<C.ETargetLanguage>("TargetLanguage", value);
                this.ProcessNamedSetHandler("TargetLanguageSetHandler", this["TargetLanguage"]);
            }
        }
        public bool ShowIncludes
        {
            get
            {
                return this.GetValueTypeOption<bool>("ShowIncludes");
            }
            set
            {
                this.SetValueTypeOption<bool>("ShowIncludes", value);
                this.ProcessNamedSetHandler("ShowIncludesSetHandler", this["ShowIncludes"]);
            }
        }
        public string AdditionalOptions
        {
            get
            {
                return this.GetReferenceTypeOption<string>("AdditionalOptions");
            }
            set
            {
                this.SetReferenceTypeOption<string>("AdditionalOptions", value);
                this.ProcessNamedSetHandler("AdditionalOptionsSetHandler", this["AdditionalOptions"]);
            }
        }
        public bool OmitFramePointer
        {
            get
            {
                return this.GetValueTypeOption<bool>("OmitFramePointer");
            }
            set
            {
                this.SetValueTypeOption<bool>("OmitFramePointer", value);
                this.ProcessNamedSetHandler("OmitFramePointerSetHandler", this["OmitFramePointer"]);
            }
        }
        public Opus.Core.StringArray DisableWarnings
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.StringArray>("DisableWarnings");
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.StringArray>("DisableWarnings", value);
                this.ProcessNamedSetHandler("DisableWarningsSetHandler", this["DisableWarnings"]);
            }
        }
        public bool AllWarnings
        {
            get
            {
                return this.GetValueTypeOption<bool>("AllWarnings");
            }
            set
            {
                this.SetValueTypeOption<bool>("AllWarnings", value);
                this.ProcessNamedSetHandler("AllWarningsSetHandler", this["AllWarnings"]);
            }
        }
        public bool ExtraWarnings
        {
            get
            {
                return this.GetValueTypeOption<bool>("ExtraWarnings");
            }
            set
            {
                this.SetValueTypeOption<bool>("ExtraWarnings", value);
                this.ProcessNamedSetHandler("ExtraWarningsSetHandler", this["ExtraWarnings"]);
            }
        }
        public bool StrictAliasing
        {
            get
            {
                return this.GetValueTypeOption<bool>("StrictAliasing");
            }
            set
            {
                this.SetValueTypeOption<bool>("StrictAliasing", value);
                this.ProcessNamedSetHandler("StrictAliasingSetHandler", this["StrictAliasing"]);
            }
        }
        public bool InlineFunctions
        {
            get
            {
                return this.GetValueTypeOption<bool>("InlineFunctions");
            }
            set
            {
                this.SetValueTypeOption<bool>("InlineFunctions", value);
                this.ProcessNamedSetHandler("InlineFunctionsSetHandler", this["InlineFunctions"]);
            }
        }
        public bool Pedantic
        {
            get
            {
                return this.GetValueTypeOption<bool>("Pedantic");
            }
            set
            {
                this.SetValueTypeOption<bool>("Pedantic", value);
                this.ProcessNamedSetHandler("PedanticSetHandler", this["Pedantic"]);
            }
        }
    }
}

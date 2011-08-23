// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=D:\dev\GoogleCode\Opus\trunk\bin\Debug\..\..\packages\C\dev\Scripts\ICCompilerOptions.cs;D:\dev\GoogleCode\Opus\trunk\bin\Debug\..\..\packages\VisualCCommon\dev\Scripts\ICCompilerOptions.cs -o=CCompilerOptionProperties.cs -n=VisualCCommon -c=CCompilerOptionCollection 
namespace VisualCCommon
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
                this.ProcessNamedSetHandler("AdditionalArgumentsSetHandler", this["AdditionalOptions"]);
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
        public bool MinimalRebuild
        {
            get
            {
                return this.GetValueTypeOption<bool>("MinimalRebuild");
            }
            set
            {
                this.SetValueTypeOption<bool>("MinimalRebuild", value);
                this.ProcessNamedSetHandler("MinimalRebuildSetHandler", this["MinimalRebuild"]);
            }
        }
        public VisualCCommon.EWarningLevel WarningLevel
        {
            get
            {
                return this.GetValueTypeOption<VisualCCommon.EWarningLevel>("WarningLevel");
            }
            set
            {
                this.SetValueTypeOption<VisualCCommon.EWarningLevel>("WarningLevel", value);
                this.ProcessNamedSetHandler("WarningLevelSetHandler", this["WarningLevel"]);
            }
        }
        public VisualCCommon.EDebugType DebugType
        {
            get
            {
                return this.GetValueTypeOption<VisualCCommon.EDebugType>("DebugType");
            }
            set
            {
                this.SetValueTypeOption<VisualCCommon.EDebugType>("DebugType", value);
                this.ProcessNamedSetHandler("DebugTypeSetHandler", this["DebugType"]);
            }
        }
        public VisualCCommon.EBrowseInformation BrowseInformation
        {
            get
            {
                return this.GetValueTypeOption<VisualCCommon.EBrowseInformation>("BrowseInformation");
            }
            set
            {
                this.SetValueTypeOption<VisualCCommon.EBrowseInformation>("BrowseInformation", value);
                this.ProcessNamedSetHandler("BrowseInformationSetHandler", this["BrowseInformation"]);
            }
        }
        public bool StringPooling
        {
            get
            {
                return this.GetValueTypeOption<bool>("StringPooling");
            }
            set
            {
                this.SetValueTypeOption<bool>("StringPooling", value);
                this.ProcessNamedSetHandler("StringPoolingSetHandler", this["StringPooling"]);
            }
        }
        public bool DisableLanguageExtensions
        {
            get
            {
                return this.GetValueTypeOption<bool>("DisableLanguageExtensions");
            }
            set
            {
                this.SetValueTypeOption<bool>("DisableLanguageExtensions", value);
                this.ProcessNamedSetHandler("DisableLanguageExtensionsSetHandler", this["DisableLanguageExtensions"]);
            }
        }
        public bool ForceConformanceInForLoopScope
        {
            get
            {
                return this.GetValueTypeOption<bool>("ForceConformanceInForLoopScope");
            }
            set
            {
                this.SetValueTypeOption<bool>("ForceConformanceInForLoopScope", value);
                this.ProcessNamedSetHandler("ForceConformanceInForLoopScopeSetHandler", this["ForceConformanceInForLoopScope"]);
            }
        }
        public bool UseFullPaths
        {
            get
            {
                return this.GetValueTypeOption<bool>("UseFullPaths");
            }
            set
            {
                this.SetValueTypeOption<bool>("UseFullPaths", value);
                this.ProcessNamedSetHandler("UseFullPathsSetHandler", this["UseFullPaths"]);
            }
        }
        public EManagedCompilation CompileAsManaged
        {
            get
            {
                return this.GetValueTypeOption<EManagedCompilation>("CompileAsManaged");
            }
            set
            {
                this.SetValueTypeOption<EManagedCompilation>("CompileAsManaged", value);
                this.ProcessNamedSetHandler("CompileAsManagedSetHandler", this["CompileAsManaged"]);
            }
        }
        public EBasicRuntimeChecks BasicRuntimeChecks
        {
            get
            {
                return this.GetValueTypeOption<EBasicRuntimeChecks>("BasicRuntimeChecks");
            }
            set
            {
                this.SetValueTypeOption<EBasicRuntimeChecks>("BasicRuntimeChecks", value);
                this.ProcessNamedSetHandler("BasicRuntimeChecksSetHandler", this["BasicRuntimeChecks"]);
            }
        }
        public bool SmallerTypeConversionRuntimeCheck
        {
            get
            {
                return this.GetValueTypeOption<bool>("SmallerTypeConversionRuntimeCheck");
            }
            set
            {
                this.SetValueTypeOption<bool>("SmallerTypeConversionRuntimeCheck", value);
                this.ProcessNamedSetHandler("SmallerTypeConversionRuntimeCheckSetHandler", this["SmallerTypeConversionRuntimeCheck"]);
            }
        }
    }
}

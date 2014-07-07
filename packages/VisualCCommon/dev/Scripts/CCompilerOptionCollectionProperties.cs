// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=../../../C/dev/Scripts/ICCompilerOptions.cs;ICCompilerOptions.cs -n=VisualCCommon -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs;../../../VisualStudioProcessor/dev/Scripts/VisualStudioDelegate.cs -pv=PrivateData
namespace VisualCCommon
{
    public partial class CCompilerOptionCollection
    {
        #region C.ICCompilerOptions Option properties
        C.DefineCollection C.ICCompilerOptions.Defines
        {
            get
            {
                return this.GetReferenceTypeOption<C.DefineCollection>("Defines", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<C.DefineCollection>("Defines", value);
                this.ProcessNamedSetHandler("DefinesSetHandler", this["Defines"]);
            }
        }
        Opus.Core.DirectoryCollection C.ICCompilerOptions.IncludePaths
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("IncludePaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.DirectoryCollection>("IncludePaths", value);
                this.ProcessNamedSetHandler("IncludePathsSetHandler", this["IncludePaths"]);
            }
        }
        Opus.Core.DirectoryCollection C.ICCompilerOptions.SystemIncludePaths
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.DirectoryCollection>("SystemIncludePaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.DirectoryCollection>("SystemIncludePaths", value);
                this.ProcessNamedSetHandler("SystemIncludePathsSetHandler", this["SystemIncludePaths"]);
            }
        }
        C.ECompilerOutput C.ICCompilerOptions.OutputType
        {
            get
            {
                return this.GetValueTypeOption<C.ECompilerOutput>("OutputType", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.ECompilerOutput>("OutputType", value);
                this.ProcessNamedSetHandler("OutputTypeSetHandler", this["OutputType"]);
            }
        }
        bool C.ICCompilerOptions.DebugSymbols
        {
            get
            {
                return this.GetValueTypeOption<bool>("DebugSymbols", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DebugSymbols", value);
                this.ProcessNamedSetHandler("DebugSymbolsSetHandler", this["DebugSymbols"]);
            }
        }
        bool C.ICCompilerOptions.WarningsAsErrors
        {
            get
            {
                return this.GetValueTypeOption<bool>("WarningsAsErrors", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("WarningsAsErrors", value);
                this.ProcessNamedSetHandler("WarningsAsErrorsSetHandler", this["WarningsAsErrors"]);
            }
        }
        bool C.ICCompilerOptions.IgnoreStandardIncludePaths
        {
            get
            {
                return this.GetValueTypeOption<bool>("IgnoreStandardIncludePaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("IgnoreStandardIncludePaths", value);
                this.ProcessNamedSetHandler("IgnoreStandardIncludePathsSetHandler", this["IgnoreStandardIncludePaths"]);
            }
        }
        C.EOptimization C.ICCompilerOptions.Optimization
        {
            get
            {
                return this.GetValueTypeOption<C.EOptimization>("Optimization", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.EOptimization>("Optimization", value);
                this.ProcessNamedSetHandler("OptimizationSetHandler", this["Optimization"]);
            }
        }
        string C.ICCompilerOptions.CustomOptimization
        {
            get
            {
                return this.GetReferenceTypeOption<string>("CustomOptimization", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("CustomOptimization", value);
                this.ProcessNamedSetHandler("CustomOptimizationSetHandler", this["CustomOptimization"]);
            }
        }
        C.ETargetLanguage C.ICCompilerOptions.TargetLanguage
        {
            get
            {
                return this.GetValueTypeOption<C.ETargetLanguage>("TargetLanguage", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.ETargetLanguage>("TargetLanguage", value);
                this.ProcessNamedSetHandler("TargetLanguageSetHandler", this["TargetLanguage"]);
            }
        }
        bool C.ICCompilerOptions.ShowIncludes
        {
            get
            {
                return this.GetValueTypeOption<bool>("ShowIncludes", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("ShowIncludes", value);
                this.ProcessNamedSetHandler("ShowIncludesSetHandler", this["ShowIncludes"]);
            }
        }
        string C.ICCompilerOptions.AdditionalOptions
        {
            get
            {
                return this.GetReferenceTypeOption<string>("AdditionalOptions", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<string>("AdditionalOptions", value);
                this.ProcessNamedSetHandler("AdditionalOptionsSetHandler", this["AdditionalOptions"]);
            }
        }
        bool C.ICCompilerOptions.OmitFramePointer
        {
            get
            {
                return this.GetValueTypeOption<bool>("OmitFramePointer", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("OmitFramePointer", value);
                this.ProcessNamedSetHandler("OmitFramePointerSetHandler", this["OmitFramePointer"]);
            }
        }
        Opus.Core.StringArray C.ICCompilerOptions.DisableWarnings
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.StringArray>("DisableWarnings", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.StringArray>("DisableWarnings", value);
                this.ProcessNamedSetHandler("DisableWarningsSetHandler", this["DisableWarnings"]);
            }
        }
        C.ECharacterSet C.ICCompilerOptions.CharacterSet
        {
            get
            {
                return this.GetValueTypeOption<C.ECharacterSet>("CharacterSet", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.ECharacterSet>("CharacterSet", value);
                this.ProcessNamedSetHandler("CharacterSetSetHandler", this["CharacterSet"]);
            }
        }
        C.ELanguageStandard C.ICCompilerOptions.LanguageStandard
        {
            get
            {
                return this.GetValueTypeOption<C.ELanguageStandard>("LanguageStandard", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<C.ELanguageStandard>("LanguageStandard", value);
                this.ProcessNamedSetHandler("LanguageStandardSetHandler", this["LanguageStandard"]);
            }
        }
        C.DefineCollection C.ICCompilerOptions.Undefines
        {
            get
            {
                return this.GetReferenceTypeOption<C.DefineCollection>("Undefines", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<C.DefineCollection>("Undefines", value);
                this.ProcessNamedSetHandler("UndefinesSetHandler", this["Undefines"]);
            }
        }
        #endregion
        #region ICCompilerOptions Option properties
        bool ICCompilerOptions.NoLogo
        {
            get
            {
                return this.GetValueTypeOption<bool>("NoLogo", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("NoLogo", value);
                this.ProcessNamedSetHandler("NoLogoSetHandler", this["NoLogo"]);
            }
        }
        bool ICCompilerOptions.MinimalRebuild
        {
            get
            {
                return this.GetValueTypeOption<bool>("MinimalRebuild", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("MinimalRebuild", value);
                this.ProcessNamedSetHandler("MinimalRebuildSetHandler", this["MinimalRebuild"]);
            }
        }
        VisualCCommon.EWarningLevel ICCompilerOptions.WarningLevel
        {
            get
            {
                return this.GetValueTypeOption<VisualCCommon.EWarningLevel>("WarningLevel", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<VisualCCommon.EWarningLevel>("WarningLevel", value);
                this.ProcessNamedSetHandler("WarningLevelSetHandler", this["WarningLevel"]);
            }
        }
        VisualCCommon.EDebugType ICCompilerOptions.DebugType
        {
            get
            {
                return this.GetValueTypeOption<VisualCCommon.EDebugType>("DebugType", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<VisualCCommon.EDebugType>("DebugType", value);
                this.ProcessNamedSetHandler("DebugTypeSetHandler", this["DebugType"]);
            }
        }
        VisualCCommon.EBrowseInformation ICCompilerOptions.BrowseInformation
        {
            get
            {
                return this.GetValueTypeOption<VisualCCommon.EBrowseInformation>("BrowseInformation", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<VisualCCommon.EBrowseInformation>("BrowseInformation", value);
                this.ProcessNamedSetHandler("BrowseInformationSetHandler", this["BrowseInformation"]);
            }
        }
        bool ICCompilerOptions.StringPooling
        {
            get
            {
                return this.GetValueTypeOption<bool>("StringPooling", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("StringPooling", value);
                this.ProcessNamedSetHandler("StringPoolingSetHandler", this["StringPooling"]);
            }
        }
        bool ICCompilerOptions.DisableLanguageExtensions
        {
            get
            {
                return this.GetValueTypeOption<bool>("DisableLanguageExtensions", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("DisableLanguageExtensions", value);
                this.ProcessNamedSetHandler("DisableLanguageExtensionsSetHandler", this["DisableLanguageExtensions"]);
            }
        }
        bool ICCompilerOptions.ForceConformanceInForLoopScope
        {
            get
            {
                return this.GetValueTypeOption<bool>("ForceConformanceInForLoopScope", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("ForceConformanceInForLoopScope", value);
                this.ProcessNamedSetHandler("ForceConformanceInForLoopScopeSetHandler", this["ForceConformanceInForLoopScope"]);
            }
        }
        bool ICCompilerOptions.UseFullPaths
        {
            get
            {
                return this.GetValueTypeOption<bool>("UseFullPaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("UseFullPaths", value);
                this.ProcessNamedSetHandler("UseFullPathsSetHandler", this["UseFullPaths"]);
            }
        }
        EManagedCompilation ICCompilerOptions.CompileAsManaged
        {
            get
            {
                return this.GetValueTypeOption<EManagedCompilation>("CompileAsManaged", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<EManagedCompilation>("CompileAsManaged", value);
                this.ProcessNamedSetHandler("CompileAsManagedSetHandler", this["CompileAsManaged"]);
            }
        }
        EBasicRuntimeChecks ICCompilerOptions.BasicRuntimeChecks
        {
            get
            {
                return this.GetValueTypeOption<EBasicRuntimeChecks>("BasicRuntimeChecks", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<EBasicRuntimeChecks>("BasicRuntimeChecks", value);
                this.ProcessNamedSetHandler("BasicRuntimeChecksSetHandler", this["BasicRuntimeChecks"]);
            }
        }
        bool ICCompilerOptions.SmallerTypeConversionRuntimeCheck
        {
            get
            {
                return this.GetValueTypeOption<bool>("SmallerTypeConversionRuntimeCheck", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("SmallerTypeConversionRuntimeCheck", value);
                this.ProcessNamedSetHandler("SmallerTypeConversionRuntimeCheckSetHandler", this["SmallerTypeConversionRuntimeCheck"]);
            }
        }
        EInlineFunctionExpansion ICCompilerOptions.InlineFunctionExpansion
        {
            get
            {
                return this.GetValueTypeOption<EInlineFunctionExpansion>("InlineFunctionExpansion", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<EInlineFunctionExpansion>("InlineFunctionExpansion", value);
                this.ProcessNamedSetHandler("InlineFunctionExpansionSetHandler", this["InlineFunctionExpansion"]);
            }
        }
        bool ICCompilerOptions.EnableIntrinsicFunctions
        {
            get
            {
                return this.GetValueTypeOption<bool>("EnableIntrinsicFunctions", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("EnableIntrinsicFunctions", value);
                this.ProcessNamedSetHandler("EnableIntrinsicFunctionsSetHandler", this["EnableIntrinsicFunctions"]);
            }
        }
        ERuntimeLibrary ICCompilerOptions.RuntimeLibrary
        {
            get
            {
                return this.GetValueTypeOption<ERuntimeLibrary>("RuntimeLibrary", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<ERuntimeLibrary>("RuntimeLibrary", value);
                this.ProcessNamedSetHandler("RuntimeLibrarySetHandler", this["RuntimeLibrary"]);
            }
        }
        Opus.Core.StringArray ICCompilerOptions.ForcedInclude
        {
            get
            {
                return this.GetReferenceTypeOption<Opus.Core.StringArray>("ForcedInclude", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Opus.Core.StringArray>("ForcedInclude", value);
                this.ProcessNamedSetHandler("ForcedIncludeSetHandler", this["ForcedInclude"]);
            }
        }
        #endregion
    }
}

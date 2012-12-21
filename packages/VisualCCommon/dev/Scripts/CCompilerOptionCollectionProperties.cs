// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=..\..\..\C\dev\Scripts\ICCompilerOptions.cs;ICCompilerOptions.cs -n=VisualCCommon -c=CCompilerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs;..\..\..\VisualStudioProcessor\dev\Scripts\VisualStudioDelegate.cs -pv=PrivateData
namespace VisualCCommon
{
    public partial class CCompilerOptionCollection
    {
        #region C.ICCompilerOptions Option properties
        C.DefineCollection C.ICCompilerOptions.Defines
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
        Opus.Core.DirectoryCollection C.ICCompilerOptions.IncludePaths
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
        Opus.Core.DirectoryCollection C.ICCompilerOptions.SystemIncludePaths
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
        C.ECompilerOutput C.ICCompilerOptions.OutputType
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
        bool C.ICCompilerOptions.DebugSymbols
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
        bool C.ICCompilerOptions.WarningsAsErrors
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
        bool C.ICCompilerOptions.IgnoreStandardIncludePaths
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
        C.EOptimization C.ICCompilerOptions.Optimization
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
        string C.ICCompilerOptions.CustomOptimization
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
        C.ETargetLanguage C.ICCompilerOptions.TargetLanguage
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
        bool C.ICCompilerOptions.ShowIncludes
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
        string C.ICCompilerOptions.AdditionalOptions
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
        bool C.ICCompilerOptions.OmitFramePointer
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
        Opus.Core.StringArray C.ICCompilerOptions.DisableWarnings
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
        C.ECharacterSet C.ICCompilerOptions.CharacterSet
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
        #endregion
        #region ICCompilerOptions Option properties
        bool ICCompilerOptions.NoLogo
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
        bool ICCompilerOptions.MinimalRebuild
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
        VisualCCommon.EWarningLevel ICCompilerOptions.WarningLevel
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
        VisualCCommon.EDebugType ICCompilerOptions.DebugType
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
        VisualCCommon.EBrowseInformation ICCompilerOptions.BrowseInformation
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
        bool ICCompilerOptions.StringPooling
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
        bool ICCompilerOptions.DisableLanguageExtensions
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
        bool ICCompilerOptions.ForceConformanceInForLoopScope
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
        bool ICCompilerOptions.UseFullPaths
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
        EManagedCompilation ICCompilerOptions.CompileAsManaged
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
        EBasicRuntimeChecks ICCompilerOptions.BasicRuntimeChecks
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
        bool ICCompilerOptions.SmallerTypeConversionRuntimeCheck
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
        EInlineFunctionExpansion ICCompilerOptions.InlineFunctionExpansion
        {
            get
            {
                return this.GetValueTypeOption<EInlineFunctionExpansion>("InlineFunctionExpansion");
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
                return this.GetValueTypeOption<bool>("EnableIntrinsicFunctions");
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
                return this.GetValueTypeOption<ERuntimeLibrary>("RuntimeLibrary");
            }
            set
            {
                this.SetValueTypeOption<ERuntimeLibrary>("RuntimeLibrary", value);
                this.ProcessNamedSetHandler("RuntimeLibrarySetHandler", this["RuntimeLibrary"]);
            }
        }
        #endregion
    }
}

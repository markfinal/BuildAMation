// Automatically generated file from OpusOptionCodeGenerator. DO NOT EDIT.
// Command line:
// -i=../../../C/dev/Scripts/ICCompilerOptions.cs:ICCompilerOptions.cs -n=ClangCommon -c=CCompilerOptionCollection -p -d -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs -pv=PrivateData
namespace ClangCommon
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
        C.ELanguageStandard C.ICCompilerOptions.LanguageStandard
        {
            get
            {
                return this.GetValueTypeOption<C.ELanguageStandard>("LanguageStandard");
            }
            set
            {
                this.SetValueTypeOption<C.ELanguageStandard>("LanguageStandard", value);
                this.ProcessNamedSetHandler("LanguageStandardSetHandler", this["LanguageStandard"]);
            }
        }
        #endregion
        #region ICCompilerOptions Option properties
        bool ICCompilerOptions.PositionIndependentCode
        {
            get
            {
                return this.GetValueTypeOption<bool>("PositionIndependentCode");
            }
            set
            {
                this.SetValueTypeOption<bool>("PositionIndependentCode", value);
                this.ProcessNamedSetHandler("PositionIndependentCodeSetHandler", this["PositionIndependentCode"]);
            }
        }
        #endregion
    }
}

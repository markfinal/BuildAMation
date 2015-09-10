#region License
// Copyright (c) 2010-2015, Mark Final
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of BuildAMation nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion // License
#region BamOptionGenerator
// Automatically generated file from BamOptionGenerator. DO NOT EDIT.
// Command line arguments:
//     -i=../../../C/dev/Scripts/ICCompilerOptions.cs&ICCompilerOptions.cs
//     -n=MingwCommon
//     -c=CCompilerOptionCollection
//     -p
//     -d
//     -dd=../../../CommandLineProcessor/dev/Scripts/CommandLineDelegate.cs
//     -pv=PrivateData
#endregion // BamOptionGenerator
namespace MingwCommon
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
        Bam.Core.DirectoryCollection C.ICCompilerOptions.IncludePaths
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.DirectoryCollection>("IncludePaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.DirectoryCollection>("IncludePaths", value);
                this.ProcessNamedSetHandler("IncludePathsSetHandler", this["IncludePaths"]);
            }
        }
        Bam.Core.DirectoryCollection C.ICCompilerOptions.SystemIncludePaths
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.DirectoryCollection>("SystemIncludePaths", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.DirectoryCollection>("SystemIncludePaths", value);
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
        Bam.Core.StringArray C.ICCompilerOptions.DisableWarnings
        {
            get
            {
                return this.GetReferenceTypeOption<Bam.Core.StringArray>("DisableWarnings", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetReferenceTypeOption<Bam.Core.StringArray>("DisableWarnings", value);
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
        bool ICCompilerOptions.AllWarnings
        {
            get
            {
                return this.GetValueTypeOption<bool>("AllWarnings", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("AllWarnings", value);
                this.ProcessNamedSetHandler("AllWarningsSetHandler", this["AllWarnings"]);
            }
        }
        bool ICCompilerOptions.ExtraWarnings
        {
            get
            {
                return this.GetValueTypeOption<bool>("ExtraWarnings", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("ExtraWarnings", value);
                this.ProcessNamedSetHandler("ExtraWarningsSetHandler", this["ExtraWarnings"]);
            }
        }
        bool ICCompilerOptions.StrictAliasing
        {
            get
            {
                return this.GetValueTypeOption<bool>("StrictAliasing", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("StrictAliasing", value);
                this.ProcessNamedSetHandler("StrictAliasingSetHandler", this["StrictAliasing"]);
            }
        }
        bool ICCompilerOptions.InlineFunctions
        {
            get
            {
                return this.GetValueTypeOption<bool>("InlineFunctions", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("InlineFunctions", value);
                this.ProcessNamedSetHandler("InlineFunctionsSetHandler", this["InlineFunctions"]);
            }
        }
        bool ICCompilerOptions.Pedantic
        {
            get
            {
                return this.GetValueTypeOption<bool>("Pedantic", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("Pedantic", value);
                this.ProcessNamedSetHandler("PedanticSetHandler", this["Pedantic"]);
            }
        }
        bool ICCompilerOptions.SixtyFourBit
        {
            get
            {
                return this.GetValueTypeOption<bool>("SixtyFourBit", this.SuperSetOptionCollection);
            }
            set
            {
                this.SetValueTypeOption<bool>("SixtyFourBit", value);
                this.ProcessNamedSetHandler("SixtyFourBitSetHandler", this["SixtyFourBit"]);
            }
        }
        #endregion
    }
}

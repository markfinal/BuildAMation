#region License
// Copyright (c) 2010-2019, Mark Final
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
namespace VisualCCommon
{
    /// <summary>
    /// Abstract class for common C++ compiler settings
    /// </summary>
    public abstract class CommonCxxCompilerSettings :
        CommonCompilerSettings,
        C.ICxxOnlyCompilerSettings
    {
        protected override void
        ModifyDefaults()
        {
            base.ModifyDefaults();

            (this as C.ICommonPreprocessorSettings).TargetLanguage = C.ETargetLanguage.Cxx;
        }

        [CommandLineProcessor.Enum(C.Cxx.EExceptionHandler.Disabled, "")]
        [CommandLineProcessor.Enum(C.Cxx.EExceptionHandler.Asynchronous, "-EHa")]
        [CommandLineProcessor.Enum(C.Cxx.EExceptionHandler.Synchronous, "-EHsc")]
        [CommandLineProcessor.Enum(C.Cxx.EExceptionHandler.SyncWithCExternFunctions, "-EHs")]
        [VisualStudioProcessor.Enum(C.Cxx.EExceptionHandler.Disabled, "ExceptionHandling", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "false")]
        [VisualStudioProcessor.Enum(C.Cxx.EExceptionHandler.Asynchronous, "ExceptionHandling", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "Async")]
        [VisualStudioProcessor.Enum(C.Cxx.EExceptionHandler.Synchronous, "ExceptionHandling", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "Sync")]
        [VisualStudioProcessor.Enum(C.Cxx.EExceptionHandler.Synchronous, "SyncWithCExternFunctions", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "SyncCThrow")]
        C.Cxx.EExceptionHandler? C.ICxxOnlyCompilerSettings.ExceptionHandler { get; set; }

        [CommandLineProcessor.Bool("-GR", "-GR-")]
        [VisualStudioProcessor.Bool("RuntimeTypeInfo")]
        bool? C.ICxxOnlyCompilerSettings.EnableRunTimeTypeInfo { get; set; }

        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.NotSet, "")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx98, "")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx98, "")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx03, "")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx03, "")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx11, "")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx11, "")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx14, "-std:c++14")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx14, "-std:c++14")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx17, "-std:c++17")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx17, "-std:c++17")]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.NotSet, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.Cxx98, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx98, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.Cxx03, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx03, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.Cxx11, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx11, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.Cxx14, "LanguageStandard", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "stdcpp14")]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx14, "LanguageStandard", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "stdcpp14")]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.Cxx17, "LanguageStandard", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "stdcpp17")]
        [VisualStudioProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx17, "LanguageStandard", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "stdcpp17")]
        C.Cxx.ELanguageStandard? C.ICxxOnlyCompilerSettings.LanguageStandard { get; set; }

        [CommandLineProcessor.Enum(C.Cxx.EStandardLibrary.NotSet, "")]
        [CommandLineProcessor.Enum(C.Cxx.EStandardLibrary.libstdcxx, "")]
        [CommandLineProcessor.Enum(C.Cxx.EStandardLibrary.libcxx, "")]
        [VisualStudioProcessor.Enum(C.Cxx.EStandardLibrary.NotSet, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.Cxx.EStandardLibrary.libstdcxx, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.Cxx.EStandardLibrary.libcxx, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        C.Cxx.EStandardLibrary? C.ICxxOnlyCompilerSettings.StandardLibrary { get; set; }
    }
}

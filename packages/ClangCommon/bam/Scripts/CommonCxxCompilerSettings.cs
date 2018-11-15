#region License
// Copyright (c) 2010-2018, Mark Final
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
namespace ClangCommon
{
    public abstract class CommonCxxCompilerSettings :
        ClangCommon.CommonCompilerSettings,
        C.ICxxOnlyCompilerSettings
    {
        protected CommonCxxCompilerSettings(
            Bam.Core.Module module)
            :
            base(module)
        {
            (this as C.ICommonCompilerSettings).TargetLanguage = C.ETargetLanguage.Cxx;
        }

        protected CommonCxxCompilerSettings(
            Bam.Core.Module module,
            bool useDefaults)
            :
            base(module, useDefaults)
        {
            (this as C.ICommonCompilerSettings).TargetLanguage = C.ETargetLanguage.Cxx;
        }

        [CommandLineProcessor.Enum(C.Cxx.EExceptionHandler.Disabled, "-fno-exceptions")]
        [CommandLineProcessor.Enum(C.Cxx.EExceptionHandler.Asynchronous, "-fexceptions")]
        [CommandLineProcessor.Enum(C.Cxx.EExceptionHandler.Synchronous, "-fexceptions")]
        [CommandLineProcessor.Enum(C.Cxx.EExceptionHandler.SyncWithCExternFunctions, "-fexceptions")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.EExceptionHandler.Disabled, "GCC_ENABLE_CPP_EXCEPTIONS", "NO")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.EExceptionHandler.Asynchronous, "GCC_ENABLE_CPP_EXCEPTIONS", "YES")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.EExceptionHandler.Synchronous, "GCC_ENABLE_CPP_EXCEPTIONS", "YES")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.EExceptionHandler.SyncWithCExternFunctions, "GCC_ENABLE_CPP_EXCEPTIONS", "YES")]
        C.Cxx.EExceptionHandler? C.ICxxOnlyCompilerSettings.ExceptionHandler { get; set; }

        [CommandLineProcessor.Bool("-frtti", "-fno-rtti")]
        [XcodeProjectProcessor.UniqueBool("GCC_ENABLE_CPP_RTTI", "YES", "NO")]
        bool? C.ICxxOnlyCompilerSettings.EnableRunTimeTypeInfo { get; set; }

        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.NotSet, "")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx98, "-std=c++98")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx98, "-std=gnu++98")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx03, "-std=c++03")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx03, "-std=gnu++03")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx11, "-std=c++11")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx11, "-std=gnu++11")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx14, "-std=c++14")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx14, "-std=gnu++14")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.Cxx17, "-std=c++17")]
        [CommandLineProcessor.Enum(C.Cxx.ELanguageStandard.GnuCxx17, "-std=gnu++17")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.NotSet, "CLANG_CXX_LANGUAGE_STANDARD", "", ignore: true)]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.Cxx98, "CLANG_CXX_LANGUAGE_STANDARD", "c++98")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.GnuCxx98, "CLANG_CXX_LANGUAGE_STANDARD", "gnu++98")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.Cxx03, "CLANG_CXX_LANGUAGE_STANDARD", "c++03")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.GnuCxx03, "CLANG_CXX_LANGUAGE_STANDARD", "gnu++03")]// TODO: actually not supported
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.Cxx11, "CLANG_CXX_LANGUAGE_STANDARD", "c++11")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.GnuCxx11, "CLANG_CXX_LANGUAGE_STANDARD", "gnu++11")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.Cxx14, "CLANG_CXX_LANGUAGE_STANDARD", "c++14")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.GnuCxx14, "CLANG_CXX_LANGUAGE_STANDARD", "gnu++14")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.Cxx17, "CLANG_CXX_LANGUAGE_STANDARD", "c++17")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.ELanguageStandard.GnuCxx17, "CLANG_CXX_LANGUAGE_STANDARD", "gnu++17")]
        C.Cxx.ELanguageStandard? C.ICxxOnlyCompilerSettings.LanguageStandard { get; set; }

        [CommandLineProcessor.Enum(C.Cxx.EStandardLibrary.NotSet, "")]
        [CommandLineProcessor.Enum(C.Cxx.EStandardLibrary.libstdcxx, "-stdlib=libstdc++")]
        [CommandLineProcessor.Enum(C.Cxx.EStandardLibrary.libcxx, "-stdlib=libc++")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.EStandardLibrary.NotSet, "CLANG_CXX_LIBRARY", "", ignore: true)]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.EStandardLibrary.libstdcxx, "CLANG_CXX_LIBRARY", "libstdc++")]
        [XcodeProjectProcessor.UniqueEnum(C.Cxx.EStandardLibrary.libcxx, "CLANG_CXX_LIBRARY", "libc++")]
        C.Cxx.EStandardLibrary? C.ICxxOnlyCompilerSettings.StandardLibrary { get; set; }

        public override void Validate()
        {
            base.Validate();
            if ((this as C.ICxxOnlyCompilerSettings).LanguageStandard == C.Cxx.ELanguageStandard.GnuCxx03)
            {
                throw new Bam.Core.Exception("Gnu C++03 language standard not supported");
            }
        }
    }
}

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
namespace ClangCommon
{
    /// <summary>
    /// Abstract class for common Clang compiler settings
    /// </summary>
    [CommandLineProcessor.OutputPath(C.ObjectFileBase.ObjectFileKey, "-o ")]
    [CommandLineProcessor.InputPaths(C.SourceFile.SourceFileKey, "", max_file_count: 1)]
    abstract class CommonCompilerSettings :
        C.SettingsBase,
        C.ICommonPreprocessorSettings,
        C.ICommonCompilerSettings,
        C.ICommonCompilerSettingsOSX,
        C.IAdditionalSettings,
        ICommonCompilerSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected CommonCompilerSettings()
            :
            base(ELayout.Cmds_Outputs_Inputs)
        {}

        protected override void
        ModifyDefaults()
        {
            base.ModifyDefaults();

            // not in the defaults in the C package to avoid a compile-time dependency on the Clang package
            (this as C.ICommonCompilerSettingsOSX).MacOSXMinimumVersionSupported =
                Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang").MacOSXMinimumVersionSupported;
        }

        [CommandLineProcessor.PreprocessorDefines("-D")]
        [XcodeProjectProcessor.PreprocessorDefines("GCC_PREPROCESSOR_DEFINITIONS")]
        C.PreprocessorDefinitions C.ICommonPreprocessorSettings.PreprocessorDefines { get; set; }

        [CommandLineProcessor.PathArray("-iquote")]
        [XcodeProjectProcessor.PathArray("USER_HEADER_SEARCH_PATHS")]
        Bam.Core.TokenizedStringArray C.ICommonPreprocessorSettings.IncludePaths { get; set; }

        [CommandLineProcessor.PathArray("-isystem")]
        [XcodeProjectProcessor.PathArray("SYSTEM_HEADER_SEARCH_PATHS")]
        Bam.Core.TokenizedStringArray C.ICommonPreprocessorSettings.SystemIncludePaths { get; set; }

        [CommandLineProcessor.StringArray("-U")]
        [XcodeProjectProcessor.StringArray("OTHER_CFLAGS", prefix: "-U")]
        Bam.Core.StringArray C.ICommonPreprocessorSettings.PreprocessorUndefines { get; set; }

        [CommandLineProcessor.Enum(C.ETargetLanguage.Default, "")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.C, "-x c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.Cxx, "-x c++")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveC, "-x objective-c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveCxx, "-x objective-c++")]
        [XcodeProjectProcessor.UniqueEnum(C.ETargetLanguage.Default, "GCC_INPUT_FILETYPE", "automatic")]
        [XcodeProjectProcessor.UniqueEnum(C.ETargetLanguage.C, "GCC_INPUT_FILETYPE", "sourcecode.c.c")]
        [XcodeProjectProcessor.UniqueEnum(C.ETargetLanguage.Cxx, "GCC_INPUT_FILETYPE", "sourcecode.cpp.cpp")]
        [XcodeProjectProcessor.UniqueEnum(C.ETargetLanguage.ObjectiveC, "GCC_INPUT_FILETYPE", "sourcecode.c.objc")]
        [XcodeProjectProcessor.UniqueEnum(C.ETargetLanguage.ObjectiveCxx, "GCC_INPUT_FILETYPE", "sourcecode.cpp.objcpp")]
        C.ETargetLanguage? C.ICommonPreprocessorSettings.TargetLanguage { get; set; }

        [CommandLineProcessor.Bool("-P", "")]
        [XcodeProjectProcessor.MultiBool("OTHER_CFLAGS", "-P", "")]
        bool? C.ICommonPreprocessorSettings.SuppressLineMarkers { get; set; }

        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "-arch i386")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "-arch x86_64")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.ThirtyTwo, "VALID_ARCHS", "i386", "ARCHS", "$(ARCHS_STANDARD_32_BIT)")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.SixtyFour, "VALID_ARCHS", "x86_64", "ARCHS", "$(ARCHS_STANDARD_64_BIT)")]
        C.EBit? C.ICommonCompilerSettings.Bits { get; set; }

        [CommandLineProcessor.Bool("-g", "")]
        [XcodeProjectProcessor.UniqueBool("GCC_GENERATE_DEBUGGING_SYMBOLS", "YES", "NO")]
        bool? C.ICommonCompilerSettings.DebugSymbols { get; set; }

        [CommandLineProcessor.Bool("-Werror", "-Wno-error")]
        [XcodeProjectProcessor.UniqueBool("GCC_TREAT_WARNINGS_AS_ERRORS", "YES", "NO")]
        bool? C.ICommonCompilerSettings.WarningsAsErrors { get; set; }

        [CommandLineProcessor.Enum(C.EOptimization.Off, "-O0")]
        [CommandLineProcessor.Enum(C.EOptimization.Size, "-Os")] // TODO: is this right?
        [CommandLineProcessor.Enum(C.EOptimization.Speed, "-O2")]
        [CommandLineProcessor.Enum(C.EOptimization.Custom, "")]
        [XcodeProjectProcessor.UniqueEnum(C.EOptimization.Off, "GCC_OPTIMIZATION_LEVEL", "0")]
        [XcodeProjectProcessor.UniqueEnum(C.EOptimization.Size, "GCC_OPTIMIZATION_LEVEL", "s")]
        [XcodeProjectProcessor.UniqueEnum(C.EOptimization.Speed, "GCC_OPTIMIZATION_LEVEL", "2")]
        [XcodeProjectProcessor.UniqueEnum(C.EOptimization.Custom, "GCC_OPTIMIZATION_LEVEL", "", ignore: true)]
        C.EOptimization? C.ICommonCompilerSettings.Optimization { get; set; }

        [CommandLineProcessor.Bool("-fomit-frame-pointer", "-fno-omit-frame-pointer")]
        [XcodeProjectProcessor.MultiBool("OTHER_CFLAGS", "-fomit-frame-pointer", "-fno-omit-frame-pointer")]
        bool? C.ICommonCompilerSettings.OmitFramePointer { get; set; }

        [CommandLineProcessor.StringArray("-Wno-")]
        [XcodeProjectProcessor.StringArray("WARNING_CFLAGS", prefix: "-Wno-")]
        Bam.Core.StringArray C.ICommonCompilerSettings.DisableWarnings { get; set; }

        [CommandLineProcessor.StringArray("-include ")]
        [XcodeProjectProcessor.StringArray("OTHER_CFLAGS", prefix: "-include ")]
        Bam.Core.StringArray C.ICommonCompilerSettings.NamedHeaders { get; set; }

        [CommandLineProcessor.Bool("-E", "-c")]
        [XcodeProjectProcessor.MultiBool("OTHER_CFLAGS", "-E", "")]
        bool? C.ICommonCompilerSettings.PreprocessOnly { get; set; }

        [CommandLineProcessor.PathArray("-F")]
        [XcodeProjectProcessor.PathArray("FRAMEWORK_SEARCH_PATHS")]
        Bam.Core.TokenizedStringArray C.ICommonCompilerSettingsOSX.FrameworkSearchPaths { get; set; }

        [CommandLineProcessor.String("-mmacosx-version-min=")]
        [XcodeProjectProcessor.String("", ignore: true)]
        string C.ICommonCompilerSettingsOSX.MacOSXMinimumVersionSupported { get; set; }

        [CommandLineProcessor.StringArray("")]
        [XcodeProjectProcessor.StringArray("OTHER_CFLAGS", spacesSeparate: true)] // TODO: would be handy to guide to OTHER_CPLUSPLUSFLAGS for C++
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings { get; set; }

        [CommandLineProcessor.Bool("-Wall", "-Wno-all")]
        [XcodeProjectProcessor.MultiBool("WARNING_CFLAGS", "-Wall", "-Wno-all")]
        bool? ICommonCompilerSettings.AllWarnings { get; set; }

        [CommandLineProcessor.Bool("-Wextra", "-Wno-extra")]
        [XcodeProjectProcessor.MultiBool("WARNING_CFLAGS", "-Wextra", "-Wno-extra")]
        bool? ICommonCompilerSettings.ExtraWarnings { get; set; }

        [CommandLineProcessor.Bool("-Wpedantic", "-Wno-pedantic")]
        [XcodeProjectProcessor.UniqueBool("GCC_WARN_PEDANTIC", "YES", "NO")]
        bool? ICommonCompilerSettings.Pedantic { get; set; }

        [CommandLineProcessor.Enum(EVisibility.Default, "-fvisibility=default")]
        [CommandLineProcessor.Enum(EVisibility.Hidden, "-fvisibility=hidden")]
        [CommandLineProcessor.Enum(EVisibility.Internal, "-fvisibility=internal")]
        [CommandLineProcessor.Enum(EVisibility.Protected, "-fvisibility=protected")]
        [XcodeProjectProcessor.UniqueEnum(EVisibility.Default, "GCC_SYMBOLS_PRIVATE_EXTERN", "NO")]
        [XcodeProjectProcessor.UniqueEnum(EVisibility.Hidden, "GCC_SYMBOLS_PRIVATE_EXTERN", "YES")]
        [XcodeProjectProcessor.UniqueEnum(EVisibility.Internal, "GCC_SYMBOLS_PRIVATE_EXTERN", "YES")]
        [XcodeProjectProcessor.UniqueEnum(EVisibility.Protected, "GCC_SYMBOLS_PRIVATE_EXTERN", "YES")]
        EVisibility? ICommonCompilerSettings.Visibility { get; set; }

        [CommandLineProcessor.Bool("-fstrict-aliasing", "-fno-strict-aliasing")]
        [XcodeProjectProcessor.UniqueBool("GCC_STRICT_ALIASING", "YES", "NO")]
        bool? ICommonCompilerSettings.StrictAliasing { get; set; }

        [CommandLineProcessor.Enum(EOptimization.O1, "-O1")]
        [CommandLineProcessor.Enum(EOptimization.O3, "-O3")]
        [CommandLineProcessor.Enum(EOptimization.Ofast, "-Ofast")]
        [XcodeProjectProcessor.UniqueEnum(EOptimization.O1, "GCC_OPTIMIZATION_LEVEL", "1")]
        [XcodeProjectProcessor.UniqueEnum(EOptimization.O3, "GCC_OPTIMIZATION_LEVEL", "3")]
        [XcodeProjectProcessor.UniqueEnum(EOptimization.Ofast, "GCC_OPTIMIZATION_LEVEL", "fast")]
        EOptimization? ICommonCompilerSettings.Optimization { get; set; }

        public override void
        Validate()
        {
            base.Validate();

            if ((this as ICommonCompilerSettings).Optimization.HasValue &&
                (this as C.ICommonCompilerSettings).Optimization != C.EOptimization.Custom)
            {
                throw new Bam.Core.Exception(
                    "Compiler specific optimizations can only be set when the common optimization is C.EOptimization.Custom"
                );
            }
        }
    }
}

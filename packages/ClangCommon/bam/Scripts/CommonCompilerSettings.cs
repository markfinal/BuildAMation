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
    public abstract class CommonCompilerSettings :
        C.SettingsBase,
#if BAM_V2
#else
        CommandLineProcessor.IConvertToCommandLine,
#endif
        XcodeProjectProcessor.IConvertToProject,
        C.ICommonHasSourcePath,
        C.ICommonHasOutputPath,
        C.ICommonHasCompilerPreprocessedOutputPath,
        C.ICommonCompilerSettings,
        C.ICommonCompilerSettingsOSX,
        C.IAdditionalSettings,
        ICommonCompilerSettings
    {
        protected CommonCompilerSettings(
            Bam.Core.Module module)
            :
            this(module, true)
        {}

        protected CommonCompilerSettings(
            Bam.Core.Module module,
            bool useDefaults)
        {
            this.InitializeAllInterfaces(module, true, useDefaults);

            // not in the defaults in the C package to avoid a compile-time dependency on the Clang package
            (this as C.ICommonCompilerSettingsOSX).MacOSMinimumVersionSupported =
                Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang").MinimumVersionSupported;
        }

#if BAM_V2
#else
        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.StringArray commandLine)
        {
            CommandLineProcessor.Conversion.Convert(typeof(CommandLineCompilerImplementation), this, commandLine);
        }
#endif

        void
        XcodeProjectProcessor.IConvertToProject.Convert(
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            XcodeProjectProcessor.Conversion.Convert(typeof(XcodeCompilerImplementation), this, module, configuration);
        }

#if BAM_V2
        [CommandLineProcessor.Path("")]
#endif
        Bam.Core.TokenizedString C.ICommonHasSourcePath.SourcePath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-c -o ")]
#endif
        Bam.Core.TokenizedString C.ICommonHasOutputPath.OutputPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-E -o ")]
#endif
        Bam.Core.TokenizedString C.ICommonHasCompilerPreprocessedOutputPath.PreprocessedOutputPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.EnumAttribute(C.EBit.ThirtyTwo, "-arch i386")]
        [CommandLineProcessor.EnumAttribute(C.EBit.SixtyFour, "-arch x86_64")]
#endif
        C.EBit? C.ICommonCompilerSettings.Bits
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PreprocessorDefines("-D")]
#endif
        C.PreprocessorDefinitions C.ICommonCompilerSettings.PreprocessorDefines
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-I")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonCompilerSettings.IncludePaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-I")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonCompilerSettings.SystemIncludePaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-g", "")]
#endif
        bool? C.ICommonCompilerSettings.DebugSymbols
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Werror", "-Wno-error")]
#endif
        bool? C.ICommonCompilerSettings.WarningsAsErrors
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.EOptimization.Off, "-O0")]
        [CommandLineProcessor.Enum(C.EOptimization.Size, "-Os")] // TODO: is this right?
        [CommandLineProcessor.Enum(C.EOptimization.Speed, "-O2")]
        [CommandLineProcessor.Enum(C.EOptimization.Custom, "")]
#endif
        C.EOptimization? C.ICommonCompilerSettings.Optimization
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ETargetLanguage.Default, "")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.C, "-x c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.Cxx, "-x c++")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveC, "-x objective-c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveCxx, "-x objective-c++")]
#endif
        C.ETargetLanguage? C.ICommonCompilerSettings.TargetLanguage
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-fomit-frame-pointer", "-fno-omit-frame-pointer")]
#endif
        bool? C.ICommonCompilerSettings.OmitFramePointer
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("-Wno-")]
#endif
        Bam.Core.StringArray C.ICommonCompilerSettings.DisableWarnings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("-U")]
#endif
        Bam.Core.StringArray C.ICommonCompilerSettings.PreprocessorUndefines
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("-include ")]
#endif
        Bam.Core.StringArray C.ICommonCompilerSettings.NamedHeaders
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-F")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonCompilerSettingsOSX.FrameworkSearchPaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.String("-mmacosx-version-min=")]
#endif
        string C.ICommonCompilerSettingsOSX.MacOSMinimumVersionSupported
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
#endif
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Wall", "-Wno-all")]
#endif
        bool? ICommonCompilerSettings.AllWarnings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Wextra", "-Wno-extra")]
#endif
        bool? ICommonCompilerSettings.ExtraWarnings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Wpedantic", "-Wno-pedantic")]
#endif
        bool? ICommonCompilerSettings.Pedantic
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(EVisibility.Default, "-fvisibility=default")]
        [CommandLineProcessor.Enum(EVisibility.Hidden, "-fvisibility=hidden")]
        [CommandLineProcessor.Enum(EVisibility.Internal, "-fvisibility=internal")]
        [CommandLineProcessor.Enum(EVisibility.Protected, "-fvisibility=protected")]
#endif
        EVisibility? ICommonCompilerSettings.Visibility
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-fstrict-aliasing", "-fno-strict-aliasing")]
#endif
        bool? ICommonCompilerSettings.StrictAliasing
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(EOptimization.O1, "-O1")]
        [CommandLineProcessor.Enum(EOptimization.O3, "-O3")]
        [CommandLineProcessor.Enum(EOptimization.Ofast, "-Ofast")]
#endif
        EOptimization? ICommonCompilerSettings.Optimization
        {
            get;
            set;
        }

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

            if (((this as C.ICommonHasOutputPath).OutputPath != null) &&
                ((this as C.ICommonHasCompilerPreprocessedOutputPath).PreprocessedOutputPath != null))
            {
                throw new Bam.Core.Exception(
                    "Both output and preprocessed output paths cannot be set"
                );
            }
        }
    }
}

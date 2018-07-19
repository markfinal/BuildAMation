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
    [CommandLineProcessor.OutputPath(C.AssembledObjectFile.ObjectFileKey, "-o ")]
    [CommandLineProcessor.InputPaths(C.SourceFile.SourceFileKey, "-c ", max_file_count: 1)]
    public abstract class CommonAssemblerSettings :
        C.SettingsBase,
#if BAM_V2
#else
        CommandLineProcessor.IConvertToCommandLine,
        XcodeProjectProcessor.IConvertToProject,
#endif
#if false
        C.ICommonHasOutputPath,
        C.ICommonHasSourcePath,
#endif
        C.ICommonHasCompilerPreprocessedOutputPath,
        C.ICommonAssemblerSettings,
        C.IAdditionalSettings,
        ICommonAssemblerSettings
    {
        protected CommonAssemblerSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

#if BAM_V2
#else
        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.StringArray commandLine)
        {
            CommandLineProcessor.Conversion.Convert(typeof(CommandLineAssemblerImplementation), this, commandLine);
        }

        void
        XcodeProjectProcessor.IConvertToProject.Convert(
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            XcodeProjectProcessor.Conversion.Convert(typeof(XcodeAssemblerImplementation), this, module, configuration);
        }
#endif

#if false
#if BAM_V2
        [CommandLineProcessor.Path("-c -o ")]
        [XcodeProjectProcessor.Path("", ignore: true)]
#endif
        string C.ICommonHasOutputPath.OutputPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("")]
        [XcodeProjectProcessor.Path("", ignore: true)]
#endif
        Bam.Core.TokenizedString C.ICommonHasSourcePath.SourcePath
        {
            get;
            set;
        }
#endif

#if BAM_V2
        [CommandLineProcessor.Path("-E -o ")]
        [XcodeProjectProcessor.Path("", ignore: true)]
#endif
        Bam.Core.TokenizedString C.ICommonHasCompilerPreprocessedOutputPath.PreprocessedOutputPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "-arch i386")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "-arch x86_64")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.ThirtyTwo, "VALID_ARCHS", "i386", "ARCHS", "$(ARCHS_STANDARD_32_BIT)")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.SixtyFour, "VALID_ARCHS", "x86_64", "ARCHS", "$(ARCHS_STANDARD_64_BIT)")]
#endif
        C.EBit? C.ICommonAssemblerSettings.Bits
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-g", "")]
        [XcodeProjectProcessor.UniqueBool("GCC_GENERATE_DEBUGGING_SYMBOLS", "YES", "NO")]
#endif
        bool C.ICommonAssemblerSettings.DebugSymbols
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Werror", "-Wno-error")]
        [XcodeProjectProcessor.UniqueBool("GCC_TREAT_WARNINGS_AS_ERRORS", "YES", "NO")]
#endif
        bool C.ICommonAssemblerSettings.WarningsAsErrors
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-I")]
        [XcodeProjectProcessor.PathArray("USER_HEADER_SEARCH_PATHS")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonAssemblerSettings.IncludePaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PreprocessorDefines("-D")]
        [XcodeProjectProcessor.PreprocessorDefines("GCC_PREPROCESSOR_DEFINITIONS")]
#endif
        C.PreprocessorDefinitions C.ICommonAssemblerSettings.PreprocessorDefines
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
        [XcodeProjectProcessor.StringArray("OTHER_CFLAGS", spacesSeparate: true)]
#endif
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

        public override void
        Validate()
        {
            base.Validate();

            if (((this is C.ICommonHasOutputPath) && (this as C.ICommonHasOutputPath).OutputPath != null) &&
                ((this as C.ICommonHasCompilerPreprocessedOutputPath).PreprocessedOutputPath != null))
            {
                throw new Bam.Core.Exception(
                    "Both output and preprocessed output paths cannot be set"
                );
            }
        }

        public override void
        AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Inputs_Outputs;
        }
    }
}

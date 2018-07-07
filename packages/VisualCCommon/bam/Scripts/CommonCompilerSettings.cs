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
using System.Linq;
namespace VisualCCommon
{
    public abstract class CommonCompilerSettings :
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        VisualStudioProcessor.IConvertToProject,
        C.ICommonHasSourcePath,
        C.ICommonHasOutputPath,
        C.ICommonCompilerSettingsWin,
        C.ICommonCompilerSettings,
        C.IAdditionalSettings,
        ICommonCompilerSettings
    {
        protected CommonCompilerSettings(
            Bam.Core.Module module)
            : this(module, true)
        {
        }

        protected CommonCompilerSettings(
            Bam.Core.Module module,
            bool useDefaults)
        {
            this.InitializeAllInterfaces(module, true, useDefaults);
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.StringArray commandLine)
        {
            CommandLineProcessor.Conversion.Convert(typeof(CommandLineImplementation), this, commandLine);
        }

        void
        VisualStudioProcessor.IConvertToProject.Convert(
            Bam.Core.Module module,
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            string condition)
        {
            VisualStudioProcessor.Conversion.Convert(typeof(VSSolutionImplementation), this, module, vsSettingsGroup, condition);
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ECharacterSet.NotSet, "")]
        [CommandLineProcessor.Enum(C.ECharacterSet.Unicode, "-DUNICODE -D_UNICODE")]
        [CommandLineProcessor.Enum(C.ECharacterSet.MultiByte, "-D_MBCS")]
#endif
        C.ECharacterSet? C.ICommonCompilerSettingsWin.CharacterSet
        {
            get;
            set;
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
        [CommandLineProcessor.Path("-Fo")]
#endif
        Bam.Core.TokenizedString C.ICommonHasOutputPath.OutputPath
        {
            get;
            set;
        }

        // no attributes as this mapping is in which compiler executable is used
        C.EBit? C.ICommonCompilerSettings.Bits
        {
            get;
            set;
        }

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
        [CommandLineProcessor.Enum(C.ECompilerOutput.CompileOnly, "-c")]
        [CommandLineProcessor.Enum(C.ECompilerOutput.Preprocess, "-E")]
        [VisualStudioProcessor.Enum(C.ECompilerOutput.CompileOnly, "ObjectFileName")]
        [VisualStudioProcessor.Enum(C.ECompilerOutput.Preprocess, "PreprocessToFile")]
#endif
        C.ECompilerOutput? C.ICommonCompilerSettings.OutputType
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Z7", "")]
#endif
        bool? C.ICommonCompilerSettings.DebugSymbols
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-WX", "-WX-")]
#endif
        bool? C.ICommonCompilerSettings.WarningsAsErrors
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.EOptimization.Off, "-Od")]
        [CommandLineProcessor.Enum(C.EOptimization.Size, "-O1")]
        [CommandLineProcessor.Enum(C.EOptimization.Speed, "-O2")]
        [CommandLineProcessor.Enum(C.EOptimization.Custom, "")] // deferred for compiler specific optimisation setting
#endif
        C.EOptimization? C.ICommonCompilerSettings.Optimization
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ETargetLanguage.C, "-TC")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.Cxx, "-TP")]
        // other dialects not supported
#endif
        C.ETargetLanguage? C.ICommonCompilerSettings.TargetLanguage
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Oy", "-Oy-")]
#endif
        bool? C.ICommonCompilerSettings.OmitFramePointer
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("-wd")]
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
        [CommandLineProcessor.StringArray("-FI")]
#endif
        Bam.Core.StringArray C.ICommonCompilerSettings.NamedHeaders
        {
            get;
            set;
        }

        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-nologo", "")]
#endif
        bool? ICommonCompilerSettings.NoLogo
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(ERuntimeLibrary.MultiThreaded, "-MT")]
        [CommandLineProcessor.Enum(ERuntimeLibrary.MultiThreadedDebug, "-MTd")]
        [CommandLineProcessor.Enum(ERuntimeLibrary.MultiThreadedDebugDLL, "-MDd")]
        [CommandLineProcessor.Enum(ERuntimeLibrary.MultiThreadedDLL, "-MD")]
#endif
        ERuntimeLibrary? ICommonCompilerSettings.RuntimeLibrary
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(EWarningLevel.Level0, "-W0")]
        [CommandLineProcessor.Enum(EWarningLevel.Level1, "-W1")]
        [CommandLineProcessor.Enum(EWarningLevel.Level2, "-W2")]
        [CommandLineProcessor.Enum(EWarningLevel.Level3, "-W3")]
        [CommandLineProcessor.Enum(EWarningLevel.Level4, "-W4")]
#endif
        EWarningLevel? ICommonCompilerSettings.WarningLevel
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Za", "")]
#endif
        bool? ICommonCompilerSettings.EnableLanguageExtensions
        {
            get;
            set;
        }

#if BAM_V2
        // TODO: have lost the prerequisite that C.ICommonCompilerSettings.Optimization is Custom
        [CommandLineProcessor.Enum(EOptimization.Full, "-Ox")]
#endif
        EOptimization? ICommonCompilerSettings.Optimization
        {
            get;
            set;
        }

        bool? ICommonCompilerSettings.IncreaseObjectFileSectionCount
        {
            get;
            set;
        }
    }
}

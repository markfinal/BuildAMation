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
namespace VisualC
{
    public sealed class CxxCompilerSettings :
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        VisualStudioProcessor.IConvertToProject,
        C.ICCompilerOptionsWin,
        C.ICommonCompilerOptions,
        C.ICxxOnlyCompilerOptions,
        VisualCCommon.ICommonCompilerOptions
    {
        public CxxCompilerSettings(
            Bam.Core.Module module)
            : this(module, true)
        {
        }

        public CxxCompilerSettings(
            Bam.Core.Module module,
            bool useDefaults)
        {
            this.InitializeAllInterfaces(module, true, useDefaults);
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.Module module,
            Bam.Core.StringArray commandLine)
        {
            // TODO: iterate in reflection, in a well defined static class
            (this as C.ICCompilerOptionsWin).Convert(module, commandLine);
            (this as C.ICommonCompilerOptions).Convert(module, commandLine);
            (this as C.ICxxOnlyCompilerOptions).Convert(module, commandLine);
            (this as VisualCCommon.ICommonCompilerOptions).Convert(module, commandLine);
        }

        void
        VisualStudioProcessor.IConvertToProject.Convert(
            Bam.Core.Module module,
            VSSolutionBuilder.VSSettingsGroup settings,
            string condition)
        {
            (this as C.ICCompilerOptionsWin).Convert(module, settings, condition);
            (this as C.ICommonCompilerOptions).Convert(module, settings, condition);
            (this as C.ICxxOnlyCompilerOptions).Convert(module, settings, condition);
            (this as VisualCCommon.ICommonCompilerOptions).Convert(module, settings, condition);
        }

        C.ECharacterSet? C.ICCompilerOptionsWin.CharacterSet
        {
            get;
            set;
        }

        C.EBit? C.ICommonCompilerOptions.Bits
        {
            get;
            set;
        }

        C.PreprocessorDefinitions C.ICommonCompilerOptions.PreprocessorDefines
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.TokenizedString> C.ICommonCompilerOptions.IncludePaths
        {
            get;
            set;
        }

        Bam.Core.Array<Bam.Core.TokenizedString> C.ICommonCompilerOptions.SystemIncludePaths
        {
            get;
            set;
        }

        C.ECompilerOutput? C.ICommonCompilerOptions.OutputType
        {
            get;
            set;
        }

        bool? C.ICommonCompilerOptions.DebugSymbols
        {
            get;
            set;
        }

        bool? C.ICommonCompilerOptions.WarningsAsErrors
        {
            get;
            set;
        }

        C.EOptimization? C.ICommonCompilerOptions.Optimization
        {
            get;
            set;
        }

        C.ETargetLanguage? C.ICommonCompilerOptions.TargetLanguage
        {
            get;
            set;
        }

        bool? C.ICommonCompilerOptions.OmitFramePointer
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonCompilerOptions.DisableWarnings
        {
            get;
            set;
        }

        Bam.Core.StringArray C.ICommonCompilerOptions.PreprocessorUndefines
        {
            get;
            set;
        }

        C.Cxx.EExceptionHandler? C.ICxxOnlyCompilerOptions.ExceptionHandler
        {
            get;
            set;
        }

        C.Cxx.ELanguageStandard? C.ICxxOnlyCompilerOptions.LanguageStandard
        {
            get;
            set;
        }

        C.Cxx.EStandardLibrary? C.ICxxOnlyCompilerOptions.StandardLibrary
        {
            get;
            set;
        }

        bool? VisualCCommon.ICommonCompilerOptions.NoLogo
        {
            get;
            set;
        }
    }
}

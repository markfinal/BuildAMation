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
namespace GccCommon
{
    /// <summary>
    /// Abstract class representing the common Gcc compiler settings
    /// </summary>
    [CommandLineProcessor.OutputPath(C.ObjectFileBase.ObjectFileKey, "-o ")]
    [CommandLineProcessor.InputPaths(C.SourceFile.SourceFileKey, "", max_file_count: 1)]
    public abstract class CommonCompilerSettings :
        C.SettingsBase,
        C.ICommonPreprocessorSettings,
        C.ICommonCompilerSettings,
        C.IAdditionalSettings,
        ICommonCompilerSettings
    {
        /// <summary>
        /// Create a settings instance
        /// </summary>
        /// <param name="module">for this Module</param>
        protected CommonCompilerSettings(
            Bam.Core.Module module)
            :
            this(module, true)
        {}

        /// <summary>
        /// Create a settings instance
        /// </summary>
        /// <param name="module">for this Module</param>
        /// <param name="useDefaults">using defaults</param>
        protected CommonCompilerSettings(
            Bam.Core.Module module,
            bool useDefaults)
        {
            this.InitializeAllInterfaces(module, true, useDefaults);
        }

        [CommandLineProcessor.PreprocessorDefines("-D")]
        C.PreprocessorDefinitions C.ICommonPreprocessorSettings.PreprocessorDefines { get; set; }

        [CommandLineProcessor.PathArray("-iquote")]
        Bam.Core.TokenizedStringArray C.ICommonPreprocessorSettings.IncludePaths { get; set; }

        [CommandLineProcessor.PathArray("-isystem")]
        Bam.Core.TokenizedStringArray C.ICommonPreprocessorSettings.SystemIncludePaths { get; set; }

        [CommandLineProcessor.StringArray("-U")]
        Bam.Core.StringArray C.ICommonPreprocessorSettings.PreprocessorUndefines { get; set; }

        [CommandLineProcessor.Enum(C.ETargetLanguage.C, "-x c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.Cxx, "-x c++")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveC, "-x objective-c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveCxx, "-x objective-c++")]
        C.ETargetLanguage? C.ICommonPreprocessorSettings.TargetLanguage { get; set; }

        [CommandLineProcessor.Bool("", "-P")]
        bool? C.ICommonPreprocessorSettings.SuppressLineMarkers { get; set; }

        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "-m32")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "-m64")]
        C.EBit? C.ICommonCompilerSettings.Bits { get; set; }

        [CommandLineProcessor.Bool("-g", "")]
        bool? C.ICommonCompilerSettings.DebugSymbols { get; set; }

        [CommandLineProcessor.Bool("-Werror", "-Wno-error")]
        bool? C.ICommonCompilerSettings.WarningsAsErrors { get; set; }

        [CommandLineProcessor.Enum(C.EOptimization.Off, "-O0")]
        [CommandLineProcessor.Enum(C.EOptimization.Size, "-O1")]
        [CommandLineProcessor.Enum(C.EOptimization.Speed, "-O2")]
        [CommandLineProcessor.Enum(C.EOptimization.Custom, "")] // Gcc specific optimisation
        C.EOptimization? C.ICommonCompilerSettings.Optimization { get; set; }

        [CommandLineProcessor.Bool("-fomit-frame-pointer", "-fno-omit-frame-pointer")]
        bool? C.ICommonCompilerSettings.OmitFramePointer { get; set; }

        [CommandLineProcessor.StringArray("-Wno-")]
        Bam.Core.StringArray C.ICommonCompilerSettings.DisableWarnings { get; set; }

        [CommandLineProcessor.StringArray("-include ")]
        Bam.Core.StringArray C.ICommonCompilerSettings.NamedHeaders { get; set; }

        [CommandLineProcessor.Bool("-E", "-c")]
        bool? C.ICommonCompilerSettings.PreprocessOnly { get; set; }

        [CommandLineProcessor.StringArray("")]
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings { get; set; }

        [CommandLineProcessor.Bool("-fPIC", "")]
        bool? ICommonCompilerSettings.PositionIndependentCode { get; set; }

        [CommandLineProcessor.Bool("-Wall", "-Wno-all")]
        bool? ICommonCompilerSettings.AllWarnings { get; set; }

        [CommandLineProcessor.Bool("-Wextra", "-Wno-extra")]
        bool? ICommonCompilerSettings.ExtraWarnings { get; set; }

        [CommandLineProcessor.Bool("-Wpedantic", "-Wno-pedantic")]
        bool? ICommonCompilerSettings.Pedantic { get; set; }

        [CommandLineProcessor.Enum(EVisibility.Default, "-fvisibility=default")]
        [CommandLineProcessor.Enum(EVisibility.Hidden, "-fvisibility=hidden")]
        [CommandLineProcessor.Enum(EVisibility.Internal, "-fvisibility=internal")]
        [CommandLineProcessor.Enum(EVisibility.Protected, "-fvisibility=protected")]
        EVisibility? ICommonCompilerSettings.Visibility { get; set; }

        [CommandLineProcessor.Bool("-fstrict-aliasing", "-fno-strict-aliasing")]
        bool? ICommonCompilerSettings.StrictAliasing { get; set; }

        [CommandLineProcessor.Enum(EOptimization.O3, "-O3")]
        [CommandLineProcessor.Enum(EOptimization.Ofast, "-Ofast")]
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

        /// <summary>
        /// Set the layout how command lines are constructed
        /// </summary>
        public override void
        AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }
    }
}

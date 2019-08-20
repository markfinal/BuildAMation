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
    /// Abstract class for common Clang preprocessor settings
    /// </summary>
    [CommandLineProcessor.OutputPath(C.PreprocessedFile.PreprocessedFileKey, "", ignore: true)]
    [CommandLineProcessor.InputPaths(C.SourceFile.SourceFileKey, "-E ", max_file_count: 1)]
    public abstract class CommonPreprocessorSettings :
        C.SettingsBase,
        C.ICommonPreprocessorSettings
    {
        /// <summary>
        /// Create a settings instance
        /// </summary>
        /// <param name="module">for this Module</param>
        protected CommonPreprocessorSettings(
            Bam.Core.Module module)
            :
            this(module, true)
        { }

        /// <summary>
        /// Create a settings instance
        /// </summary>
        /// <param name="module">for this Module</param>
        /// <param name="useDefaults">using defaults</param>
        protected CommonPreprocessorSettings(
            Bam.Core.Module module,
            bool useDefaults)
            :
            base(ELayout.Cmds_Inputs_Outputs)
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

        [CommandLineProcessor.Enum(C.ETargetLanguage.Default, "")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.C, "-x c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.Cxx, "-x c++")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveC, "-x objective-c")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.ObjectiveCxx, "-x objective-c++")]
        C.ETargetLanguage? C.ICommonPreprocessorSettings.TargetLanguage { get; set; }

        [CommandLineProcessor.Bool("-P", "")]
        bool? C.ICommonPreprocessorSettings.SuppressLineMarkers { get; set; }
    }
}

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
namespace WindowsSDKCommon
{
    [CommandLineProcessor.OutputPath(C.ObjectFileBase.ObjectFileKey, "-Fo")]
    [CommandLineProcessor.InputPaths(C.SourceFile.SourceFileKey, "", max_file_count: 1)]
    [VisualStudioProcessor.OutputPath(C.ObjectFileBase.ObjectFileKey, "", handledByMetaData: true)]
    [VisualStudioProcessor.InputPaths(C.SourceFile.SourceFileKey, "", max_file_count: 1, handledByMetaData: true)]
    public abstract class CommonWinResourceCompilerSettings :
        C.SettingsBase,
        C.ICommonWinResourceCompilerSettings,
        C.IAdditionalSettings,
        ICommonWinResourceCompilerSettings
    {
        public CommonWinResourceCompilerSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

        [CommandLineProcessor.Bool("-v", "")]
        [VisualStudioProcessor.Bool("ShowProgress")]
        bool? C.ICommonWinResourceCompilerSettings.Verbose
        {
            get;
            set;
        }

        [CommandLineProcessor.PathArray("-i")]
        [VisualStudioProcessor.PathArray("AdditionalIncludeDirectories", inheritExisting: true)]
        Bam.Core.TokenizedStringArray C.ICommonWinResourceCompilerSettings.IncludePaths
        {
            get;
            set;
        }

        [CommandLineProcessor.PreprocessorDefines("-D")]
        [VisualStudioProcessor.PreprocessorDefines("PreprocessorDefinitions", inheritExisting: true)]
        C.PreprocessorDefinitions C.ICommonWinResourceCompilerSettings.PreprocessorDefines
        {
            get;
            set;
        }

        [CommandLineProcessor.StringArray("")]
        [VisualStudioProcessor.StringArray("AdditionalOptions")]
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

        [CommandLineProcessor.Bool("-NOLOGO", "")]
        [VisualStudioProcessor.Bool("SuppressStartupBanner")]
        bool? ICommonWinResourceCompilerSettings.NoLogo
        {
            get;
            set;
        }

        public override void AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }
    }
}

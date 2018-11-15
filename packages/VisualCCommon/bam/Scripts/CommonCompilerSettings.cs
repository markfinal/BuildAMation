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
    [CommandLineProcessor.OutputPath(C.ObjectFileBase.ObjectFileKey, "-Fo")]
    [CommandLineProcessor.InputPaths(C.SourceFile.SourceFileKey, "", max_file_count: 1)]
    [VisualStudioProcessor.OutputPath(C.ObjectFileBase.ObjectFileKey, "ObjectFileName")]
    [VisualStudioProcessor.InputPaths(C.SourceFile.SourceFileKey, "", max_file_count: 1, handledByMetaData: true)]
    public abstract class CommonCompilerSettings :
        C.SettingsBase,
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
            bool useDefaults) => this.InitializeAllInterfaces(module, true, useDefaults);

        [CommandLineProcessor.Enum(C.ECharacterSet.NotSet, "")]
        [CommandLineProcessor.Enum(C.ECharacterSet.Unicode, "-DUNICODE -D_UNICODE")]
        [CommandLineProcessor.Enum(C.ECharacterSet.MultiByte, "-D_MBCS")]
        [VisualStudioProcessor.Enum(C.ECharacterSet.NotSet, "CharacterSet", VisualStudioProcessor.EnumAttribute.EMode.PassThrough, target: VisualStudioProcessor.BaseAttribute.TargetGroup.Configuration)] // set project wide
        [VisualStudioProcessor.Enum(C.ECharacterSet.Unicode, "CharacterSet", VisualStudioProcessor.EnumAttribute.EMode.PassThrough, target: VisualStudioProcessor.BaseAttribute.TargetGroup.Configuration)] // ditto
        [VisualStudioProcessor.Enum(C.ECharacterSet.MultiByte, "CharacterSet", VisualStudioProcessor.EnumAttribute.EMode.PassThrough, target: VisualStudioProcessor.BaseAttribute.TargetGroup.Configuration)] // ditto
        C.ECharacterSet? C.ICommonCompilerSettingsWin.CharacterSet { get; set; }

        // no attributes as this mapping is in which compiler executable is used
        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "")]
        [VisualStudioProcessor.Enum(C.EBit.ThirtyTwo, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.EBit.SixtyFour, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        C.EBit? C.ICommonCompilerSettings.Bits { get; set; }

        [CommandLineProcessor.PreprocessorDefines("-D")]
        [VisualStudioProcessor.PreprocessorDefines("PreprocessorDefinitions", inheritExisting: true)]
        C.PreprocessorDefinitions C.ICommonCompilerSettings.PreprocessorDefines { get; set; }

        [CommandLineProcessor.PathArray("-I")]
        [VisualStudioProcessor.PathArray("AdditionalIncludeDirectories", inheritExisting: true)]
        Bam.Core.TokenizedStringArray C.ICommonCompilerSettings.IncludePaths { get; set; }

        [CommandLineProcessor.PathArray("-I")]
        [VisualStudioProcessor.PathArray("AdditionalIncludeDirectories", inheritExisting: true)]
        Bam.Core.TokenizedStringArray C.ICommonCompilerSettings.SystemIncludePaths { get; set; }

        [CommandLineProcessor.Bool("-Z7", "")]
        [VisualStudioProcessor.Bool("DebugInformationFormat", "OldStyle", "None")]
        bool? C.ICommonCompilerSettings.DebugSymbols { get; set; }

        [CommandLineProcessor.Bool("-WX", "-WX-")]
        [VisualStudioProcessor.Bool("TreatWarningAsError")]
        bool? C.ICommonCompilerSettings.WarningsAsErrors { get; set; }

        [CommandLineProcessor.Enum(C.EOptimization.Off, "-Od")]
        [CommandLineProcessor.Enum(C.EOptimization.Size, "-O1")]
        [CommandLineProcessor.Enum(C.EOptimization.Speed, "-O2")]
        [CommandLineProcessor.Enum(C.EOptimization.Custom, "")] // deferred for compiler specific optimisation setting
        [VisualStudioProcessor.Enum(C.EOptimization.Off, "Optimization", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "Disabled")]
        [VisualStudioProcessor.Enum(C.EOptimization.Size, "Optimization", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "MinSpace")]
        [VisualStudioProcessor.Enum(C.EOptimization.Speed, "Optimization", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "MaxSpeed")]
        [VisualStudioProcessor.Enum(C.EOptimization.Custom, "Optimization", VisualStudioProcessor.EnumAttribute.EMode.NoOp)] // compiler specific setting
        C.EOptimization? C.ICommonCompilerSettings.Optimization { get; set; }

        // dialects other than C and C++ not supported
        [CommandLineProcessor.Enum(C.ETargetLanguage.C, "-TC")]
        [CommandLineProcessor.Enum(C.ETargetLanguage.Cxx, "-TP")]
        [VisualStudioProcessor.Enum(C.ETargetLanguage.C, "CompileAs", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "CompileAsC")]
        [VisualStudioProcessor.Enum(C.ETargetLanguage.Cxx, "CompileAs", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "CompileAsCpp")]
        C.ETargetLanguage? C.ICommonCompilerSettings.TargetLanguage { get; set; }

        [CommandLineProcessor.Bool("-Oy", "-Oy-")]
        [VisualStudioProcessor.Bool("OmitFramePointers")]
        bool? C.ICommonCompilerSettings.OmitFramePointer { get; set; }

        [CommandLineProcessor.StringArray("-wd")]
        [VisualStudioProcessor.StringArray("DisableSpecificWarnings", inheritExisting: true)]
        Bam.Core.StringArray C.ICommonCompilerSettings.DisableWarnings { get; set; }

        [CommandLineProcessor.StringArray("-U")]
        [VisualStudioProcessor.StringArray("UndefinePreprocessorDefinitions", inheritExisting: true)]
        Bam.Core.StringArray C.ICommonCompilerSettings.PreprocessorUndefines { get; set; }

        [CommandLineProcessor.StringArray("-FI")]
        [VisualStudioProcessor.StringArray("ForcedIncludeFiles", inheritExisting: true)]
        Bam.Core.StringArray C.ICommonCompilerSettings.NamedHeaders { get; set; }

        [CommandLineProcessor.Bool("-E", "-c")]
        [VisualStudioProcessor.Bool("PreprocessToFile")]
        bool? C.ICommonCompilerSettings.PreprocessOnly { get; set; }

        [CommandLineProcessor.StringArray("")]
        [VisualStudioProcessor.StringArray("AdditionalOptions")]
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings { get; set; }

        [CommandLineProcessor.Bool("-nologo", "")]
        [VisualStudioProcessor.Bool("SuppressStartupBanner")]
        bool? ICommonCompilerSettings.NoLogo { get; set; }

        [CommandLineProcessor.Enum(ERuntimeLibrary.MultiThreaded, "-MT")]
        [CommandLineProcessor.Enum(ERuntimeLibrary.MultiThreadedDebug, "-MTd")]
        [CommandLineProcessor.Enum(ERuntimeLibrary.MultiThreadedDLL, "-MD")]
        [CommandLineProcessor.Enum(ERuntimeLibrary.MultiThreadedDebugDLL, "-MDd")]
        [VisualStudioProcessor.Enum(ERuntimeLibrary.MultiThreaded, "RuntimeLibrary", VisualStudioProcessor.EnumAttribute.EMode.AsString)]
        [VisualStudioProcessor.Enum(ERuntimeLibrary.MultiThreadedDebug, "RuntimeLibrary", VisualStudioProcessor.EnumAttribute.EMode.AsString)]
        [VisualStudioProcessor.Enum(ERuntimeLibrary.MultiThreadedDLL, "RuntimeLibrary", VisualStudioProcessor.EnumAttribute.EMode.AsString)]
        [VisualStudioProcessor.Enum(ERuntimeLibrary.MultiThreadedDebugDLL, "RuntimeLibrary", VisualStudioProcessor.EnumAttribute.EMode.AsString)]
        ERuntimeLibrary? ICommonCompilerSettings.RuntimeLibrary { get; set; }

        [CommandLineProcessor.Enum(EWarningLevel.Level0, "-W0")]
        [CommandLineProcessor.Enum(EWarningLevel.Level1, "-W1")]
        [CommandLineProcessor.Enum(EWarningLevel.Level2, "-W2")]
        [CommandLineProcessor.Enum(EWarningLevel.Level3, "-W3")]
        [CommandLineProcessor.Enum(EWarningLevel.Level4, "-W4")]
        [VisualStudioProcessor.Enum(EWarningLevel.Level0, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "TurnOffAllWarnings")]
        [VisualStudioProcessor.Enum(EWarningLevel.Level1, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsIntegerWithPrefix, prefix: "Level")]
        [VisualStudioProcessor.Enum(EWarningLevel.Level2, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsIntegerWithPrefix, prefix: "Level")]
        [VisualStudioProcessor.Enum(EWarningLevel.Level3, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsIntegerWithPrefix, prefix: "Level")]
        [VisualStudioProcessor.Enum(EWarningLevel.Level4, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsIntegerWithPrefix, prefix: "Level")]
        EWarningLevel? ICommonCompilerSettings.WarningLevel { get; set; }

        [CommandLineProcessor.Bool("", "-Za")]
        [VisualStudioProcessor.Bool("DisableLanguageExtensions", inverted: true)]
        bool? ICommonCompilerSettings.EnableLanguageExtensions { get; set; }

        [CommandLineProcessor.Enum(EOptimization.Full, "-Ox")]
        [VisualStudioProcessor.Enum(EOptimization.Full, "Optimization", VisualStudioProcessor.EnumAttribute.EMode.AsString)]
        EOptimization? ICommonCompilerSettings.Optimization { get; set; }

        [CommandLineProcessor.Bool("-bigobj", "")]
        [VisualStudioProcessor.Bool("AdditionalOptions", "-bigobj", "")]
        bool ? ICommonCompilerSettings.IncreaseObjectFileSectionCount { get; set; }

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

        public override void AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }
    }
}

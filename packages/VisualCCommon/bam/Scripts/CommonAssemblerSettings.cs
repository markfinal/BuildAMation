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
namespace VisualCCommon
{
    /// <summary>
    /// Abstract class for common assembler settings
    /// </summary>
    [CommandLineProcessor.OutputPath(C.AssembledObjectFile.ObjectFileKey, "-Fo")]
    [CommandLineProcessor.InputPaths(C.SourceFile.SourceFileKey, "-c ")]
    [VisualStudioProcessor.OutputPath(C.AssembledObjectFile.ObjectFileKey, "ObjectFileName", handledByMetaData: true)] // if deeper than just $(IntDir)myobj.obj, MASM seems to fail
    [VisualStudioProcessor.InputPaths(C.SourceFile.SourceFileKey, "", handledByMetaData: true)]
    abstract class CommonAssemblerSettings :
        C.SettingsBase,
        C.ICommonAssemblerSettings,
        C.IAdditionalSettings,
        ICommonAssemblerSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected CommonAssemblerSettings()
            :
            base(ELayout.Cmds_Outputs_Inputs)
        {}

        // defined in the executable used
        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "")]
        [VisualStudioProcessor.Enum(C.EBit.ThirtyTwo, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.EBit.SixtyFour, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        C.EBit? C.ICommonAssemblerSettings.Bits { get; set; }

        [CommandLineProcessor.Bool("-Zi", "")]
        [VisualStudioProcessor.Bool("GenerateDebugInformation")]
        bool C.ICommonAssemblerSettings.DebugSymbols { get; set; }

        [CommandLineProcessor.Bool("-WX", "")]
        [VisualStudioProcessor.Bool("TreatWarningsAsErrors")]
        bool C.ICommonAssemblerSettings.WarningsAsErrors { get; set; }

        [CommandLineProcessor.PathArray("-I")]
        [VisualStudioProcessor.PathArray("IncludePaths", inheritExisting: true)]
        Bam.Core.TokenizedStringArray C.ICommonAssemblerSettings.IncludePaths { get; set; }

        [CommandLineProcessor.PreprocessorDefines("-D")]
        [VisualStudioProcessor.PreprocessorDefines("PreprocessorDefinitions", inheritExisting: true)]
        C.PreprocessorDefinitions C.ICommonAssemblerSettings.PreprocessorDefines { get; set; }

        [CommandLineProcessor.StringArray("")]
        [VisualStudioProcessor.StringArray("AdditionalOptions")]
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings { get; set; }

        [CommandLineProcessor.Bool("-nologo", "")]
        [VisualStudioProcessor.Bool("NoLogo")]
        bool ICommonAssemblerSettings.NoLogo { get; set; }

        [CommandLineProcessor.Enum(EAssemblerWarningLevel.Level0, "-W0")]
        [CommandLineProcessor.Enum(EAssemblerWarningLevel.Level1, "-W1")]
        [CommandLineProcessor.Enum(EAssemblerWarningLevel.Level2, "-W2")]
        [CommandLineProcessor.Enum(EAssemblerWarningLevel.Level3, "-W3")]
        [VisualStudioProcessor.Enum(EAssemblerWarningLevel.Level0, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsInteger)]
        [VisualStudioProcessor.Enum(EAssemblerWarningLevel.Level1, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsInteger)]
        [VisualStudioProcessor.Enum(EAssemblerWarningLevel.Level2, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsInteger)]
        [VisualStudioProcessor.Enum(EAssemblerWarningLevel.Level3, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsInteger)]
        EAssemblerWarningLevel ICommonAssemblerSettings.WarningLevel { get; set; }

        [CommandLineProcessor.Bool("-safeseh", "")]
        [VisualStudioProcessor.Bool("UseSafeExceptionHandlers")]
        bool ICommonAssemblerSettings.SafeExceptionHandlers { get; set; }

        public override void
        Validate()
        {
            base.Validate();

            if ((this as ICommonAssemblerSettings).SafeExceptionHandlers)
            {
                if ((this.Module as C.CModule).BitDepth != C.EBit.ThirtyTwo)
                {
                    throw new Bam.Core.Exception("Safe exception handlers are only valid in 32-bit");
                }
            }
        }
    }
}

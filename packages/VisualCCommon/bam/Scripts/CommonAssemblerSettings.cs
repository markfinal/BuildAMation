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
namespace VisualCCommon
{
    [CommandLineProcessor.OutputPath(C.AssembledObjectFile.ObjectFileKey, "-Fo")]
    [CommandLineProcessor.InputPaths(C.SourceFile.SourceFileKey, "-c ")]
    [VisualStudioProcessor.OutputPath(C.AssembledObjectFile.ObjectFileKey, "ObjectFileName", handledByMetaData: true)] // if deeper than just $(IntDir)myobj.obj, MASM seems to fail
    [VisualStudioProcessor.InputPaths(C.SourceFile.SourceFileKey, "", handledByMetaData: true)]
    public abstract class CommonAssemblerSettings :
        C.SettingsBase,
#if BAM_V2
#else
        CommandLineProcessor.IConvertToCommandLine,
        VisualStudioProcessor.IConvertToProject,
#endif
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
#endif

#if BAM_V2
        // defined in the executable used
        [VisualStudioProcessor.Enum(C.EBit.ThirtyTwo, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.EBit.SixtyFour, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
#endif
        C.EBit? C.ICommonAssemblerSettings.Bits
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-Zi", "")]
        [VisualStudioProcessor.Bool("GenerateDebugInformation")]
#endif
        bool C.ICommonAssemblerSettings.DebugSymbols
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-WX", "-WX-")]
        [VisualStudioProcessor.Bool("TreatWarningsAsErrors")]
#endif
        bool C.ICommonAssemblerSettings.WarningsAsErrors
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-I")]
        [VisualStudioProcessor.PathArray("IncludePaths", inheritExisting: true)]
#endif
        Bam.Core.TokenizedStringArray C.ICommonAssemblerSettings.IncludePaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PreprocessorDefines("-D")]
        [VisualStudioProcessor.PreprocessorDefines("PreprocessorDefinitions", inheritExisting: true)]
#endif
        C.PreprocessorDefinitions C.ICommonAssemblerSettings.PreprocessorDefines
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
        [VisualStudioProcessor.StringArray("AdditionalOptions")]
#endif
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-nologo", "")]
        [VisualStudioProcessor.Bool("NoLogo")]
#endif
        bool ICommonAssemblerSettings.NoLogo
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(EAssemblerWarningLevel.Level0, "-W0")]
        [CommandLineProcessor.Enum(EAssemblerWarningLevel.Level1, "-W1")]
        [CommandLineProcessor.Enum(EAssemblerWarningLevel.Level2, "-W2")]
        [CommandLineProcessor.Enum(EAssemblerWarningLevel.Level3, "-W3")]
        [VisualStudioProcessor.Enum(EAssemblerWarningLevel.Level0, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsInteger)]
        [VisualStudioProcessor.Enum(EAssemblerWarningLevel.Level1, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsInteger)]
        [VisualStudioProcessor.Enum(EAssemblerWarningLevel.Level2, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsInteger)]
        [VisualStudioProcessor.Enum(EAssemblerWarningLevel.Level3, "WarningLevel", VisualStudioProcessor.EnumAttribute.EMode.AsInteger)]
#endif
        EAssemblerWarningLevel ICommonAssemblerSettings.WarningLevel
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-safeseh", "")]
        [VisualStudioProcessor.Bool("UseSafeExceptionHandlers")]
#endif
        bool ICommonAssemblerSettings.SafeExceptionHandlers
        {
            get;
            set;
        }

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

        public override void
        AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }
    }
}

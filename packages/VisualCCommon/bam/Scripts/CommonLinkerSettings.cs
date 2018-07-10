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
    public abstract class CommonLinkerSettings :
        C.SettingsBase,
#if BAM_V2
#else
        CommandLineProcessor.IConvertToCommandLine,
#endif
        VisualStudioProcessor.IConvertToProject,
        C.ICommonHasOutputPath,
        C.ICommonHasImportLibraryPathWin,
        C.ICommonHasProgramDatabasePathWin,
        C.ICommonLinkerSettingsWin,
        C.ICommonLinkerSettings,
        C.IAdditionalSettings,
        ICommonLinkerSettings
    {
        protected CommonLinkerSettings(
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
#endif

        void
        VisualStudioProcessor.IConvertToProject.Convert(
            Bam.Core.Module module,
            VSSolutionBuilder.VSSettingsGroup vsSettingsGroup,
            string condition)
        {
            VisualStudioProcessor.Conversion.Convert(typeof(VSSolutionImplementation), this, module, vsSettingsGroup, condition);
        }

#if BAM_V2
        [CommandLineProcessor.Path("-OUT:")]
        [VisualStudioProcessor.Path("", ignored: true)]
#endif
        Bam.Core.TokenizedString C.ICommonHasOutputPath.OutputPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-IMPLIB:")]
        [VisualStudioProcessor.Path("", ignored: true)]
#endif
        Bam.Core.TokenizedString C.ICommonHasImportLibraryPathWin.ImportLibraryPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-PDB:")]
        [VisualStudioProcessor.Path("", ignored: true)]
#endif
        Bam.Core.TokenizedString C.ICommonHasProgramDatabasePathWin.PDBPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ESubsystem.NotSet, "")]
        [CommandLineProcessor.Enum(C.ESubsystem.Console, "-SUBSYSTEM:CONSOLE")]
        [CommandLineProcessor.Enum(C.ESubsystem.Windows, "-SUBSYSTEM:WINDOWS")]
        [VisualStudioProcessor.Enum(C.ESubsystem.NotSet, "SubSystem", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.ESubsystem.Console, "SubSystem", VisualStudioProcessor.EnumAttribute.EMode.AsString)]
        [VisualStudioProcessor.Enum(C.ESubsystem.Windows, "SubSystem", VisualStudioProcessor.EnumAttribute.EMode.AsString)]
#endif
        C.ESubsystem? C.ICommonLinkerSettingsWin.SubSystem
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-DEF:")]
        [VisualStudioProcessor.Path("ModuleDefinitionFile")]
#endif
        Bam.Core.TokenizedString C.ICommonLinkerSettingsWin.ExportDefinitionFile
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "-MACHINE:X86")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "-MACHINE:X64")]
        [VisualStudioProcessor.Enum(C.EBit.ThirtyTwo, "TargetMachine", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "MachineX86")]
        [VisualStudioProcessor.Enum(C.EBit.SixtyFour, "TargetMachine", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "MachineX64")]
#endif
        C.EBit C.ICommonLinkerSettings.Bits
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ELinkerOutput.Executable, "")]
        [CommandLineProcessor.Enum(C.ELinkerOutput.DynamicLibrary, "-DLL")]
        [VisualStudioProcessor.Enum(C.ELinkerOutput.Executable, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.ELinkerOutput.DynamicLibrary, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
#endif
        C.ELinkerOutput C.ICommonLinkerSettings.OutputType
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-LIBPATH:")]
        [VisualStudioProcessor.PathArray("AdditionalLibraryDirectories")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettings.LibraryPaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
        [VisualStudioProcessor.StringArray("AdditionalDependencies")]
#endif
        Bam.Core.StringArray C.ICommonLinkerSettings.Libraries
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-DEBUG", "")]
        [VisualStudioProcessor.Bool("GenerateDebugInformation")]
#endif
        bool C.ICommonLinkerSettings.DebugSymbols
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
        [VisualStudioProcessor.StringArray("")]
#endif
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-NOLOGO", "")]
        [VisualStudioProcessor.Bool("SuppressStartupBanner")]
#endif
        bool ICommonLinkerSettings.NoLogo
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-MANIFEST", "-MANIFEST:NO")]
        [VisualStudioProcessor.Bool("EnableManifest", target: VisualStudioProcessor.BaseAttribute.TargetGroup.Configuration)]
#endif
        bool ICommonLinkerSettings.GenerateManifest
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-SAFESEH", "-SAFESEH:NO")]
        [VisualStudioProcessor.Bool("ImageHasSafeExceptionHandlers")]
#endif
        bool ICommonLinkerSettings.SafeExceptionHandlers
        {
            get;
            set;
        }
    }
}

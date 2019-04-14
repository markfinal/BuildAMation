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
    [CommandLineProcessor.OutputPath(C.ConsoleApplication.ExecutableKey, "-OUT:")]
    [CommandLineProcessor.OutputPath(C.ConsoleApplication.PDBKey, "-PDB:")]
    [CommandLineProcessor.OutputPath(C.ConsoleApplication.ImportLibraryKey, "-IMPLIB:")]
    [CommandLineProcessor.InputPaths(C.ObjectFileBase.ObjectFileKey, "")]
    [VisualStudioProcessor.OutputPath(C.ConsoleApplication.ExecutableKey, "OutputFile", enableSideEffets: true)]
    [VisualStudioProcessor.OutputPath(C.ConsoleApplication.PDBKey, "ProgramDatabaseFile")]
    [VisualStudioProcessor.OutputPath(C.ConsoleApplication.ImportLibraryKey, "ImportLibrary")]
    [VisualStudioProcessor.InputPaths(C.ObjectFileBase.ObjectFileKey, "", handledByMetaData: true)]
    public abstract class CommonLinkerSettings :
        C.SettingsBase,
        C.ICommonLinkerSettingsWin,
        C.ICommonLinkerSettings,
        C.IAdditionalSettings,
        ICommonLinkerSettings
    {
        protected CommonLinkerSettings(
            Bam.Core.Module module) => this.InitializeAllInterfaces(module, false, true);

        [CommandLineProcessor.Enum(C.ESubsystem.NotSet, "")]
        [CommandLineProcessor.Enum(C.ESubsystem.Console, "-SUBSYSTEM:CONSOLE")]
        [CommandLineProcessor.Enum(C.ESubsystem.Windows, "-SUBSYSTEM:WINDOWS")]
        [VisualStudioProcessor.Enum(C.ESubsystem.NotSet, "SubSystem", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.ESubsystem.Console, "SubSystem", VisualStudioProcessor.EnumAttribute.EMode.AsString)]
        [VisualStudioProcessor.Enum(C.ESubsystem.Windows, "SubSystem", VisualStudioProcessor.EnumAttribute.EMode.AsString)]
        C.ESubsystem? C.ICommonLinkerSettingsWin.SubSystem { get; set; }

        [CommandLineProcessor.Path("-DEF:")]
        [VisualStudioProcessor.Path("ModuleDefinitionFile")]
        Bam.Core.TokenizedString C.ICommonLinkerSettingsWin.ExportDefinitionFile { get; set; }

        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "-MACHINE:X86")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "-MACHINE:X64")]
        [VisualStudioProcessor.Enum(C.EBit.ThirtyTwo, "TargetMachine", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "MachineX86")]
        [VisualStudioProcessor.Enum(C.EBit.SixtyFour, "TargetMachine", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "MachineX64")]
        C.EBit C.ICommonLinkerSettings.Bits { get; set; }

        [CommandLineProcessor.Enum(C.ELinkerOutput.Executable, "")]
        [CommandLineProcessor.Enum(C.ELinkerOutput.DynamicLibrary, "-DLL")]
        [VisualStudioProcessor.Enum(C.ELinkerOutput.Executable, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        [VisualStudioProcessor.Enum(C.ELinkerOutput.DynamicLibrary, "", VisualStudioProcessor.EnumAttribute.EMode.NoOp)]
        C.ELinkerOutput C.ICommonLinkerSettings.OutputType { get; set; }

        [CommandLineProcessor.PathArray("-LIBPATH:")]
        [VisualStudioProcessor.PathArray("AdditionalLibraryDirectories")]
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettings.LibraryPaths { get; set; }

        [CommandLineProcessor.StringArray("")]
        [VisualStudioProcessor.StringArray("AdditionalDependencies")]
        Bam.Core.StringArray C.ICommonLinkerSettings.Libraries { get; set; }

        [CommandLineProcessor.Bool("-DEBUG", "")]
        [VisualStudioProcessor.Bool("GenerateDebugInformation")]
        bool C.ICommonLinkerSettings.DebugSymbols { get; set; }

        [CommandLineProcessor.StringArray("")]
        [VisualStudioProcessor.StringArray("AdditionalOptions")]
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings { get; set; }

        [CommandLineProcessor.Bool("-NOLOGO", "")]
        [VisualStudioProcessor.Bool("SuppressStartupBanner")]
        bool ICommonLinkerSettings.NoLogo { get; set; }

        [CommandLineProcessor.Bool("-MANIFEST", "-MANIFEST:NO")]
        [VisualStudioProcessor.Bool("EnableManifest", target: VisualStudioProcessor.BaseAttribute.TargetGroup.Configuration)]
        bool ICommonLinkerSettings.GenerateManifest { get; set; }

        [CommandLineProcessor.Bool("-SAFESEH", "-SAFESEH:NO")]
        [VisualStudioProcessor.Bool("ImageHasSafeExceptionHandlers")]
        bool ICommonLinkerSettings.SafeExceptionHandlers { get; set; }

        [CommandLineProcessor.Enum(ELinkTimeCodeGeneration.Off, "-LTCG:OFF")]
        [CommandLineProcessor.Enum(ELinkTimeCodeGeneration.On, "-LTCG")]
        [CommandLineProcessor.Enum(ELinkTimeCodeGeneration.Incremental, "-LTCG:incremental")]
        [VisualStudioProcessor.Enum(ELinkTimeCodeGeneration.Off, "LinkTimeCodeGeneration", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString:"Default")]
        [VisualStudioProcessor.Enum(ELinkTimeCodeGeneration.On, "LinkTimeCodeGeneration", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "UseLinkTimeCodeGeneration")]
        [VisualStudioProcessor.Enum(ELinkTimeCodeGeneration.Incremental, "LinkTimeCodeGeneration", VisualStudioProcessor.EnumAttribute.EMode.VerbatimString, verbatimString: "UseFastLinkTimeCodeGeneration")]
        ELinkTimeCodeGeneration ICommonLinkerSettings.LinkTimeCodeGeneration { get; set; }

        public override void AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }
    }
}

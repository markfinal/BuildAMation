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
        CommandLineProcessor.IConvertToCommandLine,
        VisualStudioProcessor.IConvertToProject,
        C.ICommonHasOutputPath,
        C.ICommonHasImportLibraryPathWin,
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
        [CommandLineProcessor.Path("-OUT:")]
#endif
        Bam.Core.TokenizedString C.ICommonHasOutputPath.OutputPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-IMPLIB:")]
#endif
        Bam.Core.TokenizedString C.ICommonHasImportLibraryPathWin.ImportLibraryPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.EnumAttribute(C.ESubsystem.NotSet, "")]
        [CommandLineProcessor.EnumAttribute(C.ESubsystem.Console, "-SUBSYSTEM:CONSOLE")]
        [CommandLineProcessor.EnumAttribute(C.ESubsystem.Windows, "-SUBSYSTEM:WINDOWS")]
#endif
        C.ESubsystem? C.ICommonLinkerSettingsWin.SubSystem
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-DEF:")]
#endif
        Bam.Core.TokenizedString C.ICommonLinkerSettingsWin.ExportDefinitionFile
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "-MACHINE:X86")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "-MACHINE:X64")]
#endif
        C.EBit C.ICommonLinkerSettings.Bits
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ELinkerOutput.Executable, "")]
        [CommandLineProcessor.Enum(C.ELinkerOutput.DynamicLibrary, "-DLL")]
#endif
        C.ELinkerOutput C.ICommonLinkerSettings.OutputType
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-LIBPATH:")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettings.LibraryPaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
#endif
        Bam.Core.StringArray C.ICommonLinkerSettings.Libraries
        {
            get;
            set;
        }

        bool C.ICommonLinkerSettings.DebugSymbols
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
#endif
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-NOLOGO", "")]
#endif
        bool ICommonLinkerSettings.NoLogo
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-MANIFEST", "-MANIFEST:NO")]
#endif
        bool ICommonLinkerSettings.GenerateManifest
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-SAFESEH", "-SAFESEH:NO")]
#endif
        bool ICommonLinkerSettings.SafeExceptionHandlers
        {
            get;
            set;
        }
    }
}

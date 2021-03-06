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
namespace MingwCommon
{
    /// <summary>
    /// Abstract class representing the common Mingw linker settings
    /// </summary>
    [CommandLineProcessor.OutputPath(C.ConsoleApplication.ExecutableKey, "-o ")]
    [CommandLineProcessor.OutputPath(C.ConsoleApplication.ImportLibraryKey, "-Wl,--out-implib,")]
    [CommandLineProcessor.InputPaths(C.ObjectFileBase.ObjectFileKey, "")]
    abstract class CommonLinkerSettings :
        C.SettingsBase,
        C.ICommonLinkerSettingsWin,
        C.ICommonLinkerSettings,
        C.IAdditionalSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        protected CommonLinkerSettings()
            :
            base(ELayout.Inputs_Cmds_Outputs)
        {}

        [CommandLineProcessor.Enum(C.ESubsystem.NotSet, "")]
        [CommandLineProcessor.Enum(C.ESubsystem.Console, "-Wl,-subsystem,console")]
        [CommandLineProcessor.Enum(C.ESubsystem.Windows, "-Wl,-subsystem,windows")]
        C.ESubsystem? C.ICommonLinkerSettingsWin.SubSystem { get; set; }

        [CommandLineProcessor.Path("")] // just add it to the command line
        Bam.Core.TokenizedString C.ICommonLinkerSettingsWin.ExportDefinitionFile { get; set; }

        [CommandLineProcessor.Enum(C.EBit.ThirtyTwo, "-m32")]
        [CommandLineProcessor.Enum(C.EBit.SixtyFour, "-m64")]
        C.EBit C.ICommonLinkerSettings.Bits { get; set; }

        [CommandLineProcessor.Enum(C.ELinkerOutput.Executable, "")]
        [CommandLineProcessor.Enum(C.ELinkerOutput.DynamicLibrary, "-shared")]
        C.ELinkerOutput C.ICommonLinkerSettings.OutputType { get; set; }

        [CommandLineProcessor.PathArray("-L")]
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettings.LibraryPaths { get; set; }

        [CommandLineProcessor.StringArray("")]
        Bam.Core.StringArray C.ICommonLinkerSettings.Libraries { get; set; }

        [CommandLineProcessor.Bool("-g", "")]
        bool C.ICommonLinkerSettings.DebugSymbols { get; set; }

        [CommandLineProcessor.StringArray("")]
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings { get; set; }
    }
}

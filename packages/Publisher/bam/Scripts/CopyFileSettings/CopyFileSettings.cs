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
namespace Publisher
{
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedFileKey, "")]
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedDirectoryKey, "")]
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedRenamedDirectoryKey, "")]
    [CommandLineProcessor.InputPaths(C.ConsoleApplication.ExecutableKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(C.SharedObjectSymbolicLink.SOSymLinkKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(PreExistingFile.ExistingFileKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(PreExistingDirectory.ExistingDirectoryKey, "", max_file_count: 1)] // TODO path modifier, add /* upon rename
    [CommandLineProcessor.InputPaths(CollatedObject.CopiedFileKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(CollatedObject.CopiedDirectoryKey, "", max_file_count: 1)]
    public sealed class PosixCopyFileSettings :
        Bam.Core.Settings,
#if BAM_V2
#else
        CommandLineProcessor.IConvertToCommandLine,
#endif
        ICopyFileSettings
    {
        public PosixCopyFileSettings()
        {}

        public PosixCopyFileSettings(
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

#if BAM_V2
        [CommandLineProcessor.Bool("-f", "")]
#endif
        bool ICopyFileSettings.Force
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-v", "")]
#endif
        bool ICopyFileSettings.Verbose
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-R", "")]
#endif
        bool ICopyFileSettings.Recursive
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-a", "")]
#endif
        bool ICopyFileSettings.PreserveAllAttributes
        {
            get;
            set;
        }

        public override void AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Inputs_Outputs;
        }
    }

    [CommandLineProcessor.OutputPath(CollatedObject.CopiedFileKey, "", path_modifier: "@dir($(0))/")]
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedDirectoryKey, "", path_modifier: "$(0)/")]
    [CommandLineProcessor.OutputPath(CollatedObject.CopiedRenamedDirectoryKey, "", path_modifier: "$(0)/")]
    [CommandLineProcessor.InputPaths(C.ConsoleApplication.ExecutableKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(C.ConsoleApplication.PDBKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(C.ConsoleApplication.ImportLibraryKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(PreExistingFile.ExistingFileKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(PreExistingDirectory.ExistingDirectoryKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(CollatedObject.CopiedFileKey, "", max_file_count: 1)]
    [CommandLineProcessor.InputPaths(CollatedObject.CopiedDirectoryKey, "", max_file_count: 1)]
    public sealed class WinCopyFileSettings :
        Bam.Core.Settings,
#if BAM_V2
#else
        CommandLineProcessor.IConvertToCommandLine,
#endif
        ICopyFileSettings
    {
        public WinCopyFileSettings()
        { }

        public WinCopyFileSettings(
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

#if BAM_V2
        [CommandLineProcessor.Bool("/Y /R", "")]
#endif
        bool ICopyFileSettings.Force
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("/F", "")]
#endif
        bool ICopyFileSettings.Verbose
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("/S", "")]
#endif
        bool ICopyFileSettings.Recursive
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("/K /B", "")]
#endif
        bool ICopyFileSettings.PreserveAllAttributes
        {
            get;
            set;
        }

        public override void AssignFileLayout()
        {
            this.FileLayout = ELayout.Inputs_Outputs_Cmds;
        }
    }
}

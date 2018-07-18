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
namespace ClangCommon
{
    [CommandLineProcessor.OutputPath(C.StaticLibrary.LibraryKey, "")]
    [CommandLineProcessor.InputPaths(C.ObjectFileBase.ObjectFileKey, "")]
    public abstract class CommonArchiverSettings :
        C.SettingsBase,
#if BAM_V2
#else
        CommandLineProcessor.IConvertToCommandLine,
        // TODO: Xcode translation
#endif
#if false
        C.ICommonHasOutputPath,
#endif
        C.IAdditionalSettings,
        ICommonArchiverSettings
    {
        protected CommonArchiverSettings(
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
            CommandLineProcessor.Conversion.Convert(typeof(CommandLineArchiverImplementation), this, commandLine);
        }
#endif

#if false
#if BAM_V2
        [CommandLineProcessor.Path("")]
        [XcodeProjectProcessor.Path("", ignore: true)]
#endif
        string C.ICommonHasOutputPath.OutputPath
        {
            get;
            set;
        }
#endif

#if BAM_V2
        [CommandLineProcessor.Bool("-s", "")]
        [XcodeProjectProcessor.UniqueBool("", "", "", ignore: true)]
#endif
        bool ICommonArchiverSettings.Ranlib
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-c", "")]
        [XcodeProjectProcessor.UniqueBool("", "", "", ignore: true)]
#endif
        bool ICommonArchiverSettings.DoNotWarnIfLibraryCreated
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
        [XcodeProjectProcessor.StringArray("", ignore: true)]
#endif
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(EArchiverCommand.Replace, "-r")]
        [XcodeProjectProcessor.UniqueEnum(EArchiverCommand.Replace, "", "", ignore: true)]
#endif
        EArchiverCommand ICommonArchiverSettings.Command
        {
            get;
            set;
        }

        public override void
        AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }
    }
}

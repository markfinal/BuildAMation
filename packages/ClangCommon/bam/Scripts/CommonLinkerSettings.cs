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
    public abstract class CommonLinkerSettings :
        C.SettingsBase,
        CommandLineProcessor.IConvertToCommandLine,
        XcodeProjectProcessor.IConvertToProject,
        C.ICommonHasOutputPath,
        C.ICommonLinkerSettings,
        C.ICommonLinkerSettingsOSX,
        C.IAdditionalSettings,
        ICommonLinkerSettings
    {
        protected CommonLinkerSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);

            // not in the defaults in the C package to avoid a compile-time dependency on the Clang package
            (this as C.ICommonLinkerSettingsOSX).MacOSMinimumVersionSupported =
                Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang").MinimumVersionSupported;
        }

        void
        CommandLineProcessor.IConvertToCommandLine.Convert(
            Bam.Core.StringArray commandLine)
        {
            CommandLineProcessor.Conversion.Convert(typeof(CommandLineLinkerImplementation), this, commandLine);
        }

        void
        XcodeProjectProcessor.IConvertToProject.Convert(
            Bam.Core.Module module,
            XcodeBuilder.Configuration configuration)
        {
            XcodeProjectProcessor.Conversion.Convert(typeof(XcodeLinkerImplementation), this, module, configuration);
        }

#if BAM_V2
        [CommandLineProcessor.Path("-o ")]
#endif
        Bam.Core.TokenizedString C.ICommonHasOutputPath.OutputPath
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.EnumAttribute(C.EBit.ThirtyTwo, "-arch i386")]
        [CommandLineProcessor.EnumAttribute(C.EBit.SixtyFour, "-arch x86_64")]
#endif
        C.EBit C.ICommonLinkerSettings.Bits
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ELinkerOutput.Executable, "")]
        [CommandLineProcessor.Enum(C.ELinkerOutput.DynamicLibrary, "-dynamiclib")]
#endif
        C.ELinkerOutput C.ICommonLinkerSettings.OutputType
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-L")]
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

#if BAM_V2
        [CommandLineProcessor.Bool("-g", "")]
#endif
        bool C.ICommonLinkerSettings.DebugSymbols
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-framework ")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettingsOSX.Frameworks
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-F ")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettingsOSX.FrameworkSearchPaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-Wl,-dylib_install_name,")]
#endif
        Bam.Core.TokenizedString C.ICommonLinkerSettingsOSX.InstallName
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.String("-mmacosx-version-min=")]
#endif
        string C.ICommonLinkerSettingsOSX.MacOSMinimumVersionSupported
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
        [CommandLineProcessor.PathArray("-Wl,-rpath,{0}")]
#endif
        Bam.Core.TokenizedStringArray ICommonLinkerSettings.RPath
        {
            get;
            set;
        }

        public override void
        Validate()
        {
            base.Validate();
            if (null != (this as C.ICommonLinkerSettingsOSX).InstallName)
            {
                if (!(this.Module is C.DynamicLibrary))
                {
                    throw new Bam.Core.Exception("Install name is only applicable to dynamic libraries");
                }
            }
        }
    }
}

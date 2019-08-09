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
namespace ClangCommon
{
    [CommandLineProcessor.OutputPath(C.ConsoleApplication.ExecutableKey, "-o ")]
    [CommandLineProcessor.InputPaths(C.ObjectFileBase.ObjectFileKey, "")]
    public abstract class CommonLinkerSettings :
        C.SettingsBase,
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
                Bam.Core.Graph.Instance.PackageMetaData<Clang.MetaData>("Clang").MacOSXMinimumVersionSupported;
        }

        [CommandLineProcessor.EnumAttribute(C.EBit.ThirtyTwo, "-arch i386")]
        [CommandLineProcessor.EnumAttribute(C.EBit.SixtyFour, "-arch x86_64")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.ThirtyTwo, "VALID_ARCHS", "i386", "ARCHS", "$(ARCHS_STANDARD_32_BIT)")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.SixtyFour, "VALID_ARCHS", "x86_64", "ARCHS", "$(ARCHS_STANDARD_64_BIT)")]
        C.EBit C.ICommonLinkerSettings.Bits { get; set; }

        [CommandLineProcessor.Enum(C.ELinkerOutput.Executable, "")]
        [CommandLineProcessor.Enum(C.ELinkerOutput.DynamicLibrary, "-dynamiclib")]
        [XcodeProjectProcessor.UniqueEnum(C.ELinkerOutput.Executable, "", "", ignore: true)] // EXECUTABLE_PREFIX and EXECUTABLE_EXTENSION handled in metadata
        [XcodeProjectProcessor.UniqueEnum(C.ELinkerOutput.DynamicLibrary, "", "", ignore: true)]
        C.ELinkerOutput C.ICommonLinkerSettings.OutputType { get; set; }

        [CommandLineProcessor.PathArray("-L")]
        [XcodeProjectProcessor.PathArray("LIBRARY_SEARCH_PATHS")]
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettings.LibraryPaths { get; set; }

        [CommandLineProcessor.StringArray("")]
        [XcodeProjectProcessor.LibraryArray()]
        Bam.Core.StringArray C.ICommonLinkerSettings.Libraries { get; set; }

        [CommandLineProcessor.Bool("-g", "")]
        [XcodeProjectProcessor.MultiBool("OTHER_LDFLAGS", "-g", "")]
        bool C.ICommonLinkerSettings.DebugSymbols { get; set; }

        [CommandLineProcessor.FrameworkArray("-framework ")]
        [XcodeProjectProcessor.FrameworkArray()]
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettingsOSX.Frameworks { get; set; }

        [CommandLineProcessor.PathArray("-F ")]
        [XcodeProjectProcessor.PathArray("FRAMEWORK_SEARCH_PATHS")]
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettingsOSX.FrameworkSearchPaths { get; set; }

        [CommandLineProcessor.Path("-Wl,-dylib_install_name,")]
        [XcodeProjectProcessor.Path("LD_DYLIB_INSTALL_NAME")]
        Bam.Core.TokenizedString C.ICommonLinkerSettingsOSX.InstallName { get; set; }

        [CommandLineProcessor.String("-mmacosx-version-min=")]
        [XcodeProjectProcessor.String("", ignore: true)] // dealt with separately
        string C.ICommonLinkerSettingsOSX.MacOSMinimumVersionSupported { get; set; }

        [CommandLineProcessor.Path("-current_version ")]
        [XcodeProjectProcessor.Path("DYLIB_CURRENT_VERSION")]
        Bam.Core.TokenizedString C.ICommonLinkerSettingsOSX.CurrentVersion { get; set; }

        [CommandLineProcessor.Path("-compatibility_version ")]
        [XcodeProjectProcessor.Path("DYLIB_COMPATIBILITY_VERSION")]
        Bam.Core.TokenizedString C.ICommonLinkerSettingsOSX.CompatibilityVersion { get; set; }

        [CommandLineProcessor.StringArray("")]
        [XcodeProjectProcessor.StringArray("OTHER_LDFLAGS", spacesSeparate: true)]
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings { get; set; }

        [CommandLineProcessor.PathArray("-Wl,-rpath,")]
        [XcodeProjectProcessor.PathArray("LD_RUNPATH_SEARCH_PATHS", prefixWithSrcRoot: false)]
        Bam.Core.TokenizedStringArray ICommonLinkerSettings.RPath { get; set; }

        [CommandLineProcessor.Path("-Wl,-exported_symbols_list,")]
        [XcodeProjectProcessor.Path("EXPORTED_SYMBOLS_FILE")]
        Bam.Core.TokenizedString ICommonLinkerSettings.ExportedSymbolList { get; set; }

        public override void
        Validate()
        {
            base.Validate();
            if (null != (this as C.ICommonLinkerSettingsOSX).InstallName)
            {
                if (!(this.Module is C.IDynamicLibrary))
                {
                    throw new Bam.Core.Exception(
                        $"Install name is only applicable to dynamic libraries; trying to apply to {this.Module.ToString()}"
                    );
                }
            }
        }

        /// <summary>
        /// Set the layout how command lines are constructed
        /// </summary>
        public override void
        AssignFileLayout()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }
    }
}

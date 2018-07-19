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
    [CommandLineProcessor.OutputPath(C.ConsoleApplication.ExecutableKey, "-o ")]
    [CommandLineProcessor.InputPaths(C.ObjectFileBase.ObjectFileKey, "")]
    public abstract class CommonLinkerSettings :
        C.SettingsBase,
#if BAM_V2
#else
        CommandLineProcessor.IConvertToCommandLine,
        XcodeProjectProcessor.IConvertToProject,
#endif
#if false
        C.ICommonHasOutputPath,
#endif
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

#if BAM_V2
#else
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
#endif

#if false
#if BAM_V2
        [CommandLineProcessor.Path("-o ")]
        [XcodeProjectProcessor.Path("", ignore: true)]
#endif
        string C.ICommonHasOutputPath.OutputPath
        {
            get;
            set;
        }
#endif

#if BAM_V2
        [CommandLineProcessor.EnumAttribute(C.EBit.ThirtyTwo, "-arch i386")]
        [CommandLineProcessor.EnumAttribute(C.EBit.SixtyFour, "-arch x86_64")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.ThirtyTwo, "VALID_ARCHS", "i386", "ARCHS", "$(ARCHS_STANDARD_32_BIT)")]
        [XcodeProjectProcessor.UniqueEnum(C.EBit.SixtyFour, "VALID_ARCHS", "x86_64", "ARCHS", "$(ARCHS_STANDARD_64_BIT)")]
#endif
        C.EBit C.ICommonLinkerSettings.Bits
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Enum(C.ELinkerOutput.Executable, "")]
        [CommandLineProcessor.Enum(C.ELinkerOutput.DynamicLibrary, "-dynamiclib")]
        [XcodeProjectProcessor.UniqueEnum(C.ELinkerOutput.Executable, "EXECUTABLE_PREFIX", "", "EXECUTABLE_EXTENSION", "")] // TODO: should really use $(exeext)
        [XcodeProjectProcessor.UniqueEnum(C.ELinkerOutput.DynamicLibrary, "EXECUTABLE_PREFIX", "lib", "EXECUTABLE_EXTENSION", "dylib")] // TODO: should really use $(dynamicprefix) and $(dynamicextonly)
        // TODO: should we have a Plugin on the enum?
        // TODO: current version and compatibility version
#endif
        C.ELinkerOutput C.ICommonLinkerSettings.OutputType
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-L")]
        [XcodeProjectProcessor.PathArray("LIBRARY_SEARCH_PATHS")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettings.LibraryPaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
        [XcodeProjectProcessor.LibraryArray()]
#endif
        Bam.Core.StringArray C.ICommonLinkerSettings.Libraries
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Bool("-g", "")]
        [XcodeProjectProcessor.MultiBool("OTHER_LDFLAGS", "-g", "")]
#endif
        bool C.ICommonLinkerSettings.DebugSymbols
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-framework ")]
        [XcodeProjectProcessor.FrameworkArray()]
#endif
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettingsOSX.Frameworks
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-F ")]
        [XcodeProjectProcessor.PathArray("FRAMEWORK_SEARCH_PATHS")]
#endif
        Bam.Core.TokenizedStringArray C.ICommonLinkerSettingsOSX.FrameworkSearchPaths
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.Path("-Wl,-dylib_install_name,")]
        [XcodeProjectProcessor.Path("LD_DYLIB_INSTALL_NAME")]
#endif
        Bam.Core.TokenizedString C.ICommonLinkerSettingsOSX.InstallName
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.String("-mmacosx-version-min=")]
        [XcodeProjectProcessor.String("", ignore: true)] // dealt with separately
#endif
        string C.ICommonLinkerSettingsOSX.MacOSMinimumVersionSupported
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.StringArray("")]
        [XcodeProjectProcessor.StringArray("OTHER_LDFLAGS", spacesSeparate: true)]
#endif
        Bam.Core.StringArray C.IAdditionalSettings.AdditionalSettings
        {
            get;
            set;
        }

#if BAM_V2
        [CommandLineProcessor.PathArray("-Wl,-rpath,")]
        [XcodeProjectProcessor.PathArray("LD_RUNPATH_SEARCH_PATHS")]
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
                if (!(this.Module is C.IDynamicLibrary))
                {
                    throw new Bam.Core.Exception(
                        "Install name is only applicable to dynamic libraries; trying to apply to {0}",
                        this.Module.ToString()
                    );
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

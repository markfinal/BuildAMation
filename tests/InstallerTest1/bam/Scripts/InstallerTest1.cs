#region License
// Copyright (c) 2010-2016, Mark Final
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
using Bam.Core;
namespace InstallerTest1
{
    public sealed class StaticLibrary :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/source/staticlib/*.h");
            this.CreateCxxSourceContainer("$(packagedir)/source/staticlib/*.cpp");

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                if (null != compiler)
                {
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/source/staticlib"));
                }
            });
        }
    }

    public sealed class DynamicLibrary :
        C.Cxx.DynamicLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.CreateHeaderContainer("$(packagedir)/source/dynamiclib/*.h");
            this.CreateCxxSourceContainer("$(packagedir)/source/dynamiclib/*.cpp");

            this.PublicPatch((settings, appliedTo) =>
            {
                var compiler = settings as C.ICommonCompilerSettings;
                if (null != compiler)
                {
                    compiler.IncludePaths.Add(this.CreateTokenizedString("$(packagedir)/source/dynamiclib"));
                }
            });

            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }
        }
    }

    public sealed class Executable :
        C.Cxx.GUIApplication
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(packagedir)/source/*.cpp");

            this.CompileAndLinkAgainst<StaticLibrary>(source);
            this.CompileAndLinkAgainst<DynamicLibrary>(source);

            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows) &&
                this.Linker is VisualCCommon.LinkerBase)
            {
                this.LinkAgainst<WindowsSDK.WindowsSDK>();
            }

            this.PrivatePatch(settings =>
                {
                    var gccCommon = settings as GccCommon.ICommonLinkerSettings;
                    if (null != gccCommon)
                    {
                        gccCommon.CanUseOrigin = true;
                        gccCommon.RPath.AddUnique("$ORIGIN");
                    }
                });
        }
    }

    public sealed class ExecutableRuntime :
        Publisher.Collation
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var app = this.Include<Executable>(C.GUIApplication.Key, EPublishingType.WindowedApplication);
            this.Include<DynamicLibrary>(C.DynamicLibrary.Key, ".", app);

            // copy the required runtime library next to the binary
            if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Windows) &&
                this.BuildEnvironment.Configuration != EConfiguration.Debug &&
                (app.SourceModule as Executable).Linker is VisualCCommon.LinkerBase)
            {
                var runtimeLibrary = Bam.Core.Graph.Instance.PackageMetaData<VisualCCommon.IRuntimeLibraryPathMeta>("VisualC");
                this.IncludeFile(runtimeLibrary.MSVCR((app.SourceModule as Executable).BitDepth), ".", app);
                this.IncludeFile(runtimeLibrary.MSVCP((app.SourceModule as Executable).BitDepth), ".", app);
            }
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class ExecutableDebugSymbols :
        Publisher.DebugSymbolCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.CreateSymbolsFrom<ExecutableRuntime>();
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class ExecutableStripped :
        Publisher.StrippedBinaryCollation
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.StripBinariesFrom<ExecutableRuntime, ExecutableDebugSymbols>();
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class ExecutableInnoSetup :
        Installer.InnoSetupInstaller
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = TokenizedString.CreateVerbatim("InnoSetupInstaller");

            this.SourceFolder<ExecutableStripped>(Publisher.StrippedBinaryCollation.Key);
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class ExecutableNSIS :
        Installer.NSISInstaller
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = TokenizedString.CreateVerbatim("NSISInstaller");

            this.SourceFolder<ExecutableStripped>(Publisher.StrippedBinaryCollation.Key);
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class ExecutableTarBall :
        Installer.TarBall
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = TokenizedString.CreateVerbatim("TarBallInstaller");

            this.SourceFolder<ExecutableStripped>(Publisher.StrippedBinaryCollation.Key);

            this.PrivatePatch(settings =>
                {
                    var tarSettings = settings as Installer.ITarBallSettings;
                    tarSettings.CompressionType = Installer.ETarCompressionType.gzip;
                });
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class ExecutableDMG :
        Installer.DiskImage
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = TokenizedString.CreateVerbatim("DiskImageInstaller");

            this.SourceFolder<ExecutableStripped>(Publisher.StrippedBinaryCollation.Key);
        }
    }

    // TODO: there is no equivalent on Windows
    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class ExecutableDebugSymbolsTarBall :
        Installer.TarBall
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = TokenizedString.CreateVerbatim("SymbolsTarBall");

            this.SourceFolder<ExecutableDebugSymbols>(Publisher.DebugSymbolCollation.Key);

            this.PrivatePatch(settings =>
                {
                    var tarSettings = settings as Installer.ITarBallSettings;
                    tarSettings.CompressionType = Installer.ETarCompressionType.gzip;
                });
        }
    }
}

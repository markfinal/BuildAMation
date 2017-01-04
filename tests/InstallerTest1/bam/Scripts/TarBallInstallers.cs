#region License
// Copyright (c) 2010-2017, Mark Final
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
    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class CExecutableTarBall :
        Installer.TarBall
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = TokenizedString.CreateVerbatim("C_TarBallInstaller");

            this.SourceFolder<CExecutableStripped>(Publisher.StrippedBinaryCollation.Key);

            this.PrivatePatch(settings =>
                {
                    var tarSettings = settings as Installer.ITarBallSettings;
                    tarSettings.CompressionType = Installer.ETarCompressionType.gzip;
                    if (this.BuildEnvironment.Platform.Includes(Bam.Core.EPlatform.Linux))
                    {
                        tarSettings.TransformRegEx = "'s,^.,toplevelfolder,'";
                    }
                });
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class CxxExecutableTarBall :
        Installer.TarBall
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = TokenizedString.CreateVerbatim("Cxx_TarBallInstaller");

            this.SourceFolder<CxxExecutableStripped>(Publisher.StrippedBinaryCollation.Key);

            this.PrivatePatch(settings =>
            {
                var tarSettings = settings as Installer.ITarBallSettings;
                tarSettings.CompressionType = Installer.ETarCompressionType.gzip;
            });
        }
    }

    // TODO: tarball creation is not available on Windows, and currently there is no zip
    // file creation, so Windows is missing a PDB archive
    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class CExecutableDebugSymbolsTarBall :
        Installer.TarBall
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = TokenizedString.CreateVerbatim("C_SymbolsTarBall");

            this.SourceFolder<CExecutableDebugSymbols>(Publisher.DebugSymbolCollation.Key);

            this.PrivatePatch(settings =>
                {
                    var tarSettings = settings as Installer.ITarBallSettings;
                    tarSettings.CompressionType = Installer.ETarCompressionType.gzip;
                });
        }
    }

    [Bam.Core.ConfigurationFilter(Bam.Core.EConfiguration.NotDebug)]
    sealed class CxxExecutableDebugSymbolsTarBall :
        Installer.TarBall
    {
        protected override void
        Init(
            Bam.Core.Module parent)
        {
            base.Init(parent);

            this.Macros["OutputName"] = TokenizedString.CreateVerbatim("Cxx_SymbolsTarBall");

            this.SourceFolder<CxxExecutableDebugSymbols>(Publisher.DebugSymbolCollation.Key);

            this.PrivatePatch(settings =>
            {
                var tarSettings = settings as Installer.ITarBallSettings;
                tarSettings.CompressionType = Installer.ETarCompressionType.gzip;
            });
        }
    }
}

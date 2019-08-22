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
using Bam.Core;
namespace Installer
{
    /// <summary>
    /// Settings class for hdiutil
    /// </summary>
    [CommandLineProcessor.OutputPath(DiskImage.DMGKey, "-o ")]
    [CommandLineProcessor.InputPaths(Publisher.StrippedBinaryCollation.StripBinaryDirectoryKey, "-srcfolder ")]
    public sealed class DiskImageSettings :
        Bam.Core.Settings,
        IDiskImageSettings
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public DiskImageSettings()
            :
            base(ELayout.Cmds_Inputs_Outputs)
        {}

        [CommandLineProcessor.Enum(EDiskImageFormat.UDRW, "-format UDRW")]
        [CommandLineProcessor.Enum(EDiskImageFormat.UDRO, "-format UDRO")]
        [CommandLineProcessor.Enum(EDiskImageFormat.UDCO, "-format UDCO")]
        [CommandLineProcessor.Enum(EDiskImageFormat.UDZO, "-format UDZO")]
        [CommandLineProcessor.Enum(EDiskImageFormat.ULFO, "-format ULFO")]
        [CommandLineProcessor.Enum(EDiskImageFormat.UDBZ, "-format UDBZ")]
        [CommandLineProcessor.Enum(EDiskImageFormat.UDTO, "-format UDTO")]
        [CommandLineProcessor.Enum(EDiskImageFormat.UDSP, "-format UDSP")]
        [CommandLineProcessor.Enum(EDiskImageFormat.UDSB, "-format UDSB")]
        [CommandLineProcessor.Enum(EDiskImageFormat.UFBI, "-format UFBI")]
        EDiskImageFormat IDiskImageSettings.Format { get; set; }

        [CommandLineProcessor.Enum(EDiskImageVerb.Create, "create")]
        EDiskImageVerb IDiskImageSettings.Verb { get; set; }

        [CommandLineProcessor.Enum(EDiskImageVerbosity.Default, "")]
        [CommandLineProcessor.Enum(EDiskImageVerbosity.Quiet, "-quiet")]
        [CommandLineProcessor.Enum(EDiskImageVerbosity.Verbose, "-verbose")]
        [CommandLineProcessor.Enum(EDiskImageVerbosity.Debug, "-debugs")]
        EDiskImageVerbosity IDiskImageSettings.Verbosity { get; set; }

        [CommandLineProcessor.String("-size ")]
        string IDiskImageSettings.ImageSize { get; set; }
    }
}

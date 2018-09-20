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
using Bam.Core;
namespace Installer
{
    [CommandLineProcessor.OutputPath(TarBall.TarBallKey, "-f ")]
    [CommandLineProcessor.InputPaths(Publisher.StrippedBinaryCollation.StripBinaryDirectoryKey, "")]
    [CommandLineProcessor.InputPaths(Publisher.DebugSymbolCollation.DebugSymbolsDirectoryKey, "")]
    public sealed class TarBallSettings :
        Bam.Core.Settings,
        ITarBallSettings
    {
        public TarBallSettings(
            Bam.Core.Module module)
        {
            this.InitializeAllInterfaces(module, false, true);
        }

        [CommandLineProcessor.Enum(ETarOperation.Create, "-c")]
        ETarOperation ITarBallSettings.Operation
        {
            get;
            set;
        }

        private ETarCompressionType _compression;
        [CommandLineProcessor.Enum(ETarCompressionType.None, "")]
        [CommandLineProcessor.Enum(ETarCompressionType.gzip, "-z")]
        [CommandLineProcessor.Enum(ETarCompressionType.bzip, "-j")]
        [CommandLineProcessor.Enum(ETarCompressionType.lzma, "--lzma")]
        ETarCompressionType ITarBallSettings.CompressionType
        {
            get
            {
                return this._compression;
            }
            set
            {
                this._compression = value;
                switch (value)
                {
                case ETarCompressionType.None:
                    this.Module.Macros.AddVerbatim("tarext", ".tar");
                    break;

                case ETarCompressionType.gzip:
                    this.Module.Macros.AddVerbatim("tarext", ".tgz");
                    break;

                case ETarCompressionType.bzip:
                    this.Module.Macros.AddVerbatim("tarext", ".tar.bz2");
                    break;

                case ETarCompressionType.lzma:
                    this.Module.Macros.AddVerbatim("tarext", ".tar.lzma");
                    break;

                default:
                    throw new Bam.Core.Exception("Unknown tar compression, {0}", value.ToString());
                }
            }
        }

        [CommandLineProcessor.String("--transform=")]
        string ITarBallSettings.TransformRegEx
        {
            get;
            set;
        }


        [CommandLineProcessor.Bool("-v", "")]
        bool ITarBallSettings.Verbose
        {
            get;
            set;
        }

        public override void AssignFileLayout ()
        {
            this.FileLayout = ELayout.Cmds_Outputs_Inputs;
        }
    }
}
